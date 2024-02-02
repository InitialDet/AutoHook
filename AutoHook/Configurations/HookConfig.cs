using System;
using System.Xml;
using AutoHook.Classes;
using AutoHook.Data;
using AutoHook.Enums;
using AutoHook.Utils;

namespace AutoHook.Configurations;

public class HookConfig
{
    public bool Enabled = true;

    private Guid _uniqueId;
    
    public BaitFishClass BaitFish = new();

    public bool HookWeakEnabled = true;
    public bool HookWeakIntuitionEnabled = true;
    public bool HookWeakDHTHEnabled = true;
    public bool HookWeakOnlyWhenActiveSlap = false;
    public bool HookWeakOnlyWhenNOTActiveSlap = false;
    public HookType HookTypeWeak = HookType.Precision;
    public HookType HookTypeWeakIntuition = HookType.Precision;

    public bool HookStrongEnabled = true;
    public bool HookStrongIntuitionEnabled = true;
    public bool HookStrongDHTHEnabled = true;
    public bool HookStrongOnlyWhenActiveSlap = false;
    public bool HookStrongOnlyWhenNOTActiveSlap = false;
    public HookType HookTypeStrong = HookType.Powerful;
    public HookType HookTypeStrongIntuition = HookType.Powerful;

    public bool HookLegendaryEnabled = true;
    public bool HookLegendaryIntuitionEnabled = true;
    public bool HookLegendaryDHTHEnabled = true;
    public bool HookLegendaryOnlyWhenActiveSlap = false;
    public bool HookLegendaryOnlyWhenNOTActiveSlap = false;
    public HookType HookTypeLegendary = HookType.Powerful;
    public HookType HookTypeLegendaryIntuition = HookType.Powerful;

    public bool UseCustomIntuitionHook = false;

    /*public bool UseAutoMooch = true;
    public bool UseAutoMooch2 = false;
    public bool OnlyMoochIntuition = false;*/

    /*public bool UseSurfaceSlap = false;
    public bool UseIdenticalCast = false;*/
    
    public bool UseDoubleHook = false;
    public bool UseTripleHook = false;
    public bool UseDHTHPatience = false;
    public bool UseDHTHOnlyIdenticalCast = false;
    public bool UseDHTHOnlySurfaceSlap = false;
    public bool LetFishEscape = false;

    public double MaxTimeDelay = 0;
    public double MinTimeDelay = 0;

    public bool UseChumTimer = false;
    public double MaxChumTimeDelay = 0;
    public double MinChumTimeDelay = 0;

    public bool StopAfterCaught = false;
    public int StopAfterCaughtLimit = 1;
    
    public FishingSteps StopFishingStep = FishingSteps.None;

    /*public HookConfig(string bait)
    {
        BaitName = bait;
    }*/
    
    public HookConfig(BaitFishClass baitFish)
    {
        BaitFish = baitFish;
        _uniqueId = Guid.NewGuid();
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

        if (CheckHookSurfaceSlapEnabled(bite) && !PlayerResources.HasStatus(IDs.Status.SurfaceSlap))
            return HookType.None;

        if (CheckHookSurfaceSlapNOTEnabled(bite) && PlayerResources.HasStatus(IDs.Status.SurfaceSlap))
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
        bite == BiteType.Legendary ? HookLegendaryEnabled : false;

    public bool CheckHookIntuitionEnabled(BiteType bite) =>
        bite == BiteType.Weak ? HookWeakIntuitionEnabled :
        bite == BiteType.Strong ? HookStrongIntuitionEnabled :
        bite == BiteType.Legendary ? HookLegendaryIntuitionEnabled : false;

    public bool CheckHookSurfaceSlapEnabled(BiteType bite) =>
        bite == BiteType.Weak ? HookWeakOnlyWhenActiveSlap :
        bite == BiteType.Strong ? HookStrongOnlyWhenActiveSlap :
        bite == BiteType.Legendary ? HookLegendaryOnlyWhenActiveSlap : false;

    public bool CheckHookSurfaceSlapNOTEnabled(BiteType bite) =>
        bite == BiteType.Weak ? HookWeakOnlyWhenNOTActiveSlap :
        bite == BiteType.Strong ? HookStrongOnlyWhenNOTActiveSlap :
        bite == BiteType.Legendary ? HookLegendaryOnlyWhenNOTActiveSlap : false;

    public bool CheckHookDHTHEnabled(BiteType bite) =>
        bite == BiteType.Weak ? HookWeakDHTHEnabled :
        bite == BiteType.Strong ? HookStrongDHTHEnabled :
        bite == BiteType.Legendary ? HookLegendaryDHTHEnabled : false;
    
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
        if (UseTripleHook || UseDoubleHook)
        {
            if (UseDHTHOnlyIdenticalCast && !PlayerResources.HasStatus(IDs.Status.IdenticalCast))
                return HookType.None;

            if (UseDHTHOnlySurfaceSlap && !PlayerResources.HasStatus(IDs.Status.SurfaceSlap))
                return HookType.None;

            if (PlayerResources.HasStatus(IDs.Status.AnglersFortune) && !UseDHTHPatience)
                return HookType.None;

            if (UseTripleHook && PlayerResources.GetCurrentGp() >= 700 && CheckHookDHTHEnabled(bite))
                return HookType.Triple;

            if (UseDoubleHook && PlayerResources.GetCurrentGp() >= 400 && CheckHookDHTHEnabled(bite))
                return HookType.Double;

            if (LetFishEscape)
                return null;
        }

        return HookType.None;
    }
    
    
    public Guid GetUniqueId()
    {
        if (_uniqueId == Guid.Empty)
            _uniqueId = Guid.NewGuid();
        
        return _uniqueId;
    }
    
    public override bool Equals(object? obj)
    {
        return obj is HookConfig settings &&
               BaitFish == settings.BaitFish;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(BaitFish?.Name + @"a");
    }
}
