/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;

namespace InventorySimulator;

public partial class InventorySimulator
{
    [ConsoleCommand("css_ws", "Refreshes player's inventory.")]
    public void OnWSCommand(CCSPlayerController? player, CommandInfo _)
    {
        var url = UrlHelper.FormatUrl(ConVars.WsUrlPrintFormat.Value, ConVars.Url.Value);
        player?.PrintToChat(Localizer["invsim.announce", url]);
        if (!ConVars.IsWsEnabled.Value || player == null)
            return;
        var controllerState = player.GetState();
        
        // Cooldown diferenciado baseado em VIP
        var isVip = controllerState.IsVip();
        var cooldown = ConVars.VipEnabled.Value 
            ? (isVip ? ConVars.WsCooldownVip.Value : ConVars.WsCooldownNonVip.Value)
            : ConVars.WsCooldown.Value;
        
        var diff = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - controllerState.WsUpdatedAt;
        if (diff < cooldown)
        {
            player.PrintToChat(Localizer["invsim.ws_cooldown", cooldown - diff]);
            return;
        }
        if (controllerState.IsFetching)
        {
            player.PrintToChat(Localizer["invsim.ws_in_progress"]);
            return;
        }
        HandlePlayerInventoryRefresh(player, true);
        
        // Mensagem diferenciada para VIPs e não-VIPs (em português)
        if (ConVars.VipEnabled.Value)
        {
            if (isVip)
            {
                player.PrintToChat(" \x04[Inventory Simulator] \x01Inventário atualizado com sucesso! \x0BObrigado por ser VIP! 💎");
            }
            else
            {
                player.PrintToChat(" \x04[Inventory Simulator] \x01Inventário atualizado! \x06Quer cooldown reduzido e acesso a facas/luvas? \x0BSeja VIP!");
            }
        }
        else
        {
            player.PrintToChat(Localizer["invsim.ws_new"]);
        }
    }

    [ConsoleCommand("css_vips", "Updates the VIP list from API and shows online VIPs.")]
    public void OnVipsCommand(CCSPlayerController? player, CommandInfo _)
    {
        if (player == null || CSS.VipService == null)
            return;
        
        player.PrintToChat(" \x04[VIP] \x01Atualizando lista de VIPs...");
        
        // Executar atualização em background e mostrar VIPs online após
        Task.Run(async () =>
        {
            await CSS.VipService.UpdateVipListAsync();
            
            // Voltar para a thread principal para interagir com a API do jogo
            Server.NextFrame(() =>
            {
                if (player == null || !player.IsValid)
                    return;
                
                var vipCount = CSS.VipService.GetVipCount();
                player.PrintToChat($" \x04[VIP] \x01Lista atualizada! \x0B{vipCount} VIPs no total.");
                
                // Mostrar VIPs online
                var onlineVips = Utilities.GetPlayers()
                    .Where(p => p != null && p.IsValid && !p.IsBot && p.Connected == PlayerConnectedState.PlayerConnected)
                    .Where(p => p.GetState().IsVip())
                    .ToList();
                
                if (onlineVips.Count > 0)
                {
                    player.PrintToChat($" \x04[VIP] \x0B{onlineVips.Count} VIP(s) online:");
                    foreach (var vip in onlineVips)
                    {
                        player.PrintToChat($"  \x06• \x01{vip.PlayerName}");
                    }
                }
                else
                {
                    player.PrintToChat(" \x04[VIP] \x07Nenhum VIP online no momento.");
                }
            });
        });
    }

    [ConsoleCommand("css_spray", "Spray player's graffiti.")]
    public void OnSprayCommand(CCSPlayerController? player, CommandInfo _)
    {
        if (player != null && ConVars.IsSprayEnabled.Value)
        {
            var controllerState = player.GetState();
            var cooldown = ConVars.SprayCooldown.Value;
            var diff = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - controllerState.SprayUsedAt;
            if (diff < cooldown)
            {
                player.PrintToChat(Localizer["invsim.spray_cooldown", cooldown - diff]);
                return;
            }
            HandlePlayerGraffitiSpray(player);
        }
    }

    [ConsoleCommand("css_wslogin", "Authenticate player to Inventory Simulator.")]
    public void OnWsloginCommand(CCSPlayerController? player, CommandInfo _)
    {
        if (ConVars.IsWsLogin.Value && Api.HasApiKey() && player != null)
        {
            var controllerState = player.GetState();
            player.PrintToChat(Localizer["invsim.login_in_progress"]);
            if (controllerState.IsAuthenticating)
                return;
            HandleSignIn(player);
        }
    }
}
