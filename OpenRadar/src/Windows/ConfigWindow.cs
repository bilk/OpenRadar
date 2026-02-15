namespace OpenRadar.Windows;

public class ConfigWindow : Window
{
    public ConfigWindow() : base($"OpenRadar {P.GetType().Assembly.GetName().Version} ###configopenradar")
    {
        Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoResize;
        SizeConstraints = new()
        {
            MinimumSize = new Vector2(400, 400),
            MaximumSize = new Vector2(400, 400)
        };
        P.windowSystem.AddWindow(this);
    }

    public void Dispose()
    {
        P.windowSystem.RemoveWindow(this);
    }

    public override void Draw()
    {
        ImGui.Text("Future Config Window...");
        /*
        Show Locked PFs
        Info button to find where last found (info from PlayerTrack)
        
        */
    }
}