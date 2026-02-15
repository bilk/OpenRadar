using Dalamud.Interface.Textures.TextureWraps;
using ECommons.ImGuiMethods;
using Lumina.Excel.Sheets;

namespace OpenRadar.Windows;

public class MainWindow : Window
{
    public MainWindow() : base($"OpenRadar {P.GetType().Assembly.GetName().Version} ###openradar")
    {
        Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoFocusOnAppearing;
        SizeConstraints = new()
        {
            MinimumSize = new Vector2(250, 300),
            MaximumSize = new Vector2(250, 300)
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

        var extractedPlayers = Network.RecentExtractedPlayers;

        if (extractedPlayers.Count > 0)
        {
            var listing = Network.PFListings
                .FirstOrDefault(l => 
                l.hostContentId == extractedPlayers.First()!.content_id);
                
            if (listing != null)
            {
                ImGuiEx.TextCentered(new Vector4(0f, 1f, 0f, 1f), listing.duty.Name.ToString());
                ImGui.Separator();
                ImGui.Dummy(new Vector2(20,10));

                if (ImGui.BeginTable("Players", 3, ImGuiTableFlags.BordersH | ImGuiTableFlags.SizingFixedFit))
                {
                    ImGui.TableSetupColumn("##job");
                    ImGui.TableSetupColumn("Name");
                    ImGui.TableSetupColumn("World");
                    ImGui.TableHeadersRow();
                    for (int i = 0; i < listing.slotCount; i++)
                    {
                        var job = listing.jobsPresent[i];
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        if (job.RowId != 0)
                        {
                            var jobIcon = Util.GetJobIcon(job.RowId);
                            if (jobIcon != null)
                                ImGui.Image(jobIcon.Handle, new Vector2(20,20));
                            else
                                ImGui.Image(Util.GetJobIcon(45)!.Handle, new Vector2(20,20));
                            ImGui.TableNextColumn();
                            var player = extractedPlayers[i];
                            if (player != null && !player.name.IsNullOrEmpty())
                            {
                                if (i == 0)
                                    ImGui.TextColored(new Vector4(0f, 0.3f, 1f, 1f), player.name);
                                else
                                    ImGui.Text(player.name);
                            }
                            else
                            {
                                ImGui.TextColored(new Vector4(1f, 0.2f, 0f, 1f), "Player Missing");
                            }
                            ImGui.TableNextColumn();
                            if (player != null && !player.world.IsNullOrEmpty())
                            {
                                var world = Svc.Data.GetExcelSheet<World>().First(world => world.RowId == player.world!.ParseInt()).InternalName.ExtractText();
                                ImGui.Text(world);
                            }

                        }
                        else
                        {
                            ImGui.TableNextColumn();
                            ImGui.Text("Empty");
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
