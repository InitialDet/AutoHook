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
    public HookType WeakTugHook { get; set; } = HookType.Precision;

    public bool HookStrongkEnabled = true;
    public HookType StrongTugHook { get; set; } = HookType.Powerful;

    public bool HookLendarykEnabled = true;
    public HookType LegendaryTugHook { get; set; } = HookType.Powerful;

    // todo: add a checkbox to enable/disable autocast
    public bool UseAutoMooch = true;
    public bool UseAutoMooch2 = false;

    public bool UseDoubleHook = false;
    public bool UseTripleHook = false;
    public bool UseDHTHPacience = false;

    public double MaxTimeDelay = 0;
    public double MinTimeDelay = 0;

    public HookConfig(string bait)
    {
        BaitName = bait;
    }

    public HookType GetHook(BiteType bite)
    {
        if (!CheckHookEnabled(bite))
            return HookType.None;

        var hook = GetDoubleTripleHook(bite);

        if (hook != HookType.None)
            return hook;

        return GetPatienceHook(bite);
    }

    public bool CheckHookEnabled(BiteType bite) =>
        bite == BiteType.Weak ? HookWeakEnabled :
        bite == BiteType.Strong ? HookStrongkEnabled :
        bite == BiteType.Legendary ? HookLendarykEnabled :
        false;

    private HookType GetPatienceHook(BiteType bite) => bite switch
    {
        BiteType.Weak => WeakTugHook,
        BiteType.Strong => StrongTugHook,
        BiteType.Legendary => LegendaryTugHook,
        _ => HookType.None,
    };

    private HookType GetDoubleTripleHook(BiteType bite)
    {
        HookType hook = HookType.None;

        if (PlayerResources.HasStatus(IDs.Status.AnglersFortune) && !UseDHTHPacience)
            return hook;

        if (UseDoubleHook && PlayerResources.GetCurrentGP() > 400)
            hook = HookType.Double;
        else if (UseTripleHook && PlayerResources.GetCurrentGP() > 700)
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
