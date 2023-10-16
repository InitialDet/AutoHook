using AutoHook.Data;
using AutoHook.Resources.Localization;
using AutoHook.Utils;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoHook.Classes.AutoCasts;

public class AutoPrizeCatch : BaseActionCast
{

    public bool UseWhenMoochIIOnCD = false;

    public bool UseOnlyWithIdenticalCast = false;
    
    public AutoPrizeCatch() : base(UIStrings.Prize_Catch, Data.IDs.Actions.PrizeCatch, ActionType.Action)
    {
        DoesCancelMooch = true;
        HelpText = UIStrings.Use_Prize_Catch_HelpText;
    }

    public override bool CastCondition()
    {
        if (!Enabled)
            return false;

        if (UseWhenMoochIIOnCD && PlayerResources.ActionAvailable(IDs.Actions.Mooch2))
            return false;

        if (UseOnlyWithIdenticalCast && !PlayerResources.HasStatus(IDs.Status.IdenticalCast))
            return false;

        if (PlayerResources.HasStatus(IDs.Status.MakeshiftBait))
            return false;

        if (PlayerResources.HasStatus(IDs.Status.PrizeCatch))
            return false;

        if (PlayerResources.HasStatus(IDs.Status.AnglersFortune))
            return false;

        return PlayerResources.ActionAvailable(IDs.Actions.PrizeCatch);
    }
    
    protected override DrawOptionsDelegate DrawOptions => () =>
    {
        if (DrawUtil.Checkbox(UIStrings.AutoCastExtraOptionPrizeCatch,
                ref UseWhenMoochIIOnCD, UIStrings.ExtraOptionPrizeCatchHelpMarker))
        {
            Service.Save();
        }

        if (DrawUtil.Checkbox(UIStrings.OnlyUseWhenIdenticalCastIsActive,
                ref UseOnlyWithIdenticalCast))
        {
            Service.Save();
        }
    };
}