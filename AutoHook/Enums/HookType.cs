using AutoHook.Data;

namespace AutoHook.Enums;

public enum HookType : uint
{
    None = 0,
    Normal = IDs.idNormalHook,
    Precision = IDs.idPrecision,
    Powerful = IDs.idPowerful,
    Double = IDs.idDoubleHook,
    Triple = IDs.idTripleHook,
}
