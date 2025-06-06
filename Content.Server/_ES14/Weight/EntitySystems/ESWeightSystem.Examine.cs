using Content.Shared._ES14.Weight.Components;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.Verbs;
using Robust.Shared.Utility;

namespace Content.Server._ES14.Weight.EntitySystems;

public sealed partial class ESWeightSystem
{
    [Dependency] private readonly ExamineSystemShared _examineSystem = default!;

    private void InitializeExamine()
    {
        SubscribeLocalEvent<ESWeightComponent, ExaminedEvent>(OnExamined);
        //SubscribeLocalEvent<ESWeightComponent, GetVerbsEvent<ExamineVerb>>(OnGetExamineVerbs);
    }

    private void OnExamined(Entity<ESWeightComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;
        args.PushMarkup(
            Loc.GetString(
                "weight-examine",
                ("weight", FixedPoint2.New(ent.Comp.Total))));
    }

    /*
    private void OnGetExamineVerbs(EntityUid uid, ESWeightComponent component, GetVerbsEvent<ExamineVerb> args)
    {
        if (component.HideInExamine)
            return;

        var detailsRange = _examineSystem.IsInDetailsRange(args.User, uid);
        var verb = new ExamineVerb
        {
            Act = () =>
            {
                var formattedMessage = new FormattedMessage();
                var markup = Loc.GetString(
                    "weight-examine",
                    ("weight", FixedPoint2.New(component.Total)));
                formattedMessage.PushMarkup(markup);
                _examineSystem.SendExamineMessage(args.User, uid, formattedMessage, false, false);
            },
            Text = Loc.GetString("weight-examinable-verb-text"),
            Category = VerbCategory.Examine,
            Disabled = !detailsRange,
            Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/dot.svg.192dpi.png"))
        };

        args.Verbs.Add(verb);
    }
    */
}
