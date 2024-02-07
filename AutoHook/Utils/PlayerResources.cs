using System;
using System.Collections.Generic;
using System.Linq;
using AutoHook.Classes;
using AutoHook.Data;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel.GeneratedSheets;
using LuminaAction = Lumina.Excel.GeneratedSheets.Action;
using Task = System.Threading.Tasks.Task;

namespace AutoHook.Utils;

public class PlayerResources : IDisposable
{
    public static List<BaitFishClass> Baits { get; set; } = new();
    public static List<BaitFishClass> Fishes { get; set; } = new();

    public void Initialize()
    {
        //Service.GameInteropProvider.InitializeFromAttributes(this);
        
        Baits = Service.DataManager.GetExcelSheet<Item>()?
                    .Where(i => i.ItemSearchCategory.Row == BaitFishClass.FishingTackleRow)
                    .Select(b => new BaitFishClass(b))
                    .ToList()
                ?? new List<BaitFishClass>();
        
        Fishes = Service.DataManager.GetExcelSheet<FishParameter>()?
                     .Where(f => f.Item != 0 && f.Item < 1000000)
                     .Select(f => new BaitFishClass(f))
                     .GroupBy(f => f.Id)
                     .Select(group => group.First())
                     .ToList()
                 ?? new List<BaitFishClass>();
    }
    
    public void Dispose()
    {
        //receiveActionEffectHook?.Disable();
    }

    public static bool IsMoochAvailable()
    {
        if (ActionTypeAvailable(IDs.Actions.Mooch))
            return true;

        if (ActionTypeAvailable(IDs.Actions.Mooch2))
            return true;

        return false;
    }

    public static bool HasStatus(uint statusID)
    {
        if (Service.ClientState.LocalPlayer?.StatusList == null)
            return false;

        foreach (var buff in Service.ClientState.LocalPlayer.StatusList)
        {
            if (buff.StatusId == statusID)
                return true;
        }

        return false;
    }

    public unsafe static bool IsInActiveSpectralCurrent() => EventFramework.Instance()->GetInstanceContentOceanFishing()->SpectralCurrentActive;

    public static uint GetCurrentGp()
    {
        if (Service.ClientState.LocalPlayer?.CurrentGp == null)
            return 0;

        return Service.ClientState.LocalPlayer.CurrentGp;
    }

    public static uint GetMaxGp()
    {
        if (Service.ClientState.LocalPlayer?.MaxGp == null)
            return 0;

        return Service.ClientState.LocalPlayer.MaxGp;
    }

    public static bool HasAnglersArtStacks(int amount)
    {
        if (Service.ClientState.LocalPlayer?.StatusList == null)
            return false;

        foreach (var buff in Service.ClientState.LocalPlayer.StatusList)
        {
            if (buff.StatusId == IDs.Status.AnglersArt)
                return buff.StackCount >= amount;
        }

        return false;
    }
    
    public static float GetStatusTime(uint statusId)
    {
        if (Service.ClientState.LocalPlayer?.StatusList == null)
            return 0;

        foreach (var buff in Service.ClientState.LocalPlayer.StatusList)
        {
            if (buff.StatusId == statusId)
                return buff.RemainingTime;
        }

        return 0;
    }

    // status 0 == available to cast? not sure but it seems to be
    // Also make sure its the skill is not on cooldown (mainly for mooch2)
    public static unsafe bool ActionTypeAvailable(uint id, ActionType actionType = ActionType.Action)
    {
        return ActionStatus(id, actionType) == 0 && !ActionOnCoolDown(id, actionType);
    }
    
    public static unsafe bool IsCastAvailable()
    {
        return ActionStatus(IDs.Actions.Cast) == 0 && !ActionOnCoolDown(IDs.Actions.Cast) && !_blockCasting; 
    }

    public static unsafe bool ActionOnCoolDown(uint id, ActionType actionType = ActionType.Action)
    {
        var group = GetRecastGroups(id, actionType);

        if (group == -1) // Im assuming -1 recast group has no CD
            return false;

        var recastDetail = ActionManager.Instance()->GetRecastGroupDetail(group);

        return recastDetail->Total - recastDetail->Elapsed > 0;
    }

