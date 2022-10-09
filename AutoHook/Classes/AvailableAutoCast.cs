using AutoHook.Configurations;
using AutoHook.Data;
using AutoHook.Utils;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using System;

namespace AutoHook.Classes;

#region AutoMakeShiftBait
public sealed class AutoMakeShiftBait : BaseActionCast
{
    public int MakeshiftBaitStacks = 5;

    public AutoMakeShiftBait() : base("熟练渔技", IDs.Actions.MakeshiftBait, ActionType.Spell)
    {

    }
    public override bool CastCondition()
    {
        if (!Enabled)
            return false;

        if (PlayerResources.HasStatus(IDs.Status.MakeshiftBait))
            return false;

        if (PlayerResources.HasStatus(IDs.Status.PrizeCatch))
            return false;

        if (PlayerResources.HasStatus(IDs.Status.AnglersFortune))
            return false;


        bool available = PlayerResources.ActionAvailable(IDs.Actions.MakeshiftBait);
        bool hasStacks = PlayerResources.HasAnglersArtStacks(MakeshiftBaitStacks);

        return hasStacks && available;
    }
}
#endregion

#region AutoThaliaksFavor
public sealed class AutoThaliaksFavor : BaseActionCast
{
    public int ThaliaksFavorStacks = 3;
    public int ThaliaksFavorRecover = 150;

    public AutoThaliaksFavor() : base("沙利亚克的恩宠", IDs.Actions.ThaliaksFavor, ActionType.Spell)
    {

    }

    public override bool CastCondition()
    {
        bool hasStacks = PlayerResources.HasAnglersArtStacks(ThaliaksFavorStacks);

        bool notOvercaped = (PlayerResources.GetCurrentGP() + ThaliaksFavorRecover) < PlayerResources.GetMaxGP();

        return hasStacks && notOvercaped; // dont use if its going to overcap gp
    }
}
#endregion

#region AutoChum
public sealed class AutoChum : BaseActionCast
{
    public AutoChum() : base("撒饵", IDs.Actions.Chum, ActionType.Spell)
    {
        DoesCancelMooch = true;
    }

    public override bool CastCondition()
    {
        return true;
    }
}
#endregion

#region AutoFishEyes
public class AutoFishEyes : BaseActionCast
{

    public AutoFishEyes() : base("鱼眼", IDs.Actions.FishEyes, ActionType.Spell)
    {
        DoesCancelMooch = true;
    }

    public override bool CastCondition()
    {
        return true;
    }
}
#endregion

#region IdenticalCast
public sealed class AutoIdenticalCast : BaseActionCast
{
    // this option is based on the custom HookConfig, not the AutoCast tab
    public AutoIdenticalCast() : base("专一垂钓", Data.IDs.Actions.IdenticalCast, ActionType.Spell)
    {
        Enabled = true;
    }

    public override bool CastCondition()
    {
        return HookConfig?.UseIdenticalCast ?? false;
    }
}
#endregion

#region AutoSurfaceSlap
public sealed class AutoSurfaceSlap : BaseActionCast
{

    // this option is based on the custom HookConfig, not the AutoCast tab
    public AutoSurfaceSlap() : base("拍击水面", Data.IDs.Actions.SurfaceSlap, ActionType.Spell)
    {
        Enabled = true;
    }
    public override bool CastCondition()
    {
        return HookConfig?.UseSurfaceSlap ?? false; ;
    }
}
#endregion

#region AutoPrizeCatch
public class AutoPrizeCatch : BaseActionCast
{

    public AutoPrizeCatch() : base("大鱼猎手", Data.IDs.Actions.PrizeCatch, ActionType.Spell)
    {
        DoesCancelMooch = true;
    }

    public override bool CastCondition()
    {
        if (!Enabled)
            return false;

        if (PlayerResources.HasStatus(IDs.Status.MakeshiftBait))
            return false;

        if (PlayerResources.HasStatus(IDs.Status.PrizeCatch))
            return false;

        if (PlayerResources.HasStatus(IDs.Status.AnglersFortune))
            return false;

        return PlayerResources.ActionAvailable(IDs.Actions.PrizeCatch);
    }
}
#endregion

