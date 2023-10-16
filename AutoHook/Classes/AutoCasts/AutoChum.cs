using AutoHook.Data;
using AutoHook.Resources.Localization;
using AutoHook.Utils;

namespace AutoHook.Classes.AutoCasts;

public class AutoChum : BaseActionCast
{
    private bool _onlyUseWithIntuition;
    
    public AutoChum() : base(UIStrings.Chum, IDs.Actions.Chum)
    {
        DoesCancelMooch = true;
        HelpText = UIStrings.CancelsCurrentMooch;
    }

    public override bool CastCondition()
    {
        if (!PlayerResources.HasStatus(IDs.Status.FishersIntuition) && _onlyUseWithIntuition)
            return false;

        return true; 
    }

    protected override DrawOptionsDelegate DrawOptions => () =>
    {
        if (DrawUtil.Checkbox(UIStrings.OnlyUseWhenFisherSIntutionIsActive,
                ref _onlyUseWithIntuition))
        {
            Service.Save();
        }
    };
}