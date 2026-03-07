using Dalamud.Game.Gui.PartyFinder.Types;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ECommons.ImGuiMethods;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using OpenRadar;
using System;
using System.Net;

namespace OpenRadar.UI;

public class MainWindow : Window
{
    public MainWindow() : base($"OpenRadar {P.GetType().Assembly.GetName().Version} ###openradar")
    {
        Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoFocusOnAppearing;
        Size = new Vector2(400, 320);

        P.windowSystem.AddWindow(this);
    }

    public void Dispose()
    {
        P.windowSystem.RemoveWindow(this);
    }

    public override void Draw()
    {
        if (ListingPlayers.Length == 0 || CurrentPost is not { } listing) return;

        if (listing.JoinConditionFlags.HasFlag(AgentLookingForGroup.JoinCondition.PrivateParty))
        {
            ImEx.CentreText("Private Party", Col.LowRed, true);
            return;
        }

        ImEx.CentreText(Util.DutyIdToName(listing.DutyId), Col.Green);
        ImGui.Separator();
        ImGui.Spacing();

        using var table = ImRaii.Table("Members", 7, ImGuiTableFlags.BordersH | ImGuiTableFlags.SizingFixedFit);
        
        if (!table) return;

        ImEx.TableColumn("##job", 20f);
        ImEx.TableColumn("Name", 130f);
        ImEx.TableColumn("World", 90f);
        ImEx.TableColumn("Prog", 70f);
        ImEx.TableColumn("Best", 20f);
        ImEx.TableColumn("Med", 20f);
        ImEx.TableColumn("Kills", 20f);
        ImGui.TableHeadersRow();

        for (int i = 0; i < ListingPlayers.Length; i++)
        {
            var player = ListingPlayers[i];
            ImGui.TableNextRow();
            
            // ── Job Column ────────────────────────────────────────
            ImGui.TableNextColumn();
            if (player == null)
            {
                var roleTextureWrap = Util.JobFlagsToRoleTexture((JobFlags)listing.SlotFlags[i]);
                if (roleTextureWrap != null)
                    ImEx.Image(roleTextureWrap, new(20,20));
                ImGui.TableNextColumn();
                ImEx.Text("-");
                continue;
            }
            var jobIcon = Util.GetJobIcon(player.jobId);
            if (jobIcon != null) ImEx.Image(jobIcon, new(20,20));

            if (player.name is not string name || player.world is not ushort world) continue;
            // ── Name Column ────────────────────────────────────────
            ImGui.TableNextColumn();
            ImEx.Text(name, Col.Cyan);
            ImEx.HoverToolTip("Open Tomestone Profile", true);
            ImEx.ClickableTextLink($"https://tomestone.gg/charcter-name/{world}/{name}");

            // ── World Column ────────────────────────────────────────
            ImGui.TableNextColumn();
            var worldName = Util.WorldIdToName(world);
            if (!worldName.IsNullOrEmpty()) ImEx.Text(worldName);

            // ── Prog Column ────────────────────────────────────────
            ImGui.TableNextColumn();
            if (player.progPoint is { } prog)
            {
                var (colour, text) = prog switch
                {
                    "fresh" => (Col.Red, "Fresh"),
                    "done" => (Col.fGold, "Cleared"),
                    "hidden" => (Col.LowGrey, "Hidden"),
                    _ => (Encounters.ProgToColour(prog, listing.DutyId), prog)
                };
                ImEx.Text(text, colour);
            }

            // ── FFLogs Columns ───────────────────────────────────────
            var logData = player.logData;
            if (logData == null) continue;

            ImGui.TableNextColumn();
            if (logData.BestParse is { } bp) ImEx.Text(Floor1(bp), ParseColour(bp));
            
            ImGui.TableNextColumn();
            if (logData.MedianParse is { } mp) ImEx.Text(Floor1(mp), ParseColour(mp));

            ImGui.TableNextColumn();
            if (logData.Kills is { } k) ImEx.Text(k);
        }
    }

    private static string Floor1(float value)
        => (MathF.Floor(value * 10f) / 10f).ToString("F1");

    private static Vector4 ParseColour(float pct) => pct switch
    {
        >= 100f => Col.fGold,
        >= 99f => Col.fPink,
        >= 95f => Col.fOrange,
        >= 75f => Col.fPurple, 
        >= 50f => Col.fBlue, 
        >= 25f => Col.fGreen, 
        _ => Col.fGrey,
    };
}
