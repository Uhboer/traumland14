using Robust.Client.UserInterface;

namespace Content.Client.Stylesheets
{
    public interface IStylesheetManager
    {
        // FIXME: I need move all UI styling into one file. So, for now, we can link on existed styles class.
        StyleLora StyleLora { get; }

        Stylesheet SheetNano { get; }
        Stylesheet SheetSpace { get; }
        Stylesheet SheetLora { get; }

        void Initialize();
    }
}
