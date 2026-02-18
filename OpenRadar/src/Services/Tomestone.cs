using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ECommons.ExcelServices;
using Lumina;
using Newtonsoft.Json.Linq;
using OpenRadar;
using OpenRadar.Tasks;
using SQLitePCL;

namespace Openradar;

public static class Tomestone
{
    // Tomestone's API still is WIP and requires an auth key, thus this solves redirect and parses html. 
    // Unsure whether this is generally frowned upon. Also involves large packets.
    // Will break if anti-bot measures are implemented.
    // Later implementation should ask user for auth key and heavily rate limit parsing html directly.
    // First implementation is purely just parsing html because it's easier. The API is confusing.

    // The API does not offer a method of fetching best prog for an activity, the response bodies are huge too
    // User fetches from progression-graph and then finds the lowest value of the many bestPercent
    // although is a single request, progression-graph response body can be very big, html request may be smaller (cap).
    // Can fetch based on activity but its worse in every way.
    // Pulling redirect, parsing html seems easier and faster
    // Could check if the duty is dawntrail and not need to find redirect location, as /character-name/world/player%20name/progress?... returns dawntrail progress by default

    // A hidden json exists called character-contents, still requires lodestone ID but lightweight and very good

    // remove this html agility pack shit
    private static bool RefetchedAlready = false;

    public static void GetPlayerProg(Data.PlayerInfo playerInfo, int index)
    {
        Task.Run(async () =>
        {
            await GetPlayerProgAsync(playerInfo, index).ConfigureAwait(false); 
        });
    }

    private static async Task GetPlayerProgAsync(Data.PlayerInfo player, int index)
    {
        var postInfo = Data.CurrentPost;

        if (player != null && postInfo != null && postInfo.dutyId != 0)
        {
            var prog = await FetchPlayerProg(player, postInfo.dutyId);
            Data.ProgPoints[index] = prog;
        }
    }

    private static string PrettifyProg(string prog)
    {
        // do some font awesome business
        // cba rn
        // options are 
        // - prog point (player is in progress)
        // - done (player has complete the fight)
        // - notstarted (player is yet to start)
        // - hidden (player has a hidden profile)
        // - invalid (not a valid duty category (normal raids etc))
        // - null (inaccessible json, maybe something broke)
        // -- inaccessible meaning player does not exist usually
        // i should probably make an enum


        return "poo";
    }

    private static async Task<string?> FetchPlayerProg(PlayerInfo player, ushort dutyId) 
    {
        var name = player.name!;
        var world = Util.WorldIdToName(player.world);
        var dutyInfo = Encounters.DataQuery(dutyId);
        Svc.Log.Debug("Fetching LodestoneId");
        var lodestoneId = await ResolveRedirectAndGetLodestoneId(name, world);

        if (lodestoneId == null || dutyInfo == null)
        {
            if (!RefetchedAlready)
            {
                Svc.Log.Warning($"{name}@{world} has changed name/world. Refetching and updating local database...");
                TaskPlateInfoFetch.Enqueue(player.content_id);
                RefetchedAlready = true;
                return null;
            }
            else
            {
                Svc.Log.Error($"{name}@{world} does not exist in tomestone's database. Giving up...");
                RefetchedAlready = false;
                return "?";
            }
        }
        Svc.Log.Debug($"Got LodestoneId ");
        var progPageUrl = $"https://tomestone.gg/character-contents/{lodestoneId}/{name.ToLower().Replace(" ", "-")}/progress?encounterExpansion={dutyInfo.expansion.ToLower()}";
        Svc.Log.Debug(progPageUrl);
        var pageJson = await FetchPageJson(progPageUrl);
        if (pageJson == null)
            return null;

        //Svc.Log.Debug(pageJson);
        return ParseJson(pageJson, dutyInfo);
    }

    private static string? ParseLoop(JArray duties, Encounters.Info dutyInfo)
    {
        for (int i = 0; i < duties.Count; i++)
        {
            var duty = duties[i];
            var dutyName = duty["zoneName"];
            if (dutyName!.ToString().ToLower() == dutyInfo.name.ToLower())
            {
                if (dutyInfo.savageParent != null && // is savage
                    dutyInfo.name.ToLower() == dutyInfo.savageParent!.ToLower() &&  // selected duty is parent
                    (i+1) < duties.Count && // verifies that it is before door boss
                    duty["activity"] as JObject != null) // before door boss is complete
                {
                    continue; // go to after door boss as before is complete
                }

                var progression = duty["progression"] as JObject;
                if (progression != null)
                {
                    return progression["percent"]!.ToString(); // in progress
                }

                if (duty["activity"] as JObject != null)
                {
                    return "done"; // they've completed that duty
                }; // progression and activity do not exist, therefore not started at all
            }
        }
        return "fresh";
    }


    private static string? ParseJson(string json, Encounters.Info dutyInfo)
    {
        // honestly this is horrible. parsing raw html is probably less of a mess. works though
        try
        {
            var jsonResponse = JToken.Parse(json);
            if (jsonResponse.Count() == 0)
                return "hidden";

            var encounters = jsonResponse["encounters"] as JObject;    

            var selectedExpansion = encounters!["selectedExpansion"] as JObject;

            if (dutyInfo.category == null)
                return "invalid";

            var dutyCategory = selectedExpansion![dutyInfo.category] as JArray;

            if (dutyCategory!.Count == 0)
                return "fresh";


            if (!dutyInfo.savageParent.IsNullOrEmpty())
            {
                foreach (var savageTier in dutyCategory)
                {
                    if (savageTier["zoneName"]!.ToString().ToLower() == dutyInfo.savageParent!.ToLower())
                    {
                        
                        var savageDuties = savageTier["encounters"] as JArray;
                        if (savageDuties == null)
                            return "done"; // encounters doesn't exist, that means they have completed the tier
                        var prog = ParseLoop(savageDuties, dutyInfo);
                        if (prog != null)
                            return prog;
                    }
                }
                return "fresh";
            }
            return ParseLoop(dutyCategory, dutyInfo);
        }
        catch {}

        return null;
    }

    private static async Task<string?> FetchPageJson(string url)
    {
        try
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }
        catch
        {
            Svc.Log.Error("Tomestone JSON Error");
        }
        return null;
    }


    private static async Task<string?> ResolveRedirectAndGetLodestoneId(string name, string world)
    {
        var nameEncoded = Uri.EscapeDataString(name);
        var url = $"https://tomestone.gg/character-name/{world}/{nameEncoded}";

        string? lodestoneId = null;

        try
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false
            };

            using var client = new HttpClient(handler);

            using var request = new HttpRequestMessage(HttpMethod.Head, url);
            using var response = await client.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Found && response.Headers.Location != null)
            {
                string pattern = @"/character/(\d+)";
                var matchLodestone = Regex.Match(response.Headers.Location.ToString(), pattern);
                if (matchLodestone.Success)
                    lodestoneId = matchLodestone.Groups[1].Value;
            }
        }
        catch
        {
            Svc.Log.Error("Tomestone Redirect Error");
        }
        
        return lodestoneId;
    }

}