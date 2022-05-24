using System;
using Dalamud.Game;

namespace AutoHook.SeFunctions;

public delegate void UpdateCatchDelegate(IntPtr module, uint fishId, bool large, ushort size, byte amount, byte level, byte unk7, byte unk8, byte unk9, byte unk10,
    byte unk11, byte unk12);

public sealed class UpdateFishCatch : SeFunctionBase<UpdateCatchDelegate>
{
    public UpdateFishCatch(SigScanner sigScanner)
        : base(sigScanner, "?? 89 ?? ?? ?? ?? 89 ?? ?? ?? ?? 89 ?? ?? ?? 41 ?? 48 83 ?? ?? 41 0F ?? ?? 41 0F ?? ?? 8B DA 4C 8B ?? E8")
    { }
}