using System.Collections.Generic;
using Dalamud.Game.Gui.PartyFinder.Types;
using ECommons.DalamudServices.Legacy;

namespace OpenRadar;

public static class Network
{
    public static List<ListingInformation> PFListings = new();
    public static List<PlayerInfo?> RecentExtractedPlayers = new();
    private static bool IsReceivingPage = false;

    public unsafe static void PFExtract(
        nint dataPtr,
        ushort opCode,
        uint sourceActorId,
        uint targetActorId,
        NetworkMessageDirection direction)
    {        
        if (opCode == 760 && direction == NetworkMessageDirection.ZoneDown)
        {
            //PrintData<int>(dataPtr, 30, 30);
            RecentExtractedPlayers.Clear();
            for (int i = 12; i < 20; i++)
            {
                // content_ids stored as ulongs in packet
                ulong content_id = *((ulong*)dataPtr + i);
                var playerInfo = PlayerTrackInterop.Extract(content_id);
                RecentExtractedPlayers.Add(playerInfo);
            }
        }
        else if (opCode == 179)
        {
            // end packet after pf post packets delivered,
            Svc.Log.Debug("PF Page Complete");
            IsReceivingPage = false;
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

    private unsafe static void PrintData<T>(nint dataPtr, int totalRows, int infoPerRow) where T : unmanaged
    {
        T* ptr = (T*)dataPtr;

        for (int row = 0; row < totalRows; row++)
        {
            string packetInfoRow = "";
            for (int col = 0; col < infoPerRow; col++)
            {
                T dataPoint = *(ptr + row * infoPerRow + col);
                packetInfoRow += $"{dataPoint} ";
            }
            Svc.Log.Debug(packetInfoRow);
        }
    }
}
