using AutoHook.Data;
using AutoHook.Resources.Localization;
using AutoHook.Utils;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoHook.Classes.AutoCasts;

public class AutoPrizeCatch : BaseActionCast
{
    public bool UseWhenMoochIIOnCD = false;

    public bool UseOnlyWithIdenticalCast = false;

    public bool UseOnlyWithActiveSlap = false;

    public AutoPrizeCatch() : base(UIStrings.Prize_Catch, Data.IDs.Actions.PrizeCatch, ActionType.Action)
    {
        DoesCancelMooch = true;
        HelpText = UIStrings.Use_Prize_Catch_HelpText;
    }

    public override string GetName()
        => Name = UIStrings.Prize_Catch;

    public override bool CastCondition()
    {
        if (!Enabled)
            return false;

        if (UseWhenMoochIIOnCD && !PlayerResources.ActionOnCoolDown(IDs.Actions.Mooch2))
            return false;

        if (UseOnlyWithIdenticalCast && !PlayerResources.HasStatus(IDs.Status.IdenticalCast))
            return false;

        if (UseOnlyWithActiveSlap && !PlayerResources.HasStatus(IDs.Status.SurfaceSlap))
            return false;

        if (PlayerResources.HasStatus(IDs.Status.MakeshiftBait))
            return false;

        if (PlayerResources.HasStatus(IDs.Status.PrizeCatch))
            return false;

        if (PlayerResources.HasStatus(IDs.Status.AnglersFortune))
            return false;

        return PlayerResources.ActionTypeAvailable(IDs.Actions.PrizeCatch);
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

        if (DrawUtil.Checkbox(UIStrings.OnlyUseWhenActiveSurfaceSlap, ref UseOnlyWithActiveSlap))
        {
            Service.Save();
        }
    };
}