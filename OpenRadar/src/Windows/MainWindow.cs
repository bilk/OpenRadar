using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using ECommons.ImGuiMethods;
using System;

namespace OpenRadar.Windows;

public class MainWindow : Window
{
    private bool _fflogsExpanded = false;

    public MainWindow() : base($"OpenRadar {P.GetType().Assembly.GetName().Version} ###openradar")
    {
        Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoFocusOnAppearing;
        SizeConstraints = new()
        {
            MinimumSize = new Vector2(400, 320),
            MaximumSize = new Vector2(400, 320)
        };
        P.windowSystem.AddWindow(this);
    }

    public void Dispose()
    {
        P.windowSystem.RemoveWindow(this);
    }

    public override void Draw()
    {
        bool fflogsAvailable = P.FFLogsClient.IsConfigured
            && FFLogsEncounterMapping.GetFFLogsEncounterId(Data.CurrentPost.dutyId).HasValue;
        bool showFFLogs = _fflogsExpanded && fflogsAvailable;

        float windowWidth = showFFLogs ? 560f : 400f;
        float windowHeight = fflogsAvailable ? 355f : 320f;
        SizeConstraints = new()
        {
            MinimumSize = new Vector2(windowWidth, windowHeight),
            MaximumSize = new Vector2(windowWidth, windowHeight)
        };

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
                ImGui.Dummy(new Vector2(20, 10));

                int columnCount = showFFLogs ? 7 : 4;
                if (ImGui.BeginTable("Players", columnCount, ImGuiTableFlags.BordersH | ImGuiTableFlags.SizingFixedFit))
                {
                    ImGui.TableSetupColumn("##job", ImGuiTableColumnFlags.None, 20f);
                    ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.None, 130f);
                    ImGui.TableSetupColumn("World", ImGuiTableColumnFlags.None, 90f);
                    ImGui.TableSetupColumn("Prog", ImGuiTableColumnFlags.None, showFFLogs ? 70f : 80f);
                    if (showFFLogs)
                    {
                        ImGui.TableSetupColumn("Best", ImGuiTableColumnFlags.None, 48f);
                        ImGui.TableSetupColumn("Med", ImGuiTableColumnFlags.None, 48f);
                        ImGui.TableSetupColumn("Kills", ImGuiTableColumnFlags.None, 40f);
                    }
                    ImGui.TableHeadersRow();

                    for (int i = 0; i < listing.contentIds.Count; i++)
                    {
                        var jobIcon = listing.jobIcons[i];
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();

                        if (jobIcon != null)
                        {
                            var player = extractedPlayers[i];
                            var fflogs = showFFLogs ? Data.FFLogsResults[i] : null;
                            var wrap = jobIcon.GetWrapOrDefault();
                            if (wrap != null)
                                ImGui.Image(wrap.Handle, new Vector2(20, 20));

                            // ── Name column ──────────────────────────────────────────
                            ImGui.TableNextColumn();
                            if (player != null && !player.name.IsNullOrEmpty())
                            {
                                bool hasLink = Data.LodestoneIdCache.ContainsKey(player.content_id);
                                if (hasLink)
                                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.4f, 0.8f, 1f, 1f));

                                ImGui.TextUnformatted(player.name);

                                if (hasLink)
                                {
                                    ImGui.PopStyleColor();
                                    if (ImGui.IsItemHovered())
                                    {
                                        ImGui.SetTooltip("Open Tomestone profile");
                                        ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                                    }
                                    if (ImGui.IsItemClicked())
                                    {
                                        if (Data.LodestoneIdCache.TryGetValue(player.content_id, out var lid))
                                        {
                                            var urlName = player.name!.ToLower().Replace(" ", "-");
                                            Dalamud.Utility.Util.OpenLink($"https://tomestone.gg/character/{lid}/{urlName}");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                ImGui.TextColored(new Vector4(1f, 0.7f, 0.2f, 1f), "...");
                            }

                            // ── World column ─────────────────────────────────────────
                            ImGui.TableNextColumn();
                            if (player == null || player.world == 0)
                                ImGui.TextUnformatted("");
                            else
                                ImGui.TextUnformatted(Util.WorldIdToName(player.world));

                            // ── Prog column ──────────────────────────────────────────
                            ImGui.TableNextColumn();
                            var prog = Data.ProgPoints[i];
                            if (prog == null)
                            {
                                ImGui.TextUnformatted("-");
                            }
                            else
                            {
                                var (colour, text) = prog switch
                                {
                                    "fresh" => (new Vector4(1f, 0f, 0f, 1f), "Fresh"),
                                    "done" => (new Vector4(0.898f, 0.8f, 0.502f, 1f), "Cleared"),
                                    "hidden" => (new Vector4(1f, 1f, 1f, 0.25f), "Hidden"),
                                    _ => (Encounters.ProgToColour(prog, listing.dutyId), prog)
                                };
                                ImGui.TextColored(colour, text);
                            }

                            // ── FFLogs columns ────────────────────────────────────────
                            if (showFFLogs)
                            {
                                ImGui.TableNextColumn(); // Best
                                ImGui.TableNextColumn(); // Med
                                ImGui.TableNextColumn(); // Kills

                                if (player == null || player.name.IsNullOrEmpty())
                                {
                                    // nothing yet
                                }
                                else if (fflogs == null)
                                {
                                    ImGui.TableSetColumnIndex(4);
                                    ImGui.TextColored(new Vector4(1f, 0.7f, 0.2f, 0.6f), "...");
                                }
                                else if (fflogs.IsHidden)
                                {
                                    ImGui.TableSetColumnIndex(4);
                                    ImGui.TextColored(new Vector4(1f, 1f, 1f, 0.3f), "—");
                                }
                                else
                                {
                                    // Best %
                                    ImGui.TableSetColumnIndex(4);
                                    if (fflogs.BestParse.HasValue && fflogs.BestParse.Value > 0)
                                        ImGui.TextColored(ParseColour(fflogs.BestParse.Value), Floor1(fflogs.BestParse.Value));
                                    else
                                        ImGui.TextUnformatted("-");

                                    // Median %
                                    ImGui.TableSetColumnIndex(5);
                                    if (fflogs.MedianParse.HasValue && fflogs.MedianParse.Value > 0)
                                        ImGui.TextColored(ParseColour(fflogs.MedianParse.Value), Floor1(fflogs.MedianParse.Value));
                                    else
                                        ImGui.TextUnformatted("-");

                                    // Kills
                                    ImGui.TableSetColumnIndex(6);
                                    if (fflogs.Kills.HasValue)
                                        ImGui.TextUnformatted($"{fflogs.Kills}");
                                    else
                                        ImGui.TextUnformatted("-");
                                }
                            }
                        }
                        else
                        {
                            var roleIcon = listing.roleIcons[i];
                            if (roleIcon != null)
                                ImGui.Image(roleIcon.Handle, new Vector2(20, 20));
                            ImGui.TableNextColumn();
                            ImGui.TextColored(new Vector4(0f, 1f, 0.2f, 1f), "-");

                            if (showFFLogs)
                            {
                                ImGui.TableNextColumn();
                                ImGui.TableNextColumn();
                                ImGui.TableNextColumn();
                                ImGui.TableNextColumn();
                                ImGui.TableNextColumn();
                            }
                        }
                    }
                }
                ImGui.EndTable();
            }
        }
        else
        {
            ImGui.TextUnformatted("Loading...");
        }

        // ── Show / Hide FFLogs toggle button ─────────────────────────────────
        if (fflogsAvailable)
        {
            ImGui.Spacing();
            var btnColor = _fflogsExpanded
                ? new Vector4(0.45f, 0.08f, 0.08f, 1f)   // dark red when expanded
                : new Vector4(0.10f, 0.25f, 0.50f, 1f);   // dark blue when collapsed
            ImGui.PushStyleColor(ImGuiCol.Button, btnColor);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, btnColor with { W = 0.8f });
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, btnColor with { W = 0.6f });
            if (ImGui.Button(_fflogsExpanded ? "Hide FFLogs" : "Show FFLogs", new Vector2(-1, 28)))
                _fflogsExpanded = !_fflogsExpanded;
            ImGui.PopStyleColor(3);
        }
    }

    /// <summary>Formats a parse percentage with 1 decimal place, floored (e.g. 69.99 -> "69.9").</summary>
    private static string Floor1(float value)
        => (MathF.Floor(value * 10f) / 10f).ToString("F1");

    /// <summary>
    /// Maps a parse percentage to an FF Logs colour.
    /// RGB values match FFLogsViewer's Util.GetLogColor exactly (source values divided by 255).
    /// </summary>
    private static Vector4 ParseColour(float pct) => pct switch
    {
        >= 100f => new Vector4(229 / 255f, 204 / 255f, 128 / 255f, 1f), // gold   — 100
        >= 99f => new Vector4(226 / 255f, 104 / 255f, 168 / 255f, 1f), // pink   — 99–99.9
        >= 95f => new Vector4(255 / 255f, 128 / 255f, 0 / 255f, 1f), // orange — 95–98.9
        >= 75f => new Vector4(163 / 255f, 53 / 255f, 238 / 255f, 1f), // purple — 75–94.9
        >= 50f => new Vector4(0 / 255f, 112 / 255f, 255 / 255f, 1f), // blue   — 50–74.9
        >= 25f => new Vector4(30 / 255f, 255 / 255f, 0 / 255f, 1f), // green  — 25–49.9
        _ => new Vector4(102 / 255f, 102 / 255f, 102 / 255f, 1f), // grey   — 0–24.9
    };
}
