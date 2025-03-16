using Content.Shared.Light.Components;
using Content.Shared.Light.EntitySystems;
using Content.Shared.Maps;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._Finster.Audio;

public sealed partial class EchoEffectSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefManager = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly SharedRoofSystem _roof = default!;
    [Dependency] private readonly SharedAudioEffectsSystem _effectsSystem = default!;

    private static readonly ProtoId<AudioPresetPrototype> EchoEffectPreset = "ConcertHall";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AudioComponent, MapInitEvent>(OnMapInit, before: [typeof(SharedAudioSystem)]);
    }

    private void OnMapInit(Entity<AudioComponent> ent, ref MapInitEvent args)
    {
        if (ApplyEcho(ent))
            return;
    }

    public bool ApplyEcho(Entity<AudioComponent> ent, ref ComponentInit args)
    {
        return ApplyEcho(ent);
    }

    public bool ApplyEcho(Entity<AudioComponent> sound, ProtoId<AudioPresetPrototype>? preset = null)
    {
        if (!_timing.IsFirstTimePredicted)
            return false;

        if (!Exists(sound))
            return false;

        // Фоновая музыка не должна подвергаться эффектам эха
        if (sound.Comp.Global)
            return false;

        _effectsSystem.TryAddEffect(sound, preset ?? EchoEffectPreset);
        return true;
    }
}