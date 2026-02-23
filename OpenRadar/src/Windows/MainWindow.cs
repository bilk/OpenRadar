using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using ECommons.ImGuiMethods;

namespace OpenRadar.Windows;

public class MainWindow : Window
{
    public MainWindow() : base($"OpenRadar {P.GetType().Assembly.GetName().Version} ###openradar")
    {
        Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoFocusOnAppearing;
        SizeConstraints = new()
        {
            MinimumSize = new Vector2(400, 275),
            MaximumSize = new Vector2(400, 275)
        };
        P.windowSystem.AddWindow(this);
    }

    public void Dispose()
    {
        P.windowSystem.RemoveWindow(this);
    }

    public override void Draw()
    {
        var LookingForGroupDetailPos = AddonHandler.addonPosition;
        var windowPos = new Vector2(LookingForGroupDetailPos.X + AddonHandler.addonWidth, LookingForGroupDetailPos.Y);
        ImGui.SetWindowPos(windowPos);

        var extractedPlayers = Data.ExtractedPlayers;

        if (extractedPlayers.Count > 0)
        {
            var listing = Data.CurrentPost;
            if (listing.isPrivate)
                ImGuiEx.TextCentered(new Vector4(1f, 0f, 0f, 1f), "Private PF");
            else if (listing != null)
            {
                var dutyName = Util.DutyIdToName(listing.dutyId);
                ImGuiEx.TextCentered(new Vector4(0f, 1f, 0f, 1f), dutyName);
                ImGui.Separator();
                ImGui.Dummy(new Vector2(20,10));

                if (ImGui.BeginTable("Players", 4, ImGuiTableFlags.BordersH | ImGuiTableFlags.SizingFixedFit))
                {
                    ImGui.TableSetupColumn("##job", ImGuiTableColumnFlags.None, 20f);
                    ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.None, 140f);
                    ImGui.TableSetupColumn("World", ImGuiTableColumnFlags.None, 100f);
                    ImGui.TableSetupColumn("Prog", ImGuiTableColumnFlags.None, 80f);
                    ImGui.TableHeadersRow();
                    for (int i = 0; i < listing.contentIds.Count; i++)
                    {
                        var jobIcon = listing.jobIcons[i];
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        
                        if (jobIcon != null)
                        {
                            var player = extractedPlayers[i];
                            var wrap = jobIcon.GetWrapOrDefault();
                            if (wrap != null)
                            {
                                ImGui.Image(wrap.Handle, new Vector2(20,20));
                            }
                            ImGui.TableNextColumn();
                            if (player != null && !player.name.IsNullOrEmpty())
                            {   
                                ImGui.Text(player.name);
                            }
                            else
                            {
                                ImGui.TextColored(new Vector4(1f, 0.7f, 0.2f, 1f), "...");
                            }
                            ImGui.TableNextColumn();
                            if (player == null || player.world == 0)
                                ImGui.Text("");
                            else
                            {
                                ImGui.Text(Util.WorldIdToName(player.world));
                            }
                            ImGui.TableNextColumn();
                            var prog = Data.ProgPoints[i];

                            if (prog == null)
                            {
                                ImGui.Text("-");
                                /*
                                using (var font = ImRaii.PushFont(UiBuilder.IconFont))
                                {
                                    ImGui.TextColored(new Vector4(1f, 0.7f, 0.2f, 1f),FontAwesomeIcon.Spinner.ToIconString());
                                }*/
                            }
                            else
                            {
                                var (colour, text) = prog switch
                                {
                                    "fresh"  => (new Vector4(1f, 0f, 0f, 1f), "Fresh"),
                                    "done"   => (new Vector4(0.898f, 0.8f, 0.502f, 1f), "Cleared"),
                                    "hidden" => (new Vector4(1f, 1f, 1f, 0.25f), "Hidden"),
                                    _ => (Encounters.ProgToColour(prog, listing.dutyId), prog)
                                };
                                ImGui.TextColored(colour, text);
                            }
                        }
                        else
                        {
                            var roleIcon = listing.roleIcons[i];
                            if (roleIcon != null)
                                ImGui.Image(roleIcon.Handle, new Vector2(20,20));
                            ImGui.TableNextColumn();
                            ImGui.TextColored(new Vector4(0f, 1f, 0.2f, 1f), "-");
                        }
                    }
                }
                ImGui.EndTable();
            }
        }
        else
        {
            ImGui.Text("Loading...");
        }
    }
}