#region AutoPatienceI
public class AutoPatienceI : BaseActionCast
{
    public AutoPatienceI() : base("耐心I", Data.IDs.Actions.Patience, ActionType.Spell)
    {
        DoesCancelMooch = true;
    }

    public override bool CastCondition()
    {
        if (PlayerResources.HasStatus(IDs.Status.AnglersFortune))
            return false;

        if (PlayerResources.HasStatus(IDs.Status.PrizeCatch))
            return false;

        if (PlayerResources.HasStatus(IDs.Status.MakeshiftBait) && !AutoCastsConfig.EnableMakeshiftPatience)
            return false;

        return true;
    }
}
#endregion

#region AutoPatienceII
public class AutoPatienceII : BaseActionCast
{
    public AutoPatienceII() : base("耐心II", Data.IDs.Actions.Patience2, ActionType.Spell)
    {
        DoesCancelMooch = true;
    }

    public override bool CastCondition()
    {
        if (PlayerResources.HasStatus(IDs.Status.AnglersFortune))
            return false;

        if (PlayerResources.HasStatus(IDs.Status.PrizeCatch))
            return false;

        if (PlayerResources.HasStatus(IDs.Status.MakeshiftBait) && !AutoCastsConfig.EnableMakeshiftPatience)
            return false;

        return true;
    }
}
#endregion

#region AutoDoubleHook
public sealed class AutoDoubleHook : BaseActionCast
{
    public AutoDoubleHook() : base("双重提钩", Data.IDs.Actions.DoubleHook, ActionType.Spell)
    {

    }
    public override bool CastCondition()
    {
        return true;
    }
}
#endregion

#region AutoTripleHook
public sealed class AutoTripleHook : BaseActionCast
{
    public AutoTripleHook() : base("三重提钩", Data.IDs.Actions.TripleHook, ActionType.Spell)
    {

    }
    public override bool CastCondition()
    {
        return true;
    }
}
#endregion


#region HICordial
public class AutoHICordial : BaseActionCast
{
    readonly uint itemGPRecovery = 400;
    public AutoHICordial() : base("高级强心剂", IDs.Item.HiCordial, ActionType.Item)
    {
        GPThreshold = 1;
    }

    public override bool CastCondition()
    {
        if (!PlayerResources.HaveItemInInventory(ID))
            return false;

        bool notOvercaped = (PlayerResources.GetCurrentGP() + itemGPRecovery < PlayerResources.GetMaxGP());

        return notOvercaped && PlayerResources.IsPotOffCooldown();
    }

    public override void SetThreshold(uint newcost)
    {
        if (newcost <= 1)
            GPThreshold = 1;
        else
            GPThreshold = newcost;
    }
}
#endregion

#region Cordial
public class AutoCordial : BaseActionCast
{
    readonly uint itemGPRecovery = 300;
    public AutoCordial() : base("强心剂", Data.IDs.Item.Cordial, ActionType.Item)
    {
        GPThreshold = 1;
    }

    public override bool CastCondition()
    {
        if (!PlayerResources.HaveItemInInventory(ID))
            return false;

        bool notOvercaped = (PlayerResources.GetCurrentGP() + itemGPRecovery < PlayerResources.GetMaxGP());

        return notOvercaped && PlayerResources.IsPotOffCooldown();
    }

    public override void SetThreshold(uint newcost)
    {
        if (newcost <= 1)
            GPThreshold = 1;
        else
            GPThreshold = newcost;
    }
}
#endregion

#region HQCordial
public class AutoHQCordial : BaseActionCast
{
    readonly uint itemGPRecovery = 350;
    public AutoHQCordial() : base("强心剂HQ", IDs.Item.HQCordial, ActionType.Item)
    {
        GPThreshold = 1;
    }

    public override bool CastCondition()
    {
        if (!PlayerResources.HaveItemInInventory(ID))
            return false;

        bool notOvercaped = (PlayerResources.GetCurrentGP() + itemGPRecovery < PlayerResources.GetMaxGP());

        return notOvercaped && PlayerResources.IsPotOffCooldown();
    }

    public override void SetThreshold(uint newcost)
    {
        if (newcost <= 1)
            GPThreshold = 1;
        else
            GPThreshold = newcost;
    }
}
#endregion