using System.Threading.Tasks;
using AutoHook.Data;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoHook.Utils;

public static class PlayerResources
{
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
            if (buff.StatusId == IDs.Status.AnglersFortune)
                return buff.StackCount >= amount;
        }

        return false;
    }

    public static unsafe bool ActionAvailable(uint id)
    {
        // status 0 == available to cast? not sure but it seems to be
        // Also make sure its the skill is not on cooldown (maily for mooch2)
        return ActionManager.Instance()->GetActionStatus(ActionType.Spell, id) == 0 && !ActionManager.Instance()->IsRecastTimerActive(ActionType.Spell, id);
    }

    public static unsafe uint ActionStatus(uint id)
    {
        // status 0 == available to cast? not sure but it seems to be
        // Also make sure its the skill is not on cooldown (maily for mooch2)
        return ActionManager.Instance()->GetActionStatus(ActionType.Spell, id);
    }

    public static unsafe bool CastAction(uint id, ActionType actionType = ActionType.Spell)
    {
        return ActionManager.Instance()->UseAction(actionType, id);
    }

    static bool isCasting = false;
    public static async void CastActionDelayed(uint id, int delay, ActionType actionType = ActionType.Spell)
    {   
        if (isCasting)
            return;

        isCasting = true;
        await Task.Delay(delay);

        if (ActionAvailable(id)) 
            CastAction(id, actionType);

        isCasting = false;
    }
}
