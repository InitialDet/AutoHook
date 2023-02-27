using AutoHook.Classes;
using AutoHook.Data;
using AutoHook.Utils;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoHook.Configurations;

public class AutoCastsConfig
{
    public bool EnableAll = false;
    public bool EnableAutoCast = false;

    public bool EnableMooch = false;
    public bool EnableMooch2 = false;
    public bool OnlyMoochIntuition = false;

    public bool EnablePatience = false;
    public bool EnableMakeshiftPatience = false;

    public bool DontCancelMooch = true;

    public uint SelectedPatienceID = IDs.Actions.Patience2; // Default to Patience2

    public AutoPatienceI AutoPatienceI = new();

    public AutoPatienceII AutoPatienceII = new();

    public AutoChum AutoChum = new();

    public AutoFishEyes AutoFishEyes = new();

    public AutoHICordial AutoHICordial = new();

    public AutoHQCordial AutoHQCordial = new();

    public AutoCordial AutoCordial = new();

    public AutoHQWateredCordial AutoHQWateredCordial = new();

    public AutoWateredCordial AutoWateredCordial = new();

    public AutoThaliaksFavor AutoThaliaksFavor = new();

    public AutoMakeShiftBait AutoMakeShiftBait = new();

    public AutoIdenticalCast AutoIdenticalCast = new();

    public AutoSurfaceSlap AutoSurfaceSlap = new();

    public AutoPrizeCatch AutoPrizeCatch = new();

    public BaitConfig? BaitConfig = null;

    public bool EnableCordials = false;

    public bool EnableCordialFirst = false;


    // i could make the code more optimized but im too lazy rn.
    public AutoCast? GetNextAutoCast(BaitConfig? baitConfig)
    {
        if (!EnableAll)
            return null;

        BaitConfig = baitConfig;

        if (!PlayerResources.ActionAvailable(IDs.Actions.Cast))
            return null;

        if (AutoThaliaksFavor.IsAvailableToCast(baitConfig))
            return new(AutoThaliaksFavor.ID, AutoThaliaksFavor.ActionType);

        if (AutoMakeShiftBait.IsAvailableToCast(baitConfig))
            return new(AutoMakeShiftBait.ID, AutoMakeShiftBait.ActionType);

        if (AutoChum.IsAvailableToCast(baitConfig))
            return new(AutoChum.ID, AutoChum.ActionType);

        if (AutoFishEyes.IsAvailableToCast(baitConfig))
            return new(AutoFishEyes.ID, AutoFishEyes.ActionType);

        if (AutoIdenticalCast.IsAvailableToCast(baitConfig))
            return new(AutoIdenticalCast.ID, AutoIdenticalCast.ActionType);

        if (AutoSurfaceSlap.IsAvailableToCast(baitConfig))
            return new(AutoSurfaceSlap.ID, AutoSurfaceSlap.ActionType);

        if (AutoPrizeCatch.IsAvailableToCast(baitConfig))
            return new(AutoPrizeCatch.ID, AutoPrizeCatch.ActionType);

        if (SelectedPatienceID == IDs.Actions.Patience2)
        {
            if (AutoPatienceII.IsAvailableToCast(baitConfig))
                return new(AutoPatienceII.ID, AutoPatienceII.ActionType);
        }
        else
        {
            if (AutoPatienceI.IsAvailableToCast(baitConfig))
                return new(AutoPatienceI.ID, AutoPatienceI.ActionType);
        }

        var cordial = GetCordials();

        if (cordial != null && cordial.IsAvailableToCast(baitConfig))
            return new(cordial.ID, cordial.ActionType);

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
        bool onlyMoochIntuition = false;

        // 
        if (BaitConfig == null || BaitConfig?.BaitName == "DefaultCast" || BaitConfig?.BaitName == "DefaultMooch")
        {
            useAutoMooch = EnableMooch;
            useAutoMooch2 = EnableMooch2;
            onlyMoochIntuition = OnlyMoochIntuition;

        }
        else
        {
            useAutoMooch = BaitConfig?.UseAutoMooch ?? false;
            useAutoMooch2 = BaitConfig?.UseAutoMooch2 ?? false;
            onlyMoochIntuition = BaitConfig?.OnlyMoochIntuition ?? false;
        }

        if (useAutoMooch)
        {

            if (onlyMoochIntuition && !PlayerResources.HasStatus(IDs.Status.IdenticalCast))
                return false;

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

    private BaseActionCast? GetCordials()
    {
        bool useWateredCordial = PlayerResources.HaveItemInInventory(IDs.Item.WateredCordial);
        bool useHQWateredCordial = PlayerResources.HaveItemInInventory(IDs.Item.WateredCordial, true);
        bool useCordial = PlayerResources.HaveItemInInventory(IDs.Item.Cordial);
        bool useHQCordial = PlayerResources.HaveItemInInventory(IDs.Item.Cordial, true);
        bool useHICordial = PlayerResources.HaveItemInInventory(IDs.Item.HiCordial);

        if (EnableCordialFirst)
        {
            if (useWateredCordial)
                return AutoWateredCordial;
            if (useHQWateredCordial)
                return AutoHQWateredCordial;
            if (useCordial)
                return AutoCordial;
            if (useHQCordial)
                return AutoHQCordial;
            if (useHICordial)
                return AutoHICordial;
        }
        else
        {
            if (useHICordial)
                return AutoHICordial;
            if (useHQCordial)
                return AutoHQCordial;
            if (useCordial)
                return AutoCordial;
            if (useHQWateredCordial)
                return AutoHQWateredCordial;
            if (useWateredCordial)
                return AutoWateredCordial;
        }

        return null;
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