using System;
using AutoHook.Data;
using Dalamud.Logging;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

using LuminaAction = Lumina.Excel.GeneratedSheets.Action;
using Task = System.Threading.Tasks.Task;

namespace AutoHook.Utils;

public class PlayerResources : IDisposable
{
    //private static Lumina.Excel.ExcelSheet<LuminaAction> actionSheet = Service.DataManager.GetExcelSheet<LuminaAction>()!;

    private static unsafe IntPtr ItemContextMenuAgent => (IntPtr)Framework.Instance()->GetUiModule()->GetAgentModule()->GetAgentByInternalId(AgentId.InventoryContext);

    private static readonly unsafe ActionManager* ActionManager = FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance();

    [Signature("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 89 7C 24 38")]
    private static unsafe delegate* unmanaged<IntPtr, uint, uint, uint, short, void> _useItem = null;
    /*
    [Signature("E8 ?? ?? ?? ?? 48 8B 8D F0 03 00 00", DetourName = nameof(ReceiveActionEffectDetour))]
    private readonly Hook<ReceiveActionEffectDelegate>? receiveActionEffectHook = null;
    private delegate void ReceiveActionEffectDelegate(int sourceObjectId, IntPtr sourceActor, IntPtr position, IntPtr effectHeader, IntPtr effectArray, IntPtr effectTrail);*/

    public void Initialize()
    {
        Service.GameInteropProvider.InitializeFromAttributes(this);
        EnableHooks();
    }

    public void EnableHooks()
    {
        // Not using this rn because it didnt work like i expected
        // I was trying to wait for a server response after an action is used but, but this is not quite right
        //receiveActionEffectHook?.Enable();
    }
    public void Dispose()
    {
        //receiveActionEffectHook?.Disable();
    }


    public static bool IsMoochAvailable()
    {
        if (ActionAvailable(IDs.Actions.Mooch))
            return true;

        else if (ActionAvailable(IDs.Actions.Mooch2))
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

    // status 0 == available to cast? not sure but it seems to be
    // Also make sure its the skill is not on cooldown (mainly for mooch2)
    public static unsafe bool ActionAvailable(uint id, ActionType actionType = ActionType.Action)
    {
        if (actionType == ActionType.Item)
            return true;

        return ActionManager->GetActionStatus(actionType, id) == 0 && !FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance()->IsRecastTimerActive(ActionType.Action, id);
    }

    public static unsafe uint ActionStatus(uint id, ActionType actionType = ActionType.Action)
        => ActionManager->GetActionStatus(actionType, id);

    public static unsafe bool CastAction(uint id, ActionType actionType = ActionType.Action)
        => ActionManager->UseAction(actionType, id);

    public static unsafe int GetRecastGroups(uint id, ActionType actionType = ActionType.Action)
    => ActionManager->GetRecastGroup((int)actionType, id);

    public static unsafe void UseItems(uint id)
    {
        try
        {
            if (ItemContextMenuAgent != IntPtr.Zero)
            {
                _useItem(ItemContextMenuAgent, id, 9999, 0, 0);
            }
        }
        catch (Exception e)
        {
            PluginLog.Error(e.ToString());
        }
    }

    // RecastGroup 68 = Cordial pots
    public static unsafe bool IsPotOffCooldown()
    {
        var recast = ActionManager->GetRecastGroupDetail(68);
        return recast->Total - recast->Elapsed == 0;
    }

    public static uint CastActionCost(uint id, ActionType actionType = ActionType.Action)
        => (uint)FFXIVClientStructs.FFXIV.Client.Game.ActionManager.GetActionCost(ActionType.Action, id, 0, 0, 0, 0);

    public static unsafe float GetPotCooldown()
    {
        var recast = ActionManager->GetRecastGroupDetail(68);
        return recast->Total - recast->Elapsed;
    }

    public static unsafe bool HaveItemInInventory(uint id, bool isHQ = false)
       => InventoryManager.Instance()->GetInventoryItemCount(id, isHQ) > 0;

    private static bool _isCastingDelay = false;
    private static uint _nextActionId = 0;
    private static uint _lastActionId = 0;
    private static int _delay = 0;

    public static void CastActionDelayed(uint id, ActionType actionType = ActionType.Action, int setDelay = 0)
    {
        if (_isCastingDelay)
            return;

        _delay = setDelay;

        _nextActionId = id;

        if (actionType == ActionType.Action)
        {
            if (ActionAvailable(_nextActionId, actionType))
            {
                _isCastingDelay = true;
                if (CastAction(_nextActionId, actionType))
                {
                    ResetAutoCast();
                }
                else
                {
                    ResetAutoCast();
                }
            }

        }
        else if (actionType == ActionType.Item)
        {
            UseItems(_nextActionId);
            ResetAutoCast();
        }
    }

    private static bool _isCastingNoDelay = false;

    public static void CastActionNoDelay(uint id, ActionType actionType = ActionType.Action)
    {
        if (_isCastingNoDelay)
            return;

        _isCastingNoDelay = true;
        if (actionType == ActionType.Action)
        {
            if (ActionAvailable(id, actionType))
            {
                CastAction(id, actionType);
            }
        }
        else if (actionType == ActionType.Item)
        {
            UseItems(id);
        }

        _isCastingNoDelay = false;
    }

    public static async void ResetAutoCast()
    {
        if (_delay <= 0)
            _delay = new Random().Next(600, 700);

        _delay += ConditionalDelay();

        await Task.Delay(_delay);

        _lastActionId = _nextActionId;
        _nextActionId = 0;
        _isCastingDelay = false;
        _delay = 0;
    }

    private static int ConditionalDelay() =>
        _nextActionId switch
        {
            IDs.Actions.ThaliaksFavor => 1100,
            IDs.Actions.MakeshiftBait => 1100,
            IDs.Actions.NaturesBounty => 1100,
            IDs.Item.Cordial => 1100,
            IDs.Item.HQCordial => 1100,
            IDs.Item.HiCordial => 1100,
            _ => 0,
        };

}
