using AutoHook.Data;
using AutoHook.Utils;
using FFXIVClientStructs.FFXIV.Client.Game;
using AutoHook.Resources.Localization;

namespace AutoHook.Classes;

#region AutoMakeShiftBait
public sealed class AutoMakeShiftBait : BaseActionCast
{
    public int MakeshiftBaitStacks = 5;

    public AutoMakeShiftBait() : base(UIStrings.MakeShift_Bait, IDs.Actions.MakeshiftBait, ActionType.Spell)
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

    public AutoThaliaksFavor() : base(UIStrings.Thaliaks_Favor, IDs.Actions.ThaliaksFavor, ActionType.Spell)
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
    public bool OnlyUseWithIntuition = false;
    public AutoChum() : base(UIStrings.Chum, IDs.Actions.Chum, ActionType.Spell)
    {
        DoesCancelMooch = true;
    }

    public override bool CastCondition()
    {
        if (!PlayerResources.HasStatus(IDs.Status.FishersIntuition) && OnlyUseWithIntuition)
            return false;

        return true;
    }
}
#endregion

#region AutoFishEyes
public class AutoFishEyes : BaseActionCast
{

    public AutoFishEyes() : base(UIStrings.Fish_Eyes, IDs.Actions.FishEyes, ActionType.Spell)
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
    // this option is based on the custom BaitConfig, not the AutoCast tab
    public AutoIdenticalCast() : base(UIStrings.Identical_Cast, Data.IDs.Actions.IdenticalCast, ActionType.Spell)
    {
        Enabled = true;
    }

    public override bool CastCondition()
    {
        return _baitConfig?.UseIdenticalCast ?? false;
    }
}
#endregion

#region AutoSurfaceSlap
public sealed class AutoSurfaceSlap : BaseActionCast
{

    // this option is based on the BaitConfig, not the AutoCast tab
    public AutoSurfaceSlap() : base(UIStrings.Surface_Slap, Data.IDs.Actions.SurfaceSlap, ActionType.Spell)
    {
        Enabled = true;
    }
    public override bool CastCondition()
    {
        return _baitConfig?.UseSurfaceSlap ?? false; ;
    }
}
#endregion

#region AutoPrizeCatch
public class AutoPrizeCatch : BaseActionCast
{

    public bool UseWhenMoochIIOnCD = false;

    public bool UseOnlyWithIdenticalCast = false;


    public AutoPrizeCatch() : base(UIStrings.Prize_Catch, Data.IDs.Actions.PrizeCatch, ActionType.Spell)
    {
        DoesCancelMooch = true;
    }


    public override bool CastCondition()
    {
        if (!Enabled)
            return false;

        if (UseWhenMoochIIOnCD && PlayerResources.ActionAvailable(IDs.Actions.Mooch2))
            return false;

        if (UseOnlyWithIdenticalCast && !PlayerResources.HasStatus(IDs.Status.IdenticalCast))
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
    public AutoPatienceI() : base(UIStrings.Patience_I, Data.IDs.Actions.Patience, ActionType.Spell)
    {
        DoesCancelMooch = true;
    }

    public override bool CastCondition()
    {
        if (PlayerResources.HasStatus(IDs.Status.AnglersFortune))
            return false;

        if (PlayerResources.HasStatus(IDs.Status.PrizeCatch))
            return false;

        if (PlayerResources.HasStatus(IDs.Status.MakeshiftBait) && !AcConfig.EnableMakeshiftPatience)
            return false;

        return true;
    }
}
#endregion

#region AutoPatienceII
public class AutoPatienceII : BaseActionCast
{
    public AutoPatienceII() : base(UIStrings.Patience_II, Data.IDs.Actions.Patience2, ActionType.Spell)
    {
        DoesCancelMooch = true;
    }

    public override bool CastCondition()
    {
        if (PlayerResources.HasStatus(IDs.Status.AnglersFortune))
            return false;

        if (PlayerResources.HasStatus(IDs.Status.PrizeCatch))
            return false;

        if (PlayerResources.HasStatus(IDs.Status.MakeshiftBait) && !AcConfig.EnableMakeshiftPatience)
            return false;

        return true;
    }
}
#endregion

#region AutoDoubleHook
public sealed class AutoDoubleHook : BaseActionCast
{
    public AutoDoubleHook() : base(UIStrings.Double_Hook, Data.IDs.Actions.DoubleHook, ActionType.Spell)
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
    public AutoTripleHook() : base(UIStrings.Triple_Hook, Data.IDs.Actions.TripleHook, ActionType.Spell)
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
    public AutoHICordial() : base(UIStrings.Hi_Cordial, IDs.Item.HiCordial, ActionType.Item)
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

    public override void SetThreshold(uint newCost)
    {
        if (newCost <= 1)
            GPThreshold = 1;
        else
            GPThreshold = newCost;
    }
}
#endregion

#region Cordial
public class AutoCordial : BaseActionCast
{
    readonly uint itemGPRecovery = 300;
    public AutoCordial() : base(UIStrings.Cordial, Data.IDs.Item.Cordial, ActionType.Item)
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

    public override void SetThreshold(uint newCost)
    {
        if (newCost <= 1)
            GPThreshold = 1;
        else
            GPThreshold = newCost;
    }
}
#endregion

#region HQCordial
public class AutoHQCordial : BaseActionCast
{
    readonly uint itemGPRecovery = 350;
    public AutoHQCordial() : base(UIStrings.HQ_Cordial, IDs.Item.HQCordial, ActionType.Item)
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

    public override void SetThreshold(uint newCost)
    {
        if (newCost <= 1)
            GPThreshold = 1;
        else
            GPThreshold = newCost;
    }
}
#endregion

#region WateredCordial
public class AutoWateredCordial : BaseActionCast
{
    readonly uint itemGPRecovery = 150;
    public AutoWateredCordial() : base(UIStrings.Watered_Cordial, Data.IDs.Item.WateredCordial, ActionType.Item)
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

    public override void SetThreshold(uint newCost)
    {
        if (newCost <= 1)
            GPThreshold = 1;
        else
            GPThreshold = newCost;
    }
}
#endregion

#region HQWateredCordial
public class AutoHQWateredCordial : BaseActionCast
{
    readonly uint itemGPRecovery = 200;
    public AutoHQWateredCordial() : base(UIStrings.HQ_Watered_Cordial, IDs.Item.HQWateredCordial, ActionType.Item)
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

    public override void SetThreshold(uint newCost)
    {
        if (newCost <= 1)
            GPThreshold = 1;
        else
            GPThreshold = newCost;
    }
}
#endregion
