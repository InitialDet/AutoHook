using Dalamud.Data;
using Dalamud.Game.Network;
using Dalamud.Interface.Colors;
using Dalamud.IoC;
using Dalamud.Game;
using Dalamud.Logging;
using Dalamud.Plugin;
using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Dalamud.Game.Gui;
using System.Threading;

namespace AutoHook
{
    // Based on the FishNotify plugin
    public sealed class AutoHook : IDalamudPlugin
    {
        public string Name => "AutoHook";

        [PluginService]
        [RequiredVersion("1.0")]
        private DalamudPluginInterface PluginInterface { get; set; }
        [PluginService] [RequiredVersion("1.0")] public static SigScanner SigScanner { get; private set; } = null!;
        [PluginService] [RequiredVersion("1.0")] public static ChatGui Chat { get; private set; } = null!;

        public CommandManager _commandManager;

        [PluginService]
        private GameNetwork Network { get; set; }
        private bool settingsVisible;
        private int expectedOpCode = -1;

        public AutoHook()
        {
            _commandManager = new CommandManager(SigScanner);
            Network!.NetworkMessage += OnNetworkMessage;
            PluginInterface!.UiBuilder.Draw += OnDrawUI;
            PluginInterface!.UiBuilder.OpenConfigUi += OnOpenConfigUi;

            var client = new HttpClient();
            client.GetStringAsync("https://raw.githubusercontent.com/karashiiro/FFXIVOpcodes/master/opcodes.min.json")
                .ContinueWith(ExtractOpCode);
        }

        public void Dispose()
        {
            Network.NetworkMessage -= OnNetworkMessage;
            PluginInterface!.UiBuilder.Draw -= OnDrawUI;
            PluginInterface.UiBuilder.OpenConfigUi -= OnOpenConfigUi;
        }
        private void ExtractOpCode(Task<string> task)
        {
            try
            {
                var regions = JsonConvert.DeserializeObject<List<OpcodeRegion>>(task.Result);
                if (regions == null)
                {
                    PluginLog.Warning("No regions found in opcode list");
                    return;
                }

                var region = regions.Find(r => r.Region == "Global");
                if (region == null || region.Lists == null)
                {
                    PluginLog.Warning("No global region found in opcode list");
                    return;
                }

                if (!region.Lists.TryGetValue("ServerZoneIpcType", out List<OpcodeList> serverZoneIpcTypes))
                {
                    PluginLog.Warning("No ServerZoneIpcType in opcode list");
                    return;
                }

                var eventPlay = serverZoneIpcTypes.Find(opcode => opcode.Name == "EventPlay");
                if (eventPlay == null)
                {
                    PluginLog.Warning("No EventPlay opcode in ServerZoneIpcType");
                    return;
                }

                expectedOpCode = eventPlay.Opcode;
                PluginLog.Debug($"Found EventPlay opcode {expectedOpCode:X4}");
            }
            catch (Exception e)
            {
                PluginLog.Error(e, "Could not download/extract opcodes: {}", e.Message);
            }
        }

        private void OnNetworkMessage(IntPtr dataPtr, ushort opCode, uint sourceActorId, uint targetActorId, NetworkMessageDirection direction)
        {
            if (direction != NetworkMessageDirection.ZoneDown || opCode != expectedOpCode)
                return;

            var data = new byte[32];
            Marshal.Copy(dataPtr, data, 0, data.Length);

            int eventId = BitConverter.ToInt32(data, 8);
            short scene = BitConverter.ToInt16(data, 12);
            int param5 = BitConverter.ToInt32(data, 28);

            // Fishing event?
            if (eventId != 0x00150001)
                return;

            // Fish hooked?
            if (scene != 5)
                return;

            switch (param5)
            {
                case 0x124:
                    // light tug (!)
                    hook(1);
                    break;

                case 0x125:
                    // medium tug (!!)
                    hook(2);
                    break;

                case 0x126:
                    // heavy tug (!!!)
                    hook(3);
                    break;
            }

        }

        private async void hook(int tug)
        {
            await Task.Delay(1500);
            switch (tug)
            {
                case 1:
                    // light tug (!)
                    _commandManager.Execute("/ac \"Precision Hookset\"");
                    await Task.Delay(100);
                    _commandManager.Execute("/ac \"Hook\" ");
                    break;

                case 2:
                    // medium tug (!!)
                    _commandManager.Execute("/ac \"Powerful Hookset\"");
                    await Task.Delay(100);
                    _commandManager.Execute("/ac \"Hook\" ");
                    break;

                case 3:
                    // heavy tug (!!!)
                    _commandManager.Execute("/ac \"Powerful Hookset\"");
                    await Task.Delay(100);
                    _commandManager.Execute("/ac \"Hook\" ");
                    break;

                default:

                    break;
            }
        }

        private void OnDrawUI()
        {
            if (!settingsVisible)
                return;

            if (ImGui.Begin("FishNotify", ref settingsVisible, ImGuiWindowFlags.AlwaysAutoResize))
            {
                if (expectedOpCode > -1)
                    ImGui.TextColored(ImGuiColors.HealerGreen, $"Status: OK, opcode = {expectedOpCode:X}");
                else
                    ImGui.TextColored(ImGuiColors.DalamudRed, "Status: No opcode :(");
            }
            ImGui.End();
        }
        private void OnOpenConfigUi()
        {
            settingsVisible = !settingsVisible;
        }
    }

    public class OpcodeRegion
    {
        public string Version { get; set; }
        public string Region { get; set; }
        public Dictionary<string, List<OpcodeList>> Lists { get; set; }
    }

    public class OpcodeList
    {
        public string Name { get; set; }
        public ushort Opcode { get; set; }
    }
}
