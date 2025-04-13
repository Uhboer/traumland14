using System.Linq;
using Content.Server.Power.Components;
using Content.Shared.Teleportation.Components;
using Robust.Shared.Map;

namespace Content.Server.Power.EntitySystems;

public sealed class PowerGridAdapterSystem : EntitySystem
{
    [Dependency] private readonly IMapManager _mapManager = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        /*
        var query = EntityQueryEnumerator<PowerVerticalAdapterComponent, LinkedEntityComponent, PowerNetworkBatteryComponent>();
        while (query.MoveNext(out var uid, out var adapter, out var linkComp, out var battery))
        {
            var linkedAdapter = linkComp.LinkedEntities.First();
            TransferPower(uid, linkedAdapter, adapter.MaxPowerTransfer * frameTime);
        }

        private void TransferPower(EntityUid source, EntityUid target, float amount)
        {
            if (!TryComp<PowerNetworkBatteryComponent>(source, out var srcBat) ||
                !TryComp<PowerNetworkBatteryComponent>(target, out var tgtBat))
                return;

            var actualTransfer = Math.Min(amount, srcBat.CurrentEnergy);
            srcBat.CurrentEnergy -= actualTransfer;
            tgtBat.CurrentEnergy += actualTransfer;

            Dirty(srcBat);
            Dirty(tgtBat);
        }
        */
    }
}
