using OpenRadar.Windows;
using ECommons.Configuration;
using Dalamud.Game.Addon.Lifecycle;
using ECommons.Automation.NeoTaskManager;
using Dalamud.Game.Command;
using OpenRadar.UI;

namespace OpenRadar;

public sealed class OpenRadar : IDalamudPlugin
{
    internal static OpenRadar P = null!;
    private Configuration config = null!;
    public static Configuration C => P.config;

    public WindowSystem windowSystem = null!;
    public MainWindow mainWindow = null!;
    public ConfigWindow configWindow = null!;

    public TaskManager taskManager = null!;
    public Memory Memory = null!;
    public FFLogsClient FFLogsClient = null!;

    public OpenRadar(IDalamudPluginInterface pi)
    {
        P = this;
        Svc.Init(pi);

        EzConfig.Migrate<Configuration>();
        config = EzConfig.Init<Configuration>();

        Memory = new();
        FFLogsClient = new();

        windowSystem = new();
        mainWindow = new();
        configWindow = new();

        taskManager = new(new(abortOnTimeout: true, timeLimitMS: 25000, showDebug: false));

        Svc.PluginInterface.UiBuilder.Draw += windowSystem.Draw;
        Svc.PfGui.ReceiveListing += Network.ListingHostExtract;
        Svc.Toasts.ErrorToast += ToastHandler.ErrorToast;
        Svc.Chat.ChatMessage += ChatHandler.PlateError;

        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostDraw, "LookingForGroupDetail", AddonHandler.LookingForGroupDetail);

        Svc.PluginInterface.UiBuilder.OpenMainUi += () => { configWindow.IsOpen = true; };
        Svc.PluginInterface.UiBuilder.OpenConfigUi += () => { configWindow.IsOpen = true; };

        if (C.FirstInstalled)
        {
            if (PlayerTrackInterop.Installed())
                C.PlayerTrackReader = true;
            configWindow.IsOpen = true;
        }

        Svc.Commands.AddHandler("/openradar", new CommandInfo(OnCommand)
        {
            HelpMessage = "Displays Config Window"
        });
    }

    public void Dispose()
    {
        P.Memory.Dispose();
        P.FFLogsClient.Dispose();
        Svc.PluginInterface.UiBuilder.Draw -= windowSystem.Draw;
        Svc.PfGui.ReceiveListing -= Network.ListingHostExtract;
        Svc.Toasts.ErrorToast -= ToastHandler.ErrorToast;
        Svc.Chat.ChatMessage -= ChatHandler.PlateError;
        Svc.AddonLifecycle.UnregisterListener(AddonEvent.PostDraw, "LookingForGroupDetail", AddonHandler.LookingForGroupDetail);
        Svc.Commands.RemoveHandler("/openradar");
        ECommonsMain.Dispose();
    }

    private void OnCommand(string command, string args) => configWindow.IsOpen = !configWindow.IsOpen;
}