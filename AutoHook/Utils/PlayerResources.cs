using System;
using System.Threading.Tasks;
using AutoHook.Data;
using Dalamud.Logging;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace AutoHook.Utils;

public class PlayerResources
{
    private static unsafe IntPtr ItemContextMenuAgent => (IntPtr)Framework.Instance()->GetUiModule()->GetAgentModule()->GetAgentByInternalId(AgentId.InventoryContext);

    private static unsafe ActionManager* _actionManager = ActionManager.Instance();

    [Signature("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 41 B0 01 BA 13 00 00 00")]
    private static unsafe delegate* unmanaged<IntPtr, uint, uint, uint, short, void> useItem = null;

    public static void Initialize()
    {
        SignatureHelper.Initialise(new PlayerResources()); 
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

    public static uint GetCurrentGP()
    {
        if (Service.ClientState.LocalPlayer?.CurrentGp == null)
            return 0;

        return Service.ClientState.LocalPlayer.CurrentGp;
    }

    public static uint GetMaxGP()
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
    // Also make sure its the skill is not on cooldown (maily for mooch2)
    public static unsafe bool ActionAvailable(uint id, ActionType actionType = ActionType.Spell)
        => _actionManager->GetActionStatus(actionType, id) == 0 && !ActionManager.Instance()->IsRecastTimerActive(ActionType.Spell, id);

    public static unsafe uint ActionStatus(uint id, ActionType actionType = ActionType.Spell)
       => _actionManager->GetActionStatus(actionType, id);

    public static unsafe bool CastAction(uint id, ActionType actionType = ActionType.Spell)
        => _actionManager->UseAction(actionType, id);

    public static unsafe void UseItem(uint id)
    {
        try
        {
            if (ItemContextMenuAgent != IntPtr.Zero)
            {
                useItem(ItemContextMenuAgent, id, 9999, 0, 0);
            }
        }
        catch (Exception e)
        {
            PluginLog.Error(e.ToString());
        }
    }

    public static unsafe float PotionCooldown()
    {
        // note: potions have recast group 58; however, for some reason periodically GetRecastGroup for them returns -1...
        var recast = _actionManager->GetRecastGroupDetail(58);
        return recast->Total - recast->Elapsed;
    }

    public static unsafe bool HaveItemInInventory(uint id)
    {

        return FFXIVClientStructs.FFXIV.Client.Game.InventoryManager.Instance()->GetInventoryItemCount(id % 1000000, id >= 1000000, false, false) > 0;
    }

    static bool isCasting = false;
    public static async void CastActionDelayed(uint id, ActionType actionType = ActionType.Spell, int delay = 0)
    {
        if (isCasting)
        {
            return;
        }

        if (delay <= 0)
            delay = new Random().Next(800, 1000);

        isCasting = true;

        await Task.Delay(delay);
        if (ActionAvailable(id, actionType))
        {
            if (actionType == ActionType.Spell)
            {
                CastAction(id, actionType);
            }
            else if (actionType == ActionType.Item)
            {
                UseItem(id);
            }
        }

        isCasting = false;
    }
}
