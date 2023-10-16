using AutoHook.Data;
using AutoHook.Resources.Localization;
using AutoHook.Utils;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoHook.Classes.AutoCasts;

public class AutoIdenticalCast : BaseActionCast
{
    public bool OnlyUseUnderPatience = false;

    public AutoIdenticalCast() : base(UIStrings.Identical_Cast, IDs.Actions.IdenticalCast, ActionType.Action)
    {
        DoesCancelMooch = true;
        HelpText = UIStrings.OverridesSurfaceSlap;
    }

    public override string GetName()
        => Name = UIStrings.UseIdenticalCast;

    public override bool CastCondition()
    {
        if (PlayerResources.HasStatus(IDs.Status.IdenticalCast) || PlayerResources.HasStatus(IDs.Status.SurfaceSlap))
            return false;

        if (OnlyUseUnderPatience && !PlayerResources.HasStatus(IDs.Status.AnglersFortune))
            return false;

        return true;
    }

    protected override DrawOptionsDelegate DrawOptions => () =>
    {
        if (DrawUtil.Checkbox(UIStrings.Only_When_Patience_Active, ref OnlyUseUnderPatience))
        {
            Service.Save();
        }

        if (DrawUtil.Checkbox(UIStrings.Dont_Cancel_Mooch, ref DontCancelMooch,
                UIStrings.IdenticalCast_HelpText, true))
        {
            Service.Save();
        }
    };
}