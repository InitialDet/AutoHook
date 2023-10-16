using System;
using Dalamud.Game;

namespace AutoHook.SeFunctions
{
    public class SeAddressBase
    {
        public readonly IntPtr Address;

        public SeAddressBase(ISigScanner sigScanner, string signature, int offset = 0)
        {
            Address = sigScanner.GetStaticAddressFromSig(signature);
            if (Address != IntPtr.Zero)
                Address += offset;
            var baseOffset = (ulong)Address.ToInt64() - (ulong)sigScanner.Module.BaseAddress.ToInt64();
        }
    }
}
