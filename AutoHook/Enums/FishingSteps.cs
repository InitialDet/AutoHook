using System;

namespace AutoHook.Enums;

[Flags]
public enum FishingSteps
{
    None,
    BeganFishing,
    BeganMooching,
    FishBit,
    Hooking,
    FishCaught,
    BaitSwapped,
    PresetSwapped,
    FishReeled,
    TimeOut,
    Quitting
}