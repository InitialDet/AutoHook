using System;
using AutoHook.Data;
using AutoHook.Enums;
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

        if (!HasPatience())
            return HookType.Normal;

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

        if (HasPatience() && !UseDHTHPacience)
            return hook;

        if (UseDoubleHook && GetCurrentGP() > 400)
            hook = HookType.Double;
        else if (UseTripleHook && GetCurrentGP() > 700)
            hook = HookType.Triple;

        return hook;
    }

    private bool HasPatience()
    {
        if (Service.ClientState.LocalPlayer?.StatusList == null)
            return false;

        foreach (var buff in Service.ClientState.LocalPlayer.StatusList)
        {
            if (buff.StatusId == IDs.idPatienceBuff)
                return true;
        }

        return false;
    }

    private uint GetCurrentGP()
    {
        if (Service.ClientState.LocalPlayer?.CurrentGp == null)
            return 0;

        return Service.ClientState.LocalPlayer.CurrentGp;
    }

    public bool GetUseAutoMooch()
    {
        if (BaitName == "DefaultCast" || BaitName == "DefaultMooch")
            return Service.Configuration.UseAutoMooch;
        else
            return UseAutoMooch;
    }

    public bool GetUseAutoMooch2()
    {
        if (BaitName == "DefaultCast" || BaitName == "DefaultMooch")
            return Service.Configuration.UseAutoMooch2;
        else
            return UseAutoMooch2;
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
