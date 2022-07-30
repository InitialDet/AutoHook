using System.Collections.Generic;
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

    public bool DontCancelMooch = true;

    public bool EnablePatience = false;
    public bool EnableMakeshiftPatience = false;
    public uint SelectedPatienceID = IDs.Actions.Patience2; // Default to Patience2

    public bool EnableThaliaksFavor = false;
    public int ThaliaksFavorStacks = 3;

    public bool EnableMakeshiftBait = false;
    public int MakeshiftBaitStacks = 5;

    public bool EnablePrizeCatch = false;

    public bool EnableChum = false;
    public bool EnableFishEyes = false;

    public bool EnableIdenticalCast = false;
    public bool EnableSurfaceSlap = false;

    public bool EnableCordials = false;
    public bool EnableCordialFirst = false;


    HookConfig? hookConfig = null;

    public AutoCast? GetNextAutoCast(HookConfig? hookConfig)
    {

        if (!EnableAll)
            return null;

        this.hookConfig = hookConfig;

        if (!PlayerResources.ActionAvailable(IDs.Actions.Cast))
            return null;

        if (UseThaliaksFavor())
            return new(IDs.Actions.ThaliaksFavor, ActionType.Spell);

        if (UseMakeshiftBait())
            return new(IDs.Actions.MakeshiftBait, ActionType.Spell);

        if (UsesChum())
            return new(IDs.Actions.Chum, ActionType.Spell);

        if (UsesFishEyes())
            return new(IDs.Actions.FishEyes, ActionType.Spell);

        if (UsesIdenticalCast())
            return new(IDs.Actions.IdenticalCast, ActionType.Spell);

        if (UsesSurfaceSlap())
            return new(IDs.Actions.SurfaceSlap, ActionType.Spell);

        if (UsePrizeCatch())
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

        if (hookConfig == null || hookConfig?.BaitName == "DefaultCast" || hookConfig?.BaitName == "DefaultMooch")
        {
            useAutoMooch = EnableMooch;
            useAutoMooch2 = EnableMooch2;
        }
        else
        {
            useAutoMooch = hookConfig?.UseAutoMooch ?? false;
            useAutoMooch2 = hookConfig?.UseAutoMooch2 ?? false;
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

    private bool UsePatience()
    {
        if (EnablePatience)
        {
            if (!PlayerResources.HasStatus(IDs.Status.AnglersFortune))
            {
                // Dont use Patience if mooch is available
                if (IsMoochAvailable())
                {
                    return false;
                }

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

    private uint ThaliaksFavorRecover = 150; // This might change in the future.

    private bool UseThaliaksFavor()
    {
        if (!EnableThaliaksFavor)
            return false;
        bool available = PlayerResources.ActionAvailable(IDs.Actions.ThaliaksFavor);
        bool hasStacks = PlayerResources.HasAnglersArtStacks(ThaliaksFavorStacks);
        bool notOvercaped = (PlayerResources.GetCurrentGP() + ThaliaksFavorRecover) < PlayerResources.GetMaxGP();

        return available && hasStacks && notOvercaped; // dont use if its going to overcap gp
    }

    private bool UseMakeshiftBait()
    {
        if (!EnableMakeshiftBait)
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

    private bool IsMoochAvailable()
    {
        return PlayerResources.ActionAvailable(IDs.Actions.Mooch) || PlayerResources.ActionAvailable(IDs.Actions.Mooch2);
    }

    private bool UsePrizeCatch()
    {
        if (!EnablePrizeCatch)
            return false;

        if (IsMoochAvailable() && DontCancelMooch)
            return false;

        if (PlayerResources.HasStatus(IDs.Status.MakeshiftBait))
            return false;

        if (PlayerResources.HasStatus(IDs.Status.PrizeCatch))
            return false;

        if (PlayerResources.HasStatus(IDs.Status.AnglersFortune))
            return false;

        return PlayerResources.ActionAvailable(IDs.Actions.PrizeCatch);
    }

    private bool UsesFishEyes()
    {
        if (IsMoochAvailable() && DontCancelMooch)
            return false;

        return EnableFishEyes && PlayerResources.ActionAvailable(IDs.Actions.FishEyes);
    }

    private bool UsesChum()
    {
        if (IsMoochAvailable() && DontCancelMooch)
            return false;

        return EnableChum && PlayerResources.ActionAvailable(IDs.Actions.Chum);
    }

    private bool UsesIdenticalCast()
    {
        bool useIdenticalCast = false;

        if (hookConfig == null || hookConfig?.BaitName == "DefaultCast" || hookConfig?.BaitName == "DefaultMooch")
        {
            useIdenticalCast = EnableSurfaceSlap;
        }
        else
        {
            useIdenticalCast = hookConfig?.UseIdenticalCast ?? false;
        }

        return useIdenticalCast && PlayerResources.ActionAvailable(IDs.Actions.IdenticalCast);
    }

    private bool UsesSurfaceSlap()
    {
        bool useSurfaceSlap = false;

        if (hookConfig == null || hookConfig?.BaitName == "DefaultCast" || hookConfig?.BaitName == "DefaultMooch")
        {
            useSurfaceSlap = EnableSurfaceSlap;
        }
        else
        {
            useSurfaceSlap = hookConfig?.UseSurfaceSlap ?? false;
        }

        return useSurfaceSlap && PlayerResources.ActionAvailable(IDs.Actions.SurfaceSlap);
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

        if (EnableCordialFirst) {
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