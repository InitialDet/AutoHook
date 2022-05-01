using Dalamud.Game.Command;
using Dalamud.Game.Network;
using Dalamud.Logging;
using Dalamud.Plugin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Action = Lumina.Excel.GeneratedSheets.Action;

namespace AutoHook {
    // Based on the FishNotify plugin
    public class AutoHook : IDalamudPlugin {
        public string Name => "AutoHook";

        private const string CmdAHCfg = "/ahcfg";
        private const string CmdAHOn  = "/ahon";
        private const string CmdAHOff = "/ahoff";

        public CustomCommandManager _commandManager;

        private static Lumina.Excel.ExcelSheet<Action> actionSheet = null!;

        private int expectedOpCode = -1;

        private static PluginUI PluginUI = null!;

        const uint idHook = 296;         //Action
        const uint idPrecision = 4179;   //Action
        const uint idPowerful = 4103;    //Action
        const uint idPatienceBuff = 850; //Status

        public AutoHook(DalamudPluginInterface pluginInterface) {
            Service.Initialize(pluginInterface);
            _commandManager = new CustomCommandManager(Service.SigScanner);

            Service.Network.NetworkMessage += OnNetworkMessage;
            Service.PluginInterface!.UiBuilder.Draw += Service.WindowSystem.Draw;
            Service.PluginInterface!.UiBuilder.OpenConfigUi += OnOpenConfigUi;
            Service.Configuration = Configuration.Load();

            PluginUI = new PluginUI();

            var client = new HttpClient();
            client.GetStringAsync("https://raw.githubusercontent.com/karashiiro/FFXIVOpcodes/master/opcodes.min.json")
                .ContinueWith(ExtractOpCode);

            actionSheet = Service.GameData.GetExcelSheet<Action>()!;

            Service.Commands.AddHandler(CmdAHOff, new CommandInfo(OnCommand) {
                HelpMessage = "Disables AutoHook"
            });

            Service.Commands.AddHandler(CmdAHOn, new CommandInfo(OnCommand) {
                HelpMessage = "Enables AutoHook"
            });

            Service.Commands.AddHandler(CmdAHCfg, new CommandInfo(OnCommand) {
                HelpMessage = "Opens Config Window"
            });   
        }

        private void OnCommand(string command, string args) {
            PluginLog.Debug(command);
            if (command.Trim().Equals(CmdAHCfg))
                OnOpenConfigUi();

            if (command.Trim().Equals(CmdAHOn)) {
                Service.Chat.Print("AutoHook Enabled");
                Service.Configuration.General.AutoHookEnabled = true;
            }

            if (command.Trim().Equals(CmdAHOff)) {
                Service.Chat.Print("AutoHook Disabled");
                Service.Configuration.General.AutoHookEnabled = false;
            }
        }

        public void Dispose() {
            PluginUI.Dispose();
            Service.Configuration.Save();
            Service.Network.NetworkMessage -= OnNetworkMessage;
            Service.PluginInterface!.UiBuilder.Draw -= Service.WindowSystem.Draw;
            Service.PluginInterface.UiBuilder.OpenConfigUi -= OnOpenConfigUi;
            Service.Commands.RemoveHandler(CmdAHCfg);
            Service.Commands.RemoveHandler(CmdAHOn);
            Service.Commands.RemoveHandler(CmdAHOff);
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

                if (!region.Lists.TryGetValue("ServerZoneIpcType", out var serverZoneIpcTypes)) {
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
            if (!Service.Configuration.General.AutoHookEnabled)
                return;

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

            if (Service.ClientState.LocalPlayer?.StatusList != null) {// Check if player has Patience active
                foreach (var buff in Service.ClientState.LocalPlayer.StatusList) {
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
        private void OnOpenConfigUi() => PluginUI.Toggle();
    }

    public class OpcodeRegion {
        public string Version { get; set; } = null!;
        public string Region { get; set; } = null!;
        public Dictionary<string, List<OpcodeList>> Lists { get; set; } = null!;
    }

    public class OpcodeList {
        public string Name { get; set; } = null!;
        public ushort Opcode { get; set; }
    }
}
