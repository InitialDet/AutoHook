using System;
using System.Runtime.InteropServices;
using System.Text;
using AutoHook.SeFunctions;
using Dalamud.Game;
using Dalamud.Logging;

namespace AutoHook
{
    // I got this from the GatherBuddy repo
    public class CommandManager
    {
        private readonly ProcessChatBox _processChatBox;

        private readonly IntPtr _uiModulePtr;

        public CommandManager(SeAddressBase baseUiObject, GetUiModule getUiModule, ProcessChatBox processChatBox)
        {
            _processChatBox = processChatBox;
            _uiModulePtr = getUiModule.Invoke(Marshal.ReadIntPtr(baseUiObject.Address));
        }

        public CommandManager(SigScanner sigScanner)
            : this(new BaseUiObject(sigScanner), new GetUiModule(sigScanner),
                new ProcessChatBox(sigScanner))
        { }

        public bool Execute(string message)
        {
            var (text, length) = PrepareString(message);
            var payload = PrepareContainer(text, length);

            _processChatBox.Invoke(_uiModulePtr, payload, IntPtr.Zero, (byte)0);

            Marshal.FreeHGlobal(payload);
            Marshal.FreeHGlobal(text);
            return false;
        }

        private static (IntPtr, long) PrepareString(string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            var mem = Marshal.AllocHGlobal(bytes.Length + 30);
            Marshal.Copy(bytes, 0, mem, bytes.Length);
            Marshal.WriteByte(mem + bytes.Length, 0);
            return (mem, bytes.Length + 1);
        }

        private static IntPtr PrepareContainer(IntPtr message, long length)
        {
            var mem = Marshal.AllocHGlobal(400);
            Marshal.WriteInt64(mem, message.ToInt64());
            Marshal.WriteInt64(mem + 0x8, 64);
            Marshal.WriteInt64(mem + 0x10, length);
            Marshal.WriteInt64(mem + 0x18, 0);
            return mem;
        }
    }
}
