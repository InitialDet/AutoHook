using System;

namespace AutoHook.Enums;

[Flags]
public enum FishingSteps
{
    None = 0,
    BeganFishing = 1,
    BeganMooching = 2,
    FishBit = 4,
    Hooking = 6,
    FishCaught = 8,
    BaitSwapped = 10,
    PresetSwapped = 14,
    FishReeled = 12,
    TimeOut = 16,
    Quitting = 28
}