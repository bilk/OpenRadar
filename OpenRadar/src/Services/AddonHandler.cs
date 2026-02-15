using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;

namespace OpenRadar;

public static class AddonHandler
{

    public static Vector2 addonPosition = new();
    public static float addonWidth = new();
    public static void LookingForGroupDetail(AddonEvent type, AddonArgs args)
    {
        addonPosition = args.Addon.Position;
        addonWidth = args.Addon.ScaledWidth;
        if (args.Addon.IsVisible)
        {
            P.mainWindow.IsOpen = true;
        }
        else
        {
            P.mainWindow.IsOpen = false;
            Network.RecentExtractedPlayers.Clear();
        }
    }

}