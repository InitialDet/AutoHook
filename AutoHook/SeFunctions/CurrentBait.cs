using AutoHook.SeFunctions;
using Dalamud.Utility.Signatures;
using Dalamud.Game;

namespace SeFunctions;

public sealed class CurrentBait : SeAddressBase
{
    public CurrentBait(SigScanner sigScanner)
        : base(sigScanner, "3B 05 ?? ?? ?? ?? 75 ?? C6 43")
    {
        SignatureHelper.Initialise(this);
    }

    public unsafe uint Current
        => *(uint*)Address;
}
