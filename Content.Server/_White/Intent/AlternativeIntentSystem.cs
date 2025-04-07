using Content.Shared.Alert;
using JetBrains.Annotations;

namespace Content.Server._White.Intent;

public sealed class AlternativeIntentSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;

    public override void Initialize()
    {
        base.Initialize();


    }
}

/// <summary>
///     Intents, what should be used by Middle Mouse Button.
/// </summary>
public enum AlternativeIntents : byte
{
    Kick,
    Climb,
    Jump
}

[UsedImplicitly, DataDefinition]
public sealed partial class SetAlternativeIntentAlert : IAlertClick
{
    [DataField("intent", required: true)]
    public AlternativeIntents Intent { get; set; }

    public void AlertClicked(EntityUid uid)
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();

    }
}
