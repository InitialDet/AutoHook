using AutoHook.Resources.Localization;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoHook.Classes.AutoCasts;

public class AutoTripleHook : BaseActionCast
{
    public AutoTripleHook() : base(UIStrings.Triple_Hook, Data.IDs.Actions.TripleHook, ActionType.Action)
    {
    }
    
    public override string GetName()
        => Name = UIStrings.Triple_Hook;

    public override bool CastCondition()
    {
        return true;
    }

    /*protected override DrawOptionsDelegate DrawOptions => () =>
    {

    };*/
}