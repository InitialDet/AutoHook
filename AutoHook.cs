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
using Lumina.Excel.GeneratedSheets;
using System.Threading;
using System.Linq;
using Action = Lumina.Excel.GeneratedSheets.Action;
using Dalamud.Game.ClientState;

namespace AutoHook {
    // Based on the FishNotify plugin
    public sealed class AutoHook : IDalamudPlugin {
        public string Name => "AutoHook";

        [PluginService]
        [RequiredVersion("1.0")]
        private DalamudPluginInterface PluginInterface { get; set; }
        [PluginService] [RequiredVersion("1.0")] public static SigScanner SigScanner { get; private set; } = null!;
        [PluginService] [RequiredVersion("1.0")] public static ChatGui Chat { get; private set; } = null!;

        [PluginService] [RequiredVersion("1.0")] public static ClientState ClientState { get; private set; } = null!;

        public CommandManager _commandManager;

        [PluginService] [RequiredVersion("1.0")] public static DataManager GameData { get; private set; } = null!;

        private static Lumina.Excel.ExcelSheet<Action> actionSheet;

        [PluginService]
        private GameNetwork Network { get; set; }
        private bool settingsVisible;
        private int expectedOpCode = -1;

        uint idHook = 296;   //Action
        uint idPrecision = 4179;  //Action
        uint idPowerful = 4103;  //Action
        uint idPatienceBuff = 850; //Status

        public AutoHook() {
            _commandManager = new CommandManager(SigScanner);
            Network!.NetworkMessage += OnNetworkMessage;
            PluginInterface!.UiBuilder.Draw += OnDrawUI;
            PluginInterface!.UiBuilder.OpenConfigUi += OnOpenConfigUi;

            var client = new HttpClient();
            client.GetStringAsync("https://raw.githubusercontent.com/karashiiro/FFXIVOpcodes/master/opcodes.min.json")
                .ContinueWith(ExtractOpCode);

            actionSheet = GameData.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>()!;
        }

        public void Dispose() {
            Network.NetworkMessage -= OnNetworkMessage;
            PluginInterface!.UiBuilder.Draw -= OnDrawUI;
            PluginInterface.UiBuilder.OpenConfigUi -= OnOpenConfigUi;
        }
        private void ExtractOpCode(Task<string> task) {
            try {
                var regions = JsonConvert.DeserializeObject<List<OpcodeRegion>>(task.Result);
                if (regions == null) {
                    PluginLog.Warning("No regions found in opcode list");
                    return;
                }

                var region = regions.Find(r => r.Region == "Global");
                if (region == null || region.Lists == null) {
                    PluginLog.Warning("No global region found in opcode list");
                    return;
                }

                if (!region.Lists.TryGetValue("ServerZoneIpcType", out List<OpcodeList> serverZoneIpcTypes)) {
                    PluginLog.Warning("No ServerZoneIpcType in opcode list");
                    return;
                }

                var eventPlay = serverZoneIpcTypes.Find(opcode => opcode.Name == "EventPlay");
                if (eventPlay == null) {
                    PluginLog.Warning("No EventPlay opcode in ServerZoneIpcType");
                    return;
                }

                expectedOpCode = eventPlay.Opcode;
                PluginLog.Debug($"Found EventPlay opcode {expectedOpCode:X4}");
            } catch (Exception e) {
                PluginLog.Error(e, "Could not download/extract opcodes: {}", e.Message);
            }
        }

        private void OnNetworkMessage(IntPtr dataPtr, ushort opCode, uint sourceActorId, uint targetActorId, NetworkMessageDirection direction) {
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

            switch (param5) {
                case 0x124:
                    // light tug (!)
                    hookFish(1);
                    break;

                case 0x125:
                    // medium tug (!!)
                    hookFish(2);
                    break;

                case 0x126:
                    // heavy tug (!!!)
                    hookFish(3);
                    break;
            }

        }

        private async void hookFish(int tug) {
            var hookName = actionSheet.GetRow(idHook)?.Name; // Default hook type = Hook

            if (ClientState.LocalPlayer?.StatusList != null) {// Check if player has Patience active
                foreach (var buff in ClientState.LocalPlayer.StatusList) {
                    if (buff.StatusId == idPatienceBuff) {
                        if (tug == 1) {
                            hookName = actionSheet.GetRow(idPrecision)?.Name;
                        } else if (tug == 2 || tug == 3) {
                            hookName = actionSheet.GetRow(idPowerful)?.Name;
                            break;
                        }
                    }
                }
            }

            if (hookName == null)
                return;

            //Chat.Print(hookName);
            await Task.Delay(1500);
            _commandManager.Execute($"/ac \"{hookName}\"");
        }

        private void OnDrawUI() {
            if (!settingsVisible)
                return;

            if (ImGui.Begin("AutoHook", ref settingsVisible, ImGuiWindowFlags.AlwaysAutoResize)) {
                if (expectedOpCode > -1)
                    ImGui.TextColored(ImGuiColors.HealerGreen, $"Status: OK, opcode = {expectedOpCode:X}");
                else
                    ImGui.TextColored(ImGuiColors.DalamudRed, "Status: No opcode :(");
            }
            ImGui.End();
        }
        private void OnOpenConfigUi() {
            settingsVisible = !settingsVisible;
        }
    }

    public class OpcodeRegion {
        public string Version { get; set; }
        public string Region { get; set; }
        public Dictionary<string, List<OpcodeList>> Lists { get; set; }
    }

    public class OpcodeList {
        public string Name { get; set; }
        public ushort Opcode { get; set; }
    }
}
