using AutoHook.Resources.Localization;

namespace AutoHook.Classes.AutoCasts;

public class AutoReleaseFish : BaseActionCast
{
    public AutoReleaseFish() : base(UIStrings.ReleaseAllFish, Data.IDs.Actions.Release)
    {
        HelpText = UIStrings.ReleaseAllFishHelpText;
        DoesCancelMooch = false;
    }

    public override bool CastCondition()
    {
        return true;
    }

    public override string GetName()
        => Name = UIStrings.ReleaseAllFish;
}