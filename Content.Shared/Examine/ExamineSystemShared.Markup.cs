
using Robust.Shared.Utility;

namespace Content.Shared.Examine;

public abstract partial class ExamineSystemShared : EntitySystem
{
    public void PushEntityNameLine(ref FormattedMessage message, string entityName)
    {
        //message.PushNewline();

        //message.PushColor(Color.DarkGray);
        //message.AddText(Loc.GetString("examine-present-line") + " ");
        //message.Pop();

        message.AddText(Loc.GetString("examine-present",
            ("name", entityName)));

        //message.PushColor(Color.DarkGray);
        //message.AddText(" " + Loc.GetString("examine-present-line"));
        //message.Pop();
    }

    public void PushEntityNameLine(ref FormattedMessage message, EntityUid uid, string entityName)
    {
        //message.PushNewline();

        //message.PushColor(Color.DarkGray);
        //message.AddText(Loc.GetString("examine-present-line") + " ");
        //message.Pop();

        message.AddText(Loc.GetString("examine-present-tex",
            ("id", GetNetEntity(uid).Id),
            ("size", 10),
            ("name", entityName)));

        //message.PushColor(Color.DarkGray);
        //message.AddText(" " + Loc.GetString("examine-present-line"));
        //message.Pop();
    }

    public void PushExamineYourselfLine(ref FormattedMessage message)
    {
        //message.PushNewline();

        //message.PushColor(Color.DarkGray);
        //message.AddText(Loc.GetString("examine-present-line") + " ");
        //message.Pop();

        message.AddText(Loc.GetString("examine-himself"));

        //message.PushColor(Color.DarkGray);
        //message.AddText(" " + Loc.GetString("examine-present-line"));
        //message.Pop();
    }

    public void PushTitleLine(ref FormattedMessage message, string title)
    {
        //message.PushNewline();

        //message.PushColor(Color.DarkGray);
        //message.AddText(Loc.GetString("examine-present-line") + " ");
        //message.Pop();

        message.AddText(title);

        //message.PushColor(Color.DarkGray);
        //message.AddText(" " + Loc.GetString("examine-present-line"));
        //message.Pop();
    }

    public void PushLine(ref FormattedMessage message)
    {
        //message.PushNewline();

        message.PushColor(Color.FromHex("#282D31"));
        message.AddText(Loc.GetString("examine-border-line"));
        message.Pop();

        //message.PushNewline();
    }

    public void PushFont(ref FormattedMessage message)
    {
        message.PushTag(new MarkupNode("font", new MarkupParameter(""),
        new Dictionary<string, MarkupParameter>()
        {
                {"size", new MarkupParameter(10)}
        }));
    }
}
