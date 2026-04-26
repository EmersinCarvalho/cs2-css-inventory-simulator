/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace InventorySimulator;

public class VipService
{
    private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(10) };
    private readonly HashSet<ulong> _vipList = [];
    private DateTime _lastUpdate = DateTime.MinValue;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromSeconds(30);
    private bool _isUpdating = false;

    public bool IsVip(ulong steamId)
    {
        if (!ConVars.VipEnabled.Value || string.IsNullOrEmpty(ConVars.VipApiUrl.Value))
            return false;
        
        return _vipList.Contains(steamId);
    }

    public async Task UpdateVipListAsync()
    {
        // Prevenir múltiplas atualizações simultâneas
        if (_isUpdating)
        {
            CSS.Plugin.Logger.LogInformation("VIP update already in progress, skipping...");
            return;
        }

        // Verificar se o cache ainda é válido
        if (DateTime.UtcNow - _lastUpdate < _cacheExpiration)
        {
            CSS.Plugin.Logger.LogInformation("VIP cache still valid, skipping update...");
            return;
        }

        if (!ConVars.VipEnabled.Value)
        {
            CSS.Plugin.Logger.LogInformation("VIP system is disabled.");
            return;
        }

        var apiUrl = ConVars.VipApiUrl.Value;
        if (string.IsNullOrEmpty(apiUrl))
        {
            CSS.Plugin.Logger.LogWarning("VIP API URL not configured.");
            return;
        }

        _isUpdating = true;

        try
        {
            CSS.Plugin.Logger.LogInformation("Fetching VIP list from {Url}...", apiUrl);
            var response = await _httpClient.GetAsync(apiUrl);
            
            if (!response.IsSuccessStatusCode)
            {
                CSS.Plugin.Logger.LogError(
                    "Failed to fetch VIP list from {Url}. Status code: {StatusCode}",
                    apiUrl,
                    response.StatusCode
                );
                return;
            }

            var vipResponse = await response.Content.ReadFromJsonAsync<VipResponse>();
            
            if (vipResponse?.Vips == null)
            {
                CSS.Plugin.Logger.LogError("Invalid VIP response format from {Url}", apiUrl);
                return;
            }

            _vipList.Clear();
            foreach (var steamId in vipResponse.Vips)
            {
                _vipList.Add(steamId);
            }

            _lastUpdate = DateTime.UtcNow;
            CSS.Plugin.Logger.LogInformation(
                "VIP list refreshed successfully. {Count} VIPs loaded.",
                _vipList.Count
            );
        }
        catch (Exception ex)
        {
            CSS.Plugin.Logger.LogError(
                "Error fetching VIP list from {Url}: {Message}",
                apiUrl,
                ex.Message
            );
        }
        finally
        {
            _isUpdating = false;
        }
    }

    public int GetVipCount()
    {
        return _vipList.Count;
    }
}
