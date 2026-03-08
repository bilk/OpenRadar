using Dalamud.Game.Text.SeStringHandling;

namespace OpenRadar;

public static class ToastHandler
{
    public static bool handled = false;

    public static void ErrorToast(ref SeString message, ref bool isHandled)
    {
        if (Util.ContainsSeString(message, "Plate"))
        {
            // this needs fixing, I have no clue how error toasts work
            isHandled = false;
        }
    }
}
