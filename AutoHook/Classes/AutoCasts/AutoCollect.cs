using AutoHook.Data;
using AutoHook.Resources.Localization;
using AutoHook.Utils;
using FFXIVClientStructs.FFXIV.Client.Game;
namespace AutoHook.Classes.AutoCasts;

public class AutoCollect : BaseActionCast
{
    public AutoCollect() : base(UIStrings.Collect, IDs.Actions.Collect, ActionType.Ability)
    {

    }

    public override string GetName()
        => Name = UIStrings.Collect;

    public override bool CastCondition() => !PlayerResources.HasStatus(IDs.Status.CollectorsGlove);

    //protected override DrawOptionsDelegate DrawOptions => () =>
    //{

    //};
}