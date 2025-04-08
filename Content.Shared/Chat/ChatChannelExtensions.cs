namespace Content.Shared.Chat;

public static class ChatChannelExtensions
{
    public static Color TextColor(this ChatChannel channel)
    {
        return channel switch
        {
            /*
            ChatChannel.Server => Color.FromHex("#D2691E"),
            ChatChannel.Radio => Color.FromHex("#556B2F"),
            ChatChannel.LOOC => Color.FromHex("#4682B4"),
            ChatChannel.OOC => Color.FromHex("#708090"),
            ChatChannel.Dead => Color.FromHex("#4B0082"),
            ChatChannel.Admin => Color.FromHex("#DC143C"),
            ChatChannel.AdminAlert => Color.FromHex("#B22222"),
            ChatChannel.AdminChat => Color.FromHex("#FF1493"),
            ChatChannel.Whisper => Color.FromHex("#505050"),
            */
            ChatChannel.Server => Color.Orange,
            ChatChannel.Radio => Color.LimeGreen,
            ChatChannel.LOOC => Color.MediumTurquoise,
            ChatChannel.OOC => Color.LightSkyBlue,
            ChatChannel.Dead => Color.MediumPurple,
            ChatChannel.Admin => Color.Red,
            ChatChannel.AdminAlert => Color.Red,
            ChatChannel.AdminChat => Color.HotPink,
            ChatChannel.Whisper => Color.DarkGray,
            _ => Color.LightGray
        };
    }
}
