using System;
using AutoHook.Data;
using AutoHook.Enums;
using AutoHook.Utils;
using Dalamud.Logging;

namespace AutoHook.Configurations;

public class BaitConfig
{
    public bool Enabled = true;

    public string BaitName = "Default";

    public bool HookWeakEnabled = true;
    public bool HookWeakIntuitionEnabled = true;
    public bool HookWeakDHTHEnabled = true;
    public HookType HookTypeWeak = HookType.Precision;
    public HookType HookTypeWeakIntuition = HookType.Precision;

    public bool HookStrongEnabled = true;
    public bool HookStrongIntuitionEnabled = true;
    public bool HookStrongDHTHEnabled = true;
    public HookType HookTypeStrong = HookType.Powerful;
    public HookType HookTypeStrongIntuition = HookType.Powerful;

    public bool HookLegendaryEnabled = true;
    public bool HookLegendaryIntuitionEnabled = true;
    public bool HookLegendaryDHTHEnabled = true;
    public HookType HookTypeLegendary = HookType.Powerful;
    public HookType HookTypeLegendaryIntuition = HookType.Powerful;

    public bool UseCustomIntuitionHook = false;

    public bool UseAutoMooch = true;
    public bool UseAutoMooch2 = false;
    
    public bool UseSurfaceSlap = false;
    public bool UseIdenticalCast = false;

    public bool UseDoubleHook = false;
    public bool UseTripleHook = false;
    public bool UseDHTHPatience = false;
    public bool UseDHTHOnlySurfaceSlap = false;
    public bool LetFishEscape = false;

    public double MaxTimeDelay = 0;
    public double MinTimeDelay = 0;

    public bool StopAfterCaught = false;
    public int StopAfterCaughtLimit = 1;

    public BaitConfig(string bait)
    {
        BaitName = bait;
    }

    public HookType? GetHook(BiteType bite)
    {
        bool hasIntuition = PlayerResources.HasStatus(IDs.Status.FishersIntuition);

        if (hasIntuition && UseCustomIntuitionHook)
        {
            if (!CheckHookIntuitionEnabled(bite))
                return HookType.None;
        }
        else if (!CheckHookEnabled(bite))
            return HookType.None;

        var hook = GetDoubleTripleHook(bite);

        if (hook != HookType.None)
            return hook;

        if (hasIntuition)
            return GetIntuitionHook(bite);
        else
            return GetPatienceHook(bite);
    }

    public HookType? GetHookIgnoreEnable(BiteType bite)
    {
        bool hasIntuition = PlayerResources.HasStatus(IDs.Status.FishersIntuition);

        var hook = GetDoubleTripleHook(bite);

        if (hook == null || hook != HookType.None)
            return hook;

        if (hasIntuition)
            return GetIntuitionHook(bite);
        else
            return GetPatienceHook(bite);
    }

    public bool CheckHookEnabled(BiteType bite) =>
        bite == BiteType.Weak ? HookWeakEnabled :
        bite == BiteType.Strong ? HookStrongEnabled :
        bite == BiteType.Legendary ? HookLegendaryEnabled :
        false;

    public bool CheckHookIntuitionEnabled(BiteType bite) =>
        bite == BiteType.Weak ? HookWeakIntuitionEnabled :
        bite == BiteType.Strong ? HookStrongIntuitionEnabled :
        bite == BiteType.Legendary ? HookLegendaryIntuitionEnabled :
        false;

    public bool CheckHookDHTHEnabled(BiteType bite) =>
        bite == BiteType.Weak ? HookWeakDHTHEnabled :
        bite == BiteType.Strong ? HookStrongDHTHEnabled :
        bite == BiteType.Legendary ? HookLegendaryDHTHEnabled :
        false;


    private HookType GetPatienceHook(BiteType bite) => bite switch
    {
        BiteType.Weak => HookTypeWeak,
        BiteType.Strong => HookTypeStrong,
        BiteType.Legendary => HookTypeLegendary,
        _ => HookType.None,
    };

    private HookType GetIntuitionHook(BiteType bite) => bite switch
    {
        BiteType.Weak => HookTypeWeakIntuition,
        BiteType.Strong => HookTypeStrongIntuition,
        BiteType.Legendary => HookTypeLegendaryIntuition,
        _ => HookType.None,
    };

    private HookType? GetDoubleTripleHook(BiteType bite)
    {
        HookType hook = HookType.None;

        if (PlayerResources.HasStatus(IDs.Status.AnglersFortune) && !UseDHTHPatience)
            return hook;

        if (UseDHTHOnlySurfaceSlap && !PlayerResources.HasStatus(IDs.Status.IdenticalCast))
            return hook;

  
        if (UseDoubleHook)
        {
            if (PlayerResources.GetCurrentGP() >= 400 && CheckHookDHTHEnabled(bite))
                hook = HookType.Double;
            if (LetFishEscape)
                return null;
        }
        else if (UseTripleHook)
        {
            if (PlayerResources.GetCurrentGP() >= 700 && CheckHookDHTHEnabled(bite))
                hook = HookType.Triple;
            else if (LetFishEscape)
                return null;
        }
          
        return hook;
    }

    public override bool Equals(object? obj)
    {
        return obj is BaitConfig settings &&
               BaitName == settings.BaitName;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(BaitName + "a");
    }
}
