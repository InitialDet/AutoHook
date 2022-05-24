using AutoHook.SeFunctions;
using Dalamud.Game;

namespace SeFunctions;

public sealed class CurrentBait : SeAddressBase
{
    public CurrentBait(SigScanner sigScanner)
        : base(sigScanner, "48 83 C4 30 5B C3 49 8B C8 E8 ?? ?? ?? ?? 3B 05")
    { }

    public unsafe uint Current
        => *(uint*)Address;
}
