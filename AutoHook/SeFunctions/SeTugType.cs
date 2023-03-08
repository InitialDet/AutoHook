using System;
using AutoHook.Enums;
using Dalamud.Game;

namespace AutoHook.SeFunctions;

public sealed class SeTugType : SeAddressBase
{
    public SeTugType(SigScanner sigScanner)
        : base(sigScanner,
            "4C 8D 0D ?? ?? ?? ?? 4D 8B 13 49 8B CB 45 0F B7 43 ?? 49 8B 93 ?? ?? ?? ?? 88 44 24 20 41 FF 92 ?? ?? ?? ?? 48 83 C4 38 C3")
    { }

    public unsafe BiteType Bite
        => Address != IntPtr.Zero ? *(BiteType*)Address : BiteType.Unknown;
}

