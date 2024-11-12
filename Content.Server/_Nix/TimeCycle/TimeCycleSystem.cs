using Robust.Shared.Timing;
using Robust.Shared.Prototypes;
using Robust.Shared.Map.Components;
using Content.Shared._Nix.TimeCycle;

namespace Content.Server._Nix.TimeCycle;

public sealed partial class TimeCycleSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public double TimeCycleMilliseconds = TimeSpan.FromHours(24).TotalMilliseconds;

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(float frameTime)
    {
        var curTime = _gameTiming.CurTime;
        var query = EntityQueryEnumerator<TimeCycleComponent, MapLightComponent>();

        while (query.MoveNext(out var uid, out var timeComp, out var mapLightComp))
        {
            if (timeComp.Paused)
                continue;
            if (curTime < timeComp.DelayTime)
                continue;

            if (timeComp.SpeedUp)
                timeComp.DelayTime = curTime + TimeSpan.FromMilliseconds(10);
            else
                timeComp.DelayTime = curTime + timeComp.TimeLength;
            timeComp.CurrentTime += TimeSpan.FromMinutes(1);

            if (!_prototypeManager.TryIndex(timeComp.Palette, out TimeCyclePalettePrototype? timeColors))
                continue;
            if (timeColors is null)
                continue;

            var timeInCycle = GetTimeInCycle(timeComp.CurrentTime);
            var (colorStart, colorEnd, coef) = GetInterpolate(timeColors, timeInCycle);
            mapLightComp.AmbientLightColor = Color.InterpolateBetween(colorStart, colorEnd, coef);
            Dirty(uid, mapLightComp);
        }

        base.Update(frameTime);
    }

    public TimeSpan GetTimeInCycle(TimeSpan timeSpan)
    {
        double timeCycleMilliseconds = TimeSpan.FromHours(24).TotalMilliseconds;
        double totalMilliseconds = timeSpan.TotalMilliseconds;
        double timeInCycleMilliseconds = totalMilliseconds % timeCycleMilliseconds;
        TimeSpan timeInCycle = TimeSpan.FromMilliseconds(timeInCycleMilliseconds);
        return timeInCycle;
    }

    private (Color, Color, float) GetInterpolate(TimeCyclePalettePrototype timeColors, TimeSpan timeInCycle)
    {
        if (timeColors.TimeColors is null)
            return (Color.Black, Color.Black, 0.5f);

        var currentTime = timeInCycle.TotalHours;
        var startTime = -1;
        var endTime = -1;

        foreach (KeyValuePair<int, Color> kvp in timeColors.TimeColors)
        {
            var hour = kvp.Key;
            var color = kvp.Value;

            if (hour <= currentTime)
                startTime = hour;
            else if (hour >= currentTime && endTime == -1)
                endTime = hour;
        }

        if (startTime == -1)
            startTime = 0;
        else if (endTime == -1)
            endTime = 23;

        return (timeColors.TimeColors[startTime], timeColors.TimeColors[endTime],
            GetCoef(TimeSpan.FromHours(startTime), TimeSpan.FromHours(endTime), timeInCycle));
    }

    private float GetCoef(TimeSpan startTime, TimeSpan endTime, TimeSpan currentTime)
    {
        var result = (float)(currentTime.TotalMinutes - startTime.TotalMinutes) / (float)(endTime.TotalMinutes - startTime.TotalMinutes);
        return result;
    }
}
