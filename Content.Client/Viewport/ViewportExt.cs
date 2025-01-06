using Content.KayMisaZlevels.Client;
using Robust.Client.UserInterface.CustomControls;

namespace Content.Client.Viewport
{
    public static class ViewportExt
    {
        public static int GetRenderScale(this IViewportControl viewport)
        {
            if (viewport is ZScalingViewport svp)
                return svp.CurrentRenderScale;

            return 1;
        }
    }
}
