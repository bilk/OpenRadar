using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Dalamud.Game.Gui.PartyFinder.Types;
using ECommons.DalamudServices.Legacy;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using Lumina.Excel.Sheets;
using Microsoft.VisualBasic;
using OpenRadar.Tasks;

namespace OpenRadar;

public static class Network
{
    public static List<ListingInformation> PFListings = new();
    public static List<PlayerInfo?> RecentExtractedPlayers = new();
    private static bool IsReceivingPage = false;

    public unsafe static void PFExtract(nint dataPtr, ushort opCode, uint sourceActorId, uint targetActorId, NetworkMessageDirection direction)
    {        
        if (direction == NetworkMessageDirection.ZoneDown)
        {

            if (opCode == 760)
            {
                // content_ids from pf post
                Data.ResetExtractedData();
                for (int i = 0; i < 8; i++)
                {
                    // content_ids stored as ulongs in packet
                    ulong content_id = *((ulong*)dataPtr + i + 12);
                    Data.ExtractedContentIds[i] = content_id;
                    TaskPlayerTrackQuery.Enqueue(content_id);
                }
            }
            if (opCode == 179)
            {
                // end packet after pf post packets delivered
                Svc.Log.Debug("PF Page Complete");
                IsReceivingPage = false;
            }
            if (opCode == 689)
            {
                //Util.PrintData<ulong>(dataPtr, 10, 10);

                var player = FetchPlatePacketInfo(dataPtr);  
                if (player!=null)
                {
                    Database.AddPlayer(player);           
                    Data.UpdatePlayerList(player);
                }
            }
        }
    }
    public static void ListingExtract(IPartyFinderListing listing, IPartyFinderListingEventArgs args)
    {
        if (!IsReceivingPage)
        {
            PFListings.Clear();
            IsReceivingPage = true;
        }

        if (!args.Visible)
            return;

        Svc.Log.Debug("PF Listing Extraction");

        var extractedListing = new ListingInformation
        {
            hostContentId = listing.ContentId,
            jobsPresent = listing.JobsPresent.Select(j => j.Value).ToList(),
            duty = listing.Duty.Value,
            description = listing.Description.TextValue?.ToString() ?? "None",
            hostName = listing.Name.TextValue,
            hostWorld = listing.HomeWorld.Value.InternalName.ExtractText(),
            slotCount = listing.Slots.Count
        };

        PFListings.Add(extractedListing);
    }

    private unsafe static PlayerInfo FetchPlatePacketInfo(nint ptr)
    {
        var contentId = *((ulong*)ptr+2);
        var playerName = Util.ReadUtf8String((byte*)ptr + 421, 30);
        ushort worldId = *((ushort*)ptr + 16);

        //var world = Svc.Data.GetExcelSheet<World>().First(world => world.RowId == worldId).InternalName.ExtractText();
        return new PlayerInfo(contentId, playerName, worldId);
    }
}
