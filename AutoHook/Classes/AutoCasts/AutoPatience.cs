using AutoHook.Data;
using AutoHook.Resources.Localization;
using AutoHook.Utils;
using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;

namespace AutoHook.Classes.AutoCasts;

public class AutoPatience : BaseActionCast
{
    public bool EnableMakeshiftPatience;
    
    public AutoPatience() : base(UIStrings.AutoPatience_Patience, Data.IDs.Actions.Patience2, ActionType.Action)
    {
        DoesCancelMooch = true;
        HelpText = UIStrings.CancelsCurrentMooch;
    }

    public override bool CastCondition()
    {
        
        if (PlayerResources.HasStatus(IDs.Status.AnglersFortune))
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
    };
}