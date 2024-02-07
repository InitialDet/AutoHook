using AutoHook.Data;
using AutoHook.Resources.Localization;
using AutoHook.Utils;
using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;
using System;

namespace AutoHook.Classes.AutoCasts;

public class AutoPatience : BaseActionCast
{
    public bool EnableMakeshiftPatience;
    public int RefreshEarlyTime = 0;

    public AutoPatience() : base(UIStrings.AutoPatience_Patience, Data.IDs.Actions.Patience2, ActionType.Action)
    {
        DoesCancelMooch = true;
        HelpText = UIStrings.CancelsCurrentMooch;
    }

    public override string GetName()
        => Name = UIStrings.AutoPatience_Patience;

    public override bool CastCondition()
    {
        if (PlayerResources.HasStatus(IDs.Status.AnglersFortune) && PlayerResources.GetStatusTime(IDs.Status.AnglersFortune) > RefreshEarlyTime)
            return false;

        if (PlayerResources.HasStatus(IDs.Status.PrizeCatch))
            return false;

        if (PlayerResources.HasStatus(IDs.Status.MakeshiftBait) && !EnableMakeshiftPatience)
            return false;

        return true;
    }

    protected override DrawOptionsDelegate DrawOptions => () =>
    {
        if (DrawUtil.Checkbox(UIStrings.TabAutoCasts_DrawExtraOptionsPatience, ref EnableMakeshiftPatience))
        {
            Service.Save();
        }

        if (ImGui.RadioButton(UIStrings.Patience_I, Id == IDs.Actions.Patience))
        {
            Id = IDs.Actions.Patience;
            Service.Save();
        }

        if (ImGui.RadioButton(UIStrings.Patience_II, Id == IDs.Actions.Patience2))
        {
            Id = IDs.Actions.Patience2;
            Service.Save();
        }

        var time = RefreshEarlyTime;
        if (DrawUtil.EditNumberField(UIStrings.RefreshWhenTimeIsLessThanOrEqual, ref time))
        {
            RefreshEarlyTime = Math.Max(0, Math.Min(time, 999));
            Service.Save();
        }
    };
}