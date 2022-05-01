using Dalamud.Game;
using Dalamud.IoC;
using Dalamud.Data;
using Dalamud.Plugin;
using Dalamud.Game.Gui;
using Dalamud.Game.ClientState;
using Dalamud.Game.Network;
using Dalamud.Interface.Windowing;
using Dalamud.Game.Command;

namespace AutoHook {
    public class Service {
        public static void Initialize(DalamudPluginInterface pluginInterface)
            => pluginInterface.Create<Service>();

        public const string PluginName = "AutoHook";

        [PluginService][RequiredVersion("1.0")] public static DalamudPluginInterface PluginInterface { get; set; } = null!;
        [PluginService][RequiredVersion("1.0")] public static SigScanner SigScanner { get; private set; } = null!;
        [PluginService][RequiredVersion("1.0")] public static ChatGui Chat { get; private set; } = null!;
        [PluginService][RequiredVersion("1.0")] public static ClientState ClientState { get; private set; } = null!;
        [PluginService][RequiredVersion("1.0")] public static DataManager GameData { get; private set; } = null!;
        [PluginService][RequiredVersion("1.0")] public static GameNetwork Network { get; private set; } = null!;
        [PluginService][RequiredVersion("1.0")] public static CommandManager Commands { get; private set; } = null!;

        public static Configuration Configuration { get; set; } = null!;
        public static WindowSystem WindowSystem { get; } = new WindowSystem(PluginName);

    }
}
