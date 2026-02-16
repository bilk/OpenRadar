using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Text.SeStringHandling;

namespace OpenRadar;

public static class ToastHandler
{
    public static void ErrorToast(ref SeString message, ref bool isHandled)
    {
        if (Util.ContainsSeString(message, "Plate"))
        {
            // should create flag to state its querying pf post to prevent editing toast when not in use
            message = "";
        }
    }
}
