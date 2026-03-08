using System.ComponentModel;
using ECommons.GameHelpers;

namespace OpenRadar.UI;

public class ConfigWindow : Window
{
    public ConfigWindow() : base($"OpenRadar {P.GetType().Assembly.GetName().Version} ###configopenradar")
    {
        Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoResize;
        Size = new Vector2(500, 480);
        P.windowSystem.AddWindow(this);
    }

    public void Dispose()
    {
        P.windowSystem.RemoveWindow(this);
    }

    private Vector4 debugColor = new Vector4(0.2f, 0.6f, 0.2f, 1f);

    public override void Draw()
    {
        if (C.FirstInstalled)
        {
            WelcomeMessage();
            return;
        }

        // ── Debug ────────────────────────────────────────────────────────────
#if DEBUG
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();
        var firstInstalled = C.FirstInstalled;
        if (ImGui.Checkbox("First Installed", ref firstInstalled))

            C.FirstInstalled = firstInstalled;

        ImGui.ColorEdit4("Test Color (RGBA)", ref debugColor, ImGuiColorEditFlags.Float);
#endif
    }


    private void WelcomeMessage()
    {
        ImGui.Spacing();
        ImGui.TextColored(new Vector4(0.4f, 0.8f, 1f, 1f), "Welcome to OpenRadar!");
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.TextWrapped("OpenRadar attempts to retrieve player names and worlds from PF using the following methods:");

        ImGui.Spacing();

        ImGui.Text(" 1) Query local databases - OpenRadar's and PlayerTrack's (if installed)");
        ImGui.Text(" 2) Request Adventurer Plate information from the game servers");
        ImGui.Text(" 3) If the plate is private or missing, requests 'Friend' info of the player");

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.TextWrapped("The Adventurer Plate request simulates the user clicking 'View Adventurer Plate' and is considered safe as the user can do this normally even if the target is offline or cross-world.");

        ImGui.Spacing();

        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1f, 0.6f, 0.2f, 1f));
        ImGui.TextWrapped("The Friend Info request may carry some risk, as it queries information for a non-friend and could be seen by the game servers as suspicious. This is disabled by default and is up to the user (YOU) to enable it.");
        ImGui.PopStyleColor();

        ImGui.Spacing();

        ImGui.TextWrapped("Without the optional request enabled, the plugin retrieves roughly 95% of PF players. Enabling it increases coverage to 100%.");

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.6f, 0.2f, 1f));
        if (ImGui.Button("I Understand", new Vector2(-1, 35)))
            C.FirstInstalled = false;

        ImGui.PopStyleColor();
    }
}