    public static unsafe uint ActionStatus(uint id, ActionType actionType = ActionType.Action)
    {
        return ActionManager.Instance()->GetActionStatus(actionType, id);
    }

    public static unsafe bool CastAction(uint id, ActionType actionType = ActionType.Action)
    {
        return ActionManager.Instance()->UseAction(actionType, id);
    }
    

    public static unsafe int GetRecastGroups(uint id, ActionType actionType = ActionType.Action)
    {
        return ActionManager.Instance()->GetRecastGroup((int)actionType, id);
    }

    public static unsafe void UseItems(uint id)
    {
        AgentInventoryContext.Instance()->UseItem(id);
    }

    // RecastGroup 68 = Cordial pots
    public static unsafe bool IsPotOffCooldown()
    {
        var recast = ActionManager.Instance()->GetRecastGroupDetail(68);
        return recast->Total - recast->Elapsed == 0; 
    }

    public static unsafe uint CastActionCost(uint id, ActionType actionType = ActionType.Action)
    {
        return (uint)ActionManager.GetActionCost(actionType, id, 0, 0, 0, 0);
    }

    public static unsafe float GetCooldown(uint id, ActionType actionType)
    {
        var group = GetRecastGroups(id, actionType);

        if (group == -1) // Im assuming -1 recast group has no CD
            return 0;

        var recast = ActionManager.Instance()->GetRecastGroupDetail(group);

        return recast->Total - recast->Elapsed;
    }

    public static unsafe bool HaveItemInInventory(uint id, bool isHQ = false)
        => InventoryManager.Instance()->GetInventoryItemCount(id, isHQ) > 0;

    public static unsafe bool HaveCordialInInventory(uint id)
    {
        return InventoryManager.Instance()->GetInventoryItemCount(id) > 0;
    }
    

    private static bool _blockCasting = false;
    
    public static void CastActionDelayed(uint actionId, ActionType actionType = ActionType.Action, string actionName = "")
    {
        if (_blockCasting)
            return;
        
        if (actionType == ActionType.Action)
        {
            if (!ActionTypeAvailable(actionId, actionType))
                return;
            _blockCasting = true;
            Service.PrintDebug(@$"[PlayerResources] Casting Action: {actionName}, Id: {actionId}");
            CastAction(actionId, actionType);
            DelayNextCast(actionId);
        }
        else if (actionType == ActionType.Item)
        {
            _blockCasting = true;
            Service.PrintDebug(@$"[PlayerResources] Using Item: {actionName}, Id: {actionId}");
            UseItems(actionId);
            DelayNextCast(actionId);
        }
    }

    private static bool _blockActionNoDelay = false;

    public static void CastActionNoDelay(uint id, ActionType actionType = ActionType.Action)
    {
        // sometimes it tries to cast the same action while, this prevents that
        if (_blockActionNoDelay)
            return;

        _blockActionNoDelay = true;
        if (actionType == ActionType.Action)
        {
            if (ActionTypeAvailable(id, actionType))
            {
                Service.PrintDebug(@$"[PlayerResources] Casting {id}");
                CastAction(id, actionType);
            }
        }
        else if (actionType == ActionType.Item)
        {
            UseItems(id);
        }

        _blockActionNoDelay = false;
    }

    public static async void DelayNextCast(uint actionId)
    {
        var delay = new Random().Next(Service.Configuration.DelayBetweenCastsMin, Service.Configuration.DelayBetweenCastsMax);

        await Task.Delay(delay + ConditionalDelay(actionId));
        
        _blockCasting = false;
    }

    private static int ConditionalDelay(uint id) =>
        id switch
        {
            IDs.Actions.ThaliaksFavor => 1100,
            IDs.Actions.MakeshiftBait => 1100,
            IDs.Actions.NaturesBounty => 1100,
            IDs.Item.Cordial => 1100,
            IDs.Item.HQCordial => 1100,
            IDs.Item.HiCordial => 1100,
            IDs.Item.WateredCordial => 1100,
            IDs.Item.HQWateredCordial => 1100,
            _ => 0,
        };
}