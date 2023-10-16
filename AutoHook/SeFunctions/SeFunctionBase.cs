using System;
using System.Runtime.InteropServices;
using Dalamud.Game;
using Dalamud.Hooking;

namespace AutoHook.SeFunctions
{
    public class SeFunctionBase<T> where T : Delegate
    {
        public IntPtr Address;
        protected T? FuncDelegate;

        public SeFunctionBase(SigScanner sigScanner, int offset)
        {
            Address = sigScanner.Module.BaseAddress + offset;
        }

        public SeFunctionBase(ISigScanner sigScanner, string signature, int offset = 0)
        {
            Address = sigScanner.ScanText(signature);
            if (Address != IntPtr.Zero)
                Address += offset;
            var baseOffset = (ulong)Address.ToInt64() - (ulong)sigScanner.Module.BaseAddress.ToInt64();
        }

        public T? Delegate()
        {
            if (FuncDelegate != null)
                return FuncDelegate;

            if (Address != IntPtr.Zero)
            {
                FuncDelegate = Marshal.GetDelegateForFunctionPointer<T>(Address);
                return FuncDelegate;
            }

            Service.PluginLog.Error($"Trying to generate delegate for {GetType().Name}, but no pointer available.");
            return null;
        }

        public dynamic? Invoke(params dynamic[] parameters)
        {
            if (FuncDelegate != null)
                return FuncDelegate.DynamicInvoke(parameters);

            if (Address != IntPtr.Zero)
            {
                FuncDelegate = Marshal.GetDelegateForFunctionPointer<T>(Address);
                return FuncDelegate!.DynamicInvoke(parameters);
            }
            else
            {
                Service.PrintDebug($"[SeFunctionBase] Trying to call {GetType().Name}, but no pointer available.");
                return null;
            }
        }

        public Hook<T>? CreateHook(T detour)
        {
            if (Address != IntPtr.Zero)
            {
                var hook = Service.GameInteropProvider.HookFromAddress<T>(Address, detour);
                hook.Enable();
                return hook;
            }

            Service.PrintDebug($"[SeFunctionBase] Trying to create Hook for {GetType().Name}, but no pointer available.");
            return null;
        }
    }
}
