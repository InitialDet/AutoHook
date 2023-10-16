using AutoHook.Resources.Localization;

namespace AutoHook.Classes.AutoCasts;

public class AutoCastLine : BaseActionCast
{
    
    public AutoCastLine() : base(UIStrings.AutoCastLine_Auto_Cast_Line, Data.IDs.Actions.Cast)
    {
        GpThreshold = 1;
    }
    
    public override bool CastCondition()
    {
        return true;
    }
    
    /*protected override DrawOptionsDelegate DrawOptions => () =>
    {
        
    };*/
}