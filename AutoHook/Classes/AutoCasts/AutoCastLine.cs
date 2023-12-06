using AutoHook.Resources.Localization;

namespace AutoHook.Classes.AutoCasts;

public class AutoCastLine : BaseActionCast
{
    public AutoCastLine() : base(UIStrings.AutoCastLine_Auto_Cast_Line, Data.IDs.Actions.Cast)
    {
        
    }

    public override bool CastCondition()
    {
        return true;
    }

    public override string GetName()
        => Name = UIStrings.AutoCastLine_Auto_Cast_Line;
}