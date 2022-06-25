using AutoHook.Configurations;
using AutoHook.FishTimer;
using AutoHook.SeFunctions;
using AutoHook.Utils;
using Dalamud.Game.Command;
using Dalamud.Logging;
using Dalamud.Plugin;
using SeFunctions;

namespace AutoHook;

public class AutoHook : IDalamudPlugin
{
    public string Name => "AutoHook";

    private const string CmdAHCfg = "/ahcfg";
    private const string CmdAHOn = "/ahon";
    private const string CmdAHOff = "/ahoff";
    
    private static PluginUI PluginUI = null!;

    public HookingManager FishHooker;

    public AutoHook(DalamudPluginInterface pluginInterface)
    {
        Service.Initialize(pluginInterface);
        Service.EventFramework = new EventFramework(Service.SigScanner);
        Service.CurrentBait = new CurrentBait(Service.SigScanner);
        Service.TugType = new SeTugType(Service.SigScanner);
        Service.PluginInterface!.UiBuilder.Draw += Service.WindowSystem.Draw;
        Service.PluginInterface!.UiBuilder.OpenConfigUi += OnOpenConfigUi;
        Service.Configuration = Configuration.Load();
        Service.Language = Service.ClientState.ClientLanguage;

        PlayerResources.Initialize();

        PluginUI = new PluginUI();

        Service.Commands.AddHandler(CmdAHOff, new CommandInfo(OnCommand)
        {
            HelpMessage = "Disables AutoHook"
        });

        Service.Commands.AddHandler(CmdAHOn, new CommandInfo(OnCommand)
        {
            HelpMessage = "Enables AutoHook"
        });

        Service.Commands.AddHandler(CmdAHCfg, new CommandInfo(OnCommand)
        {
            HelpMessage = "Opens Config Window"
        });

        FishHooker = new HookingManager();
        FishHooker.Enable();

        #if (DEBUG)
            OnOpenConfigUi();
        #endif
    }

    private void OnCommand(string command, string args)
    {
        if (command.Trim().Equals(CmdAHCfg))
            OnOpenConfigUi();

        if (command.Trim().Equals(CmdAHOn))
        {
            Service.Chat.Print("AutoHook Enabled");
            Service.Configuration.PluginEnabled = true;
        }

        if (command.Trim().Equals(CmdAHOff))
        {
            Service.Chat.Print("AutoHook Disabled");
            Service.Configuration.PluginEnabled = false;
        }
    }

    public void Dispose()
    {
        PluginUI.Dispose();
        FishHooker.Dispose();
        Service.Configuration.Save();
        Service.PluginInterface!.UiBuilder.Draw -= Service.WindowSystem.Draw;
        Service.PluginInterface.UiBuilder.OpenConfigUi -= OnOpenConfigUi;
        Service.Commands.RemoveHandler(CmdAHCfg);
        Service.Commands.RemoveHandler(CmdAHOn);
        Service.Commands.RemoveHandler(CmdAHOff);
    }

    private void OnOpenConfigUi() => PluginUI.Toggle();
}

