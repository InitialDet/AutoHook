using AutoHook.Data;

namespace AutoHook.Enums;

public enum HookType : uint
{
    None = 0,
    Normal = IDs.Actions.Hook,
    Precision = IDs.Actions.PrecisionHS,
    Powerful = IDs.Actions.PowerfulHS,
    Double = IDs.Actions.DoubleHook,
    Triple = IDs.Actions.TripleHook,
}
