using System;
using AutoHook.Data;
using AutoHook.Enums;
using AutoHook.Utils;

namespace AutoHook.Configurations;

public class HookConfig
{
    public bool Enabled = true;

    public string BaitName = "Default";

    public bool HookWeakEnabled = true;
    public bool HookWeakIntuitionEnabled = true;
    public HookType HookTypeWeak = HookType.Precision;
    public HookType HookTypeWeakIntuition = HookType.Precision;

    public bool HookStrongEnabled = true;
    public bool HookStrongIntuitionEnabled = true;
    public HookType HookTypeStrong = HookType.Powerful;
    public HookType HookTypeStrongIntuition = HookType.Powerful;

    public bool HookLegendaryEnabled = true;
    public bool HookLegendaryIntuitionEnabled = true;
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

    public double MaxTimeDelay = 0;
    public double MinTimeDelay = 0;

    public bool StopAfterCaught = false;
    public int StopAfterCaughtLimit = 1;

    public HookConfig(string bait)
    {
        BaitName = bait;
    }

    public HookType GetHook(BiteType bite)
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

    private HookType GetDoubleTripleHook(BiteType bite)
    {
        HookType hook = HookType.None;

        if (PlayerResources.HasStatus(IDs.Status.AnglersFortune) && !UseDHTHPatience)
            return hook;

        if (UseDoubleHook && PlayerResources.GetCurrentGP() >= 400)
            hook = HookType.Double;
        else if (UseTripleHook && PlayerResources.GetCurrentGP() >= 700)
            hook = HookType.Triple;

        return hook;
    }

    public override bool Equals(object? obj)
    {
        return obj is HookConfig settings &&
               BaitName == settings.BaitName;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(BaitName + "a");
    }
}
