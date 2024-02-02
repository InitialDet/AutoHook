using AutoHook.Data;
using AutoHook.Resources.Localization;
using AutoHook.Utils;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoHook.Classes.AutoCasts;

public class AutoDoubleHook : BaseActionCast
{
    public bool UseOnlyWithActiveSlap = false;

    public AutoDoubleHook() : base(UIStrings.Double_Hook, Data.IDs.Actions.DoubleHook, ActionType.Action)
    {
    }

    public override string GetName()
        => Name = UIStrings.Double_Hook;

    public override bool CastCondition()
    {
        if (UseOnlyWithActiveSlap && !PlayerResources.HasStatus(IDs.Status.SurfaceSlap))
            return false;

        return true;
    }

    protected override DrawOptionsDelegate DrawOptions => () =>
    {
        if (DrawUtil.Checkbox(UIStrings.OnlyUseWhenActiveSurfaceSlap, ref UseOnlyWithActiveSlap))
        {
            Service.Save();
        }
    };
}