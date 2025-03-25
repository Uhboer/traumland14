using Content.Server._ES14.Weight.Components;
using Content.Server._ES14.Weight.Events;
using Content.Server.Examine;
using Content.Shared.Verbs;
using Robust.Shared.Utility;

namespace Content.Server._ES14.Weight.EntitySystems;

/// <summary>
/// This handles weight loss.
/// </summary>
public sealed class ESWeightLossSystem : EntitySystem
{
    [Dependency] private readonly ExamineSystem _examine = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<ESWeightLossComponent, ESWeightGetModifierEvent>(OnGetWeightModifiers);
        SubscribeLocalEvent<ESWeightLossComponent, GetVerbsEvent<ExamineVerb>>(OnClothingVerbExamine);
    }

    private void OnClothingVerbExamine(Entity<ESWeightLossComponent> ent, ref GetVerbsEvent<ExamineVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        var weightModifierPercentage = MathF.Round((1.0f - ent.Comp.InsideWeightLossModifier) * 100f, 1);

        if (weightModifierPercentage == 0.0f)
            return;

        var msg = new FormattedMessage();

        msg.AddMarkupOrThrow(Loc.GetString("weight-loss-examine", ("percent", weightModifierPercentage)));


        _examine.AddDetailedExamineVerb(args,
            ent,
            msg,
            Loc.GetString("weight-loss-examinable-verb-text"),
            "/Textures/Interface/VerbIcons/dot.svg.192dpi.png",
            Loc.GetString("weight-loss-examinable-verb-message"));
    }

    private void OnGetWeightModifiers(Entity<ESWeightLossComponent> ent, ref ESWeightGetModifierEvent args)
    {
        args.InsideWeightModifier *= ent.Comp.InsideWeightLossModifier;
    }
}