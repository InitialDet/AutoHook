using AutoHook.Resources.Localization;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoHook.Classes.AutoCasts;

public class AutoMooch2 : BaseActionCast
{
    public AutoMooch2() : base(UIStrings.UseMoochII, Data.IDs.Actions.Mooch2, ActionType.Action)
    {
    }

    public override string GetName()
        => Name = UIStrings.UseMoochII;

    public override bool CastCondition()
    {
        return true;
    }

    /*protected override DrawOptionsDelegate DrawOptions => () =>
    {

    };*/
}