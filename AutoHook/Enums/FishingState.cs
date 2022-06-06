namespace AutoHook.Enums;

public enum FishingState : byte
{
    None       = 0,
    PoleOut    = 1,
    PullPoleIn = 2,
    Quit       = 3,
    PoleReady  = 4,
    Bite       = 5,
    Reeling    = 6,
    Waiting    = 8,
    Waiting2   = 9,
}
