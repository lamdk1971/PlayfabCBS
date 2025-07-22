using PlayFab;
using PlayFab.AdminModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class TitleDataSyncTool
{
    private string sourceTitleId;
    private string destTitleId;
    private string adminKey;

    public TitleDataSyncTool(string sourceTitleId, string destTitleId, string adminKey)
    {
        this.sourceTitleId = sourceTitleId;
        this.destTitleId = destTitleId;
        this.adminKey = adminKey;
    }

    public async Task SyncAllData()
    {
        try
        {
            // 1. Sync Title Data
            await SyncTitleData();

            // 2. Sync Player Data
            await SyncPlayerData();

            // 3. Sync Virtual Currency
            await SyncVirtualCurrency();

            // 4. Sync Inventory
            await SyncInventory();

            // 5. Sync CloudScript
            await SyncCloudScript();

            // 6. Sync Economy Items
            await SyncEconomyItems();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error syncing data: {ex.Message}");
        }
    }

    private async Task SyncTitleData()
    {
        // Setup source title
        PlayFabSettings.staticSettings.TitleId = sourceTitleId;
        PlayFabSettings.staticSettings.DeveloperSecretKey = adminKey;

        // Get source data
        var sourceRequest = new GetTitleDataRequest();
        var sourceResult = await PlayFabAdminAPI.GetTitleDataAsync(sourceRequest);

        // Setup destination title
        PlayFabSettings.staticSettings.TitleId = destTitleId;

        // Set data in destination
        foreach (var kvp in sourceResult.Result.Data)
        {
            var request = new SetTitleDataRequest
            {
                Key = kvp.Key,
                Value = kvp.Value
            };
            await PlayFabAdminAPI.SetTitleDataAsync(request);
        }
    }

    private async Task SyncPlayerData()
    {
        // Get all players from source
        var request = new GetPlayersInSegmentRequest
        {
            SegmentId = "all_players", // Need to create this segment first
            MaxBatchSize = 100
        };
        
        var players = await PlayFabAdminAPI.GetPlayersInSegmentAsync(request);

        // For each player, sync their data
        foreach (var player in players.Result.PlayerProfiles)
        {
            await SyncSinglePlayerData(player.PlayerId);
        }
    }

    private async Task SyncSinglePlayerData(string playerId)
    {
        // Get player data from source
        var request = new GetUserDataRequest
        {
            PlayFabId = playerId
        };

        var sourceData = await PlayFabAdminAPI.GetUserDataAsync(request);

        // Set in destination
        // Note: Need to handle player mapping between titles
        var updateRequest = new UpdateUserDataRequest
        {
            PlayFabId = playerId, // Need to map to new PlayFabId
            Data = sourceData.Result.Data
        };

        await PlayFabAdminAPI.UpdateUserDataAsync(updateRequest);
    }

    private async Task SyncVirtualCurrency()
    {
        // Similar implementation for virtual currency
    }

    private async Task SyncInventory()
    {
        // Similar implementation for inventory items
    }

    private async Task SyncCloudScript()
    {
        // Similar implementation for CloudScript
    }

    private async Task SyncEconomyItems()
    {
        // Similar implementation for economy items
    }
} 