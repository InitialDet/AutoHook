using AutoHook.Data;
using AutoHook.Resources.Localization;
using AutoHook.Utils;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoHook.Classes.AutoCasts;

public class AutoFood : BaseActionCast //todo
{
    public float SecondsRemaining = 0;

    public AutoFood() : base(UIStrings.Food_Buff, 0, ActionType.Item)
    {
    }

    public override string GetName()
        => Name = UIStrings.Food_Buff;

    public override bool CastCondition()
    {
        if (PlayerResources.GetStatusTime(IDs.Status.FoodBuff) > SecondsRemaining)
        {
            return false;
        }

        return true;
    }

    /*protected override DrawOptionsDelegate DrawOptions => () =>
    {

    };*/
}