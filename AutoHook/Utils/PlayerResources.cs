using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AutoHook.Data;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

using LuminaAction = Lumina.Excel.GeneratedSheets.Action;

namespace AutoHook.Utils;

public class PlayerResources : IDisposable
{
    private static Lumina.Excel.ExcelSheet<LuminaAction> actionSheet = Service.DataManager.GetExcelSheet<LuminaAction>()!;

    private static unsafe IntPtr ItemContextMenuAgent => (IntPtr)Framework.Instance()->GetUiModule()->GetAgentModule()->GetAgentByInternalId(AgentId.InventoryContext);

    private static readonly unsafe ActionManager* _actionManager = ActionManager.Instance();

    [Signature("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 41 B0 01 BA 13 00 00 00")]
    private static unsafe delegate* unmanaged<IntPtr, uint, uint, uint, short, void> useItem = null;

    [Signature("E8 ?? ?? ?? ?? 48 8B 8D F0 03 00 00", DetourName = nameof(ReceiveActionEffectDetour))]
    private readonly Hook<ReceiveActionEffectDelegate>? receiveActionEffectHook = null;
    private delegate void ReceiveActionEffectDelegate(int sourceObjectId, IntPtr sourceActor, IntPtr position, IntPtr effectHeader, IntPtr effectArray, IntPtr effectTrail);

    public void Initialize()
    {
        SignatureHelper.Initialise(this);
        EnableHooks();
    }

    public void EnableHooks()
    {
        //receiveActionEffectHook?.Enable();
    }
    public void Dispose()
    {
        //receiveActionEffectHook?.Disable();
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
    {
        if (actionType == ActionType.Item)
            return true;

        return _actionManager->GetActionStatus(actionType, id) == 0 && !ActionManager.Instance()->IsRecastTimerActive(ActionType.Spell, id);
    }

    public static unsafe uint ActionStatus(uint id, ActionType actionType = ActionType.Spell)
        => _actionManager->GetActionStatus(actionType, id);

    private static unsafe bool CastAction(uint id, ActionType actionType = ActionType.Spell)
        => _actionManager->UseAction(actionType, id);

    public static unsafe int GetRecastGroups(uint id, ActionType actionType = ActionType.Spell)
        => _actionManager->GetRecastGroup((int)actionType, id);

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

    // RecastGroup 68 = Cordial pots
    public static unsafe bool IsPotOffCooldown()
    {

        var recast = _actionManager->GetRecastGroupDetail(68);
        return recast->Total - recast->Elapsed == 0;
    }

    public static uint CastActionCost(uint id, ActionType actionType = ActionType.Spell)
        => (uint)ActionManager.GetActionCost(ActionType.Spell, id, 0, 0, 0, 0);

    public static unsafe float GetPotCooldown()
    {
        var recast = _actionManager->GetRecastGroupDetail(68);
        return recast->Total - recast->Elapsed;
    }

    public static unsafe bool HaveItemInInventory(uint id, bool isHQ = false)
       => InventoryManager.Instance()->GetInventoryItemCount(id, isHQ) > 0;


    static bool isCasting = false;
    static uint NextActionID = 0;
    static uint LastActionID = 0;
    static int delay = 0;

    public static void CastActionDelayed(uint id, ActionType actionType = ActionType.Spell, int setdelay = 0)
    {
        if (isCasting)
            return;

        delay = setdelay;
        NextActionID = id;

        if (actionType == ActionType.Spell)
        {
            if (ActionAvailable(NextActionID, actionType))
            {  

                isCasting = true;
                if (CastAction(NextActionID, actionType))
                {
                    PluginLog.Debug("Castingsuccess");
                    ResetAutoCast();
                } else
                {
                    PluginLog.Debug("--------Didnt cast---------");
                    ResetAutoCast();
                }
            }
        
        }
        else if (actionType == ActionType.Item)
        {
            UseItem(NextActionID);
            ResetAutoCast();
        }
    }

   
    private void ReceiveActionEffectDetour(int sourceObjectId, IntPtr sourceActor, IntPtr position, IntPtr effectHeader, IntPtr effectArray, IntPtr effectTrail)
    {
        receiveActionEffectHook!.Original(sourceObjectId, sourceActor, position, effectHeader, effectArray, effectTrail);

        /*ActionEffectHeader header = Marshal.PtrToStructure<ActionEffectHeader>(effectHeader);

        PluginLog.Debug("owoooooo?");
        if (sourceObjectId == Service.ClientState.LocalPlayer?.ObjectId)
        {
            if (header.ActionId == LastActionID)
            {
                PluginLog.Debug("Awaaaaaaaaaaaaa?");
                LastActionID = NextActionID;
                ResetAutoCast();
            }
        }*/
    }

    public static async void ResetAutoCast()
    {
        if (delay <= 0)
            delay = new Random().Next(600, 700);

        // ThaliaksFavor is a weird skill idk how this works so im just adding a lot of delay and hoping it stops being used twice
        if (NextActionID == IDs.Actions.ThaliaksFavor) 
            delay += 1100;

        await Task.Delay(delay);

        PluginLog.Debug("Reseting AutoCast");
        NextActionID = 0;
        isCasting = false;
        delay = 0;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct ActionEffectHeader
    {
        [FieldOffset(0x0)] public long TargetObjectId;
        [FieldOffset(0x8)] public uint ActionId;
        [FieldOffset(0x14)] public uint UnkObjectId;
        [FieldOffset(0x18)] public ushort Sequence;
        [FieldOffset(0x1A)] public ushort Unk_1A;
    }

}
