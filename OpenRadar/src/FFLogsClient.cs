using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace OpenRadar;

public class FFLogsClient : IDisposable
{
    private readonly HttpClient httpClient = new();
    public bool IsTokenValid { get; private set; }

    public bool IsConfigured =>
        !C.FFLogsClientId.IsNullOrEmpty() && !C.FFLogsClientSecret.IsNullOrEmpty();

    public FFLogsClient()
    {
        Initialize();
    }

    public void Reinitialize()
    {
        IsTokenValid = false;
        httpClient.DefaultRequestHeaders.Authorization = null;
        Initialize();
    }

    private void Initialize()
    {
        if (IsConfigured)
            Task.Run(FetchAndSetToken);
    }

    private async Task FetchAndSetToken()
    {
        try
        {
            var form = new Dictionary<string, string>
            {
                { "grant_type",    "client_credentials" },
                { "client_id",     C.FFLogsClientId },
                { "client_secret", C.FFLogsClientSecret },
            };

            var response = await httpClient.PostAsync(
                "https://www.fflogs.com/oauth/token",
                new FormUrlEncodedContent(form)).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var token = JObject.Parse(json);
                var accessToken = token["access_token"]?.ToString();

                if (!string.IsNullOrEmpty(accessToken))
                {
                    httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", accessToken);
                    IsTokenValid = true;
                    Svc.Log.Info("FFLogs: token acquired successfully.");
                    return;
                }
            }

            Svc.Log.Error($"FFLogs: token request failed ({response.StatusCode}). Check Client ID / Secret.");
        }
        catch (Exception ex)
        {
            Svc.Log.Error($"FFLogs: token error - {ex.Message}");
        }
    }

    /// <summary>
    /// Queries encounterRankings for a single character and returns best%, median%, kills.
    /// Returns null on network/parse failure; FFLogsData.IsHidden = true when the character
    /// has hidden logs.
    /// </summary>
    public async Task<Data.FFLogsData?> FetchEncounterData(
        string fullName, string worldName, ushort worldId, int encounterId)
    {
        if (!IsTokenValid) return null;

        var parts = fullName.Split(' ', 2);
        if (parts.Length < 2) return null;

        var firstName = parts[0];
        var lastName = parts[1];
        var region = Util.WorldToRegion(worldId);
        var worldSlug = worldName.ToLower();
        var innerQuery = $"query {{characterData{{character(name: \\\"{firstName} {lastName}\\\" serverSlug: \\\"{worldSlug}\\\" serverRegion: \\\"{region}\\\"){{hidden encounterRankings(encounterID: {encounterId})}}}}}}";
        var query = $"{{\"query\":\"{innerQuery}\"}}";

        Svc.Log.Debug($"FFLogs query body: {query}");

        try
        {
            var content = new StringContent(query, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(
                "https://www.fflogs.com/api/v2/client", content).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                Svc.Log.Error($"FFLogs: API request failed ({response.StatusCode}).");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var result = JObject.Parse(json);
            var character = result["data"]?["characterData"]?["character"];

            if (character == null || character.Type == JTokenType.Null)
            {
                Svc.Log.Debug($"FFLogs: character not found - {firstName} {lastName}@{worldName}");
                return null;
            }

            if (character["hidden"]?.ToObject<bool>() == true)
                return new Data.FFLogsData(null, null, null, IsHidden: true);

            var rankingsToken = character["encounterRankings"];
            if (rankingsToken == null || rankingsToken.Type == JTokenType.Null)
                return null;

            JObject rankings = rankingsToken.Type == JTokenType.String
                ? JObject.Parse(rankingsToken.ToString())
                : (JObject)rankingsToken;

            return new Data.FFLogsData(
                BestParse: rankings["ranks"] is JArray { Count: > 0 } ranksArr ? ranksArr[0]?["rankPercent"]?.ToObject<float>() : null,
                MedianParse: rankings["medianPerformance"]?.ToObject<float?>(),
                Kills: rankings["totalKills"]?.ToObject<int?>()
            );
        }
        catch (Exception ex)
        {
            Svc.Log.Error($"FFLogs: fetch error - {ex.Message}");
            return null;
        }
    }

    public void Dispose()
    {
        httpClient.Dispose();
    }
}
