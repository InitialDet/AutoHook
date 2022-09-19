using System.Collections.Generic;
using AutoHook.Classes;
using AutoHook.Data;
using AutoHook.Utils;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoHook.Configurations;

public class AutoCastsConfig
{
    public bool EnableAll = false;
    public bool EnableAutoCast = false;
    public bool EnableMooch = false;

    public bool EnableMooch2 = false;

    public bool EnablePatience = false;
    public bool EnableMakeshiftPatience = false;

    public static bool DontCancelMooch = true;

    public uint SelectedPatienceID = IDs.Actions.Patience2; // Default to Patience2

    public AutoPatienceI AutoPatienceI = new();

    public AutoPatienceII AutoPatienceII = new();

    public AutoChum AutoChum = new();

    public AutoFishEyes AutoFishEyes = new();

    public AutoHICordial AutoHICordial = new();

    public AutoHQCordial AutoHQCordial = new();

    public AutoCordial AutoCordial = new();

    public AutoThaliaksFavor AutoThaliaksFavor = new();

    public AutoMakeShiftBait AutoMakeShiftBait = new();

    public AutoIdenticalCast AutoIdenticalCast = new();

    public AutoSurfaceSlap AutoSurfaceSlap = new();

    public AutoPrizeCatch AutoPrizeCatch = new();

    public HookConfig? HookConfig = null;

    public bool EnableCordials = false;

    public bool EnableCordialFirst = false;

    public static bool IsMoochAvailable = false;

    public AutoCast? GetNextAutoCast(HookConfig? hookConfig)
    {
        if (!EnableAll)
            return null;

        HookConfig = hookConfig;

        IsMoochAvailable = CheckMoochAvailable();

        if (!PlayerResources.ActionAvailable(IDs.Actions.Cast))
            return null;

        if (AutoThaliaksFavor.IsAvailableToCast(hookConfig))
            return new(AutoThaliaksFavor.ActionID, AutoThaliaksFavor.ActionType);

        if (AutoThaliaksFavor.IsAvailableToCast(hookConfig))
            return new(IDs.Actions.MakeshiftBait, ActionType.Spell);

        if (AutoChum.IsAvailableToCast(hookConfig))
            return new(IDs.Actions.Chum, ActionType.Spell);

         if (AutoFishEyes.IsAvailableToCast(hookConfig))
            return new(IDs.Actions.FishEyes, ActionType.Spell);

        if (AutoIdenticalCast.IsAvailableToCast(hookConfig))
            return new(IDs.Actions.IdenticalCast, ActionType.Spell);

        if (AutoSurfaceSlap.IsAvailableToCast(hookConfig))
            return new(IDs.Actions.SurfaceSlap, ActionType.Spell);

        if (AutoPrizeCatch.IsAvailableToCast(hookConfig))
             return new(IDs.Actions.PrizeCatch, ActionType.Spell);

        if (UsePatience()) // This cant be used if a mooch is available or it'll cancel it
            return new(SelectedPatienceID, ActionType.Spell);

        if (UsesCordials(out uint idCordial))
            return new(idCordial, ActionType.Item);

        if (UseMooch(out uint idMooch))
            return new(idMooch, ActionType.Spell);

        if (EnableAutoCast)
            return new(IDs.Actions.Cast, ActionType.Spell);

        return null;
    }

    private bool UseMooch(out uint id)
    {
        id = 0;

        bool useAutoMooch = false;
        bool useAutoMooch2 = false;

        if (HookConfig == null || HookConfig?.BaitName == "DefaultCast" || HookConfig?.BaitName == "DefaultMooch")
        {
            useAutoMooch = EnableMooch;
            useAutoMooch2 = EnableMooch2;
        }
        else
        {
            useAutoMooch = HookConfig?.UseAutoMooch ?? false;
            useAutoMooch2 = HookConfig?.UseAutoMooch2 ?? false;
        }

        if (useAutoMooch)
        {
            if (PlayerResources.ActionAvailable(IDs.Actions.Mooch))
            {
                id = IDs.Actions.Mooch;
                return true;
            }
            else if (useAutoMooch2 && PlayerResources.ActionAvailable(IDs.Actions.Mooch2))
            {
                id = IDs.Actions.Mooch2;
                return true;
            }
        }

        return false;
    }

    private bool CheckMoochAvailable()
    {
        if (PlayerResources.ActionAvailable(IDs.Actions.Mooch))  
            return true;
        
        else if (PlayerResources.ActionAvailable(IDs.Actions.Mooch2))
            return true;

        return false;
    }

    private bool UsePatience()
    {
        if (EnablePatience)
        {
            if (!PlayerResources.HasStatus(IDs.Status.AnglersFortune))
            {
                // Dont use Patience if mooch is available
                if (IsMoochAvailable)
                    return false;

                if (PlayerResources.HasStatus(IDs.Status.PrizeCatch))
                    return false;

                if (PlayerResources.HasStatus(IDs.Status.MakeshiftBait) && !EnableMakeshiftPatience)
                    return false;

                if (PlayerResources.ActionAvailable(SelectedPatienceID))
                {
                    if (SelectedPatienceID == IDs.Actions.Patience)
                        return PlayerResources.GetCurrentGP() >= (200 + 20);
                    if (SelectedPatienceID == IDs.Actions.Patience2)
                        return PlayerResources.GetCurrentGP() >= (560 + 20);
                }
            }
        }

        return false;
    }

    IDictionary<uint, int> cordialCost = new Dictionary<uint, int>()
            {
                {IDs.Item.Cordial,300},
                {IDs.Item.HQCordial, 350},
                {IDs.Item.HiCordial,400},
                {0,0}
            };
    private bool UsesCordials(out uint itemID)
    {
        itemID = 0;

        if (!EnableCordials)
            return false;

        bool useCordial = false;
        bool useHQCordial = false;
        bool useHICordial = false;

        if (PlayerResources.HaveItemInInventory(IDs.Item.HiCordial))
            useHICordial = true;

        if (PlayerResources.HaveItemInInventory(IDs.Item.Cordial, true))
            useHQCordial = true;

        if (PlayerResources.HaveItemInInventory(IDs.Item.Cordial))
            useCordial = true;

        if (EnableCordialFirst)
        {
            if (useHQCordial)
                itemID = IDs.Item.HQCordial;
            else if (useCordial)
                itemID = IDs.Item.Cordial;
            else if (useHICordial)
                itemID = IDs.Item.HiCordial;
        }
        else
        {
            if (useHICordial)
                itemID = IDs.Item.HiCordial;
            else if (useHQCordial)
                itemID = IDs.Item.HQCordial;
            else if (useCordial)
                itemID = IDs.Item.Cordial;
        }

        bool notOvercaped = (PlayerResources.GetCurrentGP() + cordialCost[itemID]) < PlayerResources.GetMaxGP();

        return (itemID != 0) && notOvercaped && PlayerResources.IsPotOffCooldown();
    }
}

public class AutoCast
{
    public uint Id { get; set; } = 0;
    public ActionType ActionType { get; set; } = ActionType.None;

    public AutoCast(uint id, ActionType actionType)
    {
        this.Id = id;
        this.ActionType = actionType;
    }
}