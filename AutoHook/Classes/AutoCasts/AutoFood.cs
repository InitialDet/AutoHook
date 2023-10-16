using AutoHook.Resources.Localization;
using AutoHook.Utils;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoHook.Classes.AutoCasts;

public class AutoFood : BaseActionCast
{
    private float _secondsRemaining = 0;
    
    public AutoFood() : base(UIStrings.Food_Buff, 0, ActionType.Item)
    {
        
    }
    
    public override bool CastCondition()
    {

        if (PlayerResources.CheckFoodBuff() > _secondsRemaining)
        {
            return false;
        }
        
        return true;
    }
    
    /*protected override DrawOptionsDelegate DrawOptions => () =>
    {
        
    };*/
}