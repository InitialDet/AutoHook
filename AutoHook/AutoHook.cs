using System.Globalization;
using AutoHook.Configurations;
using AutoHook.IPC;
using AutoHook.Resources.Localization;
using AutoHook.SeFunctions;
using AutoHook.Spearfishing;
using AutoHook.Utils;
using Dalamud.Game.Command;
using Dalamud.Plugin;

namespace AutoHook;

public class AutoHook : IDalamudPlugin
{
    
    /*
     todo: autofood (not yet)
     todo: Add Guides
     */
    public string Name => UIStrings.AutoHook;

    private const string CmdAhCfg = "/ahcfg";
    private const string CmdAh = "/autohook";
    private const string CmdAhOn = "/ahon";
    private const string CmdAhOff = "/ahoff";
    private const string CmdAhtg = "/ahtg";

    private static PluginUi _pluginUi = null!;

    private static AutoGig _autoGig = null!;

    private readonly HookingManager _hookManager;

    private readonly PlayerResources _playerResources;

    public AutoHook(DalamudPluginInterface pluginInterface)
    {
        Service.Initialize(pluginInterface);
        AutoHookIPC.Init();
        Service.EventFramework = new EventFramework(Service.SigScanner);
        Service.EquipedBait = new CurrentBait(Service.SigScanner);
        Service.TugType = new SeTugType(Service.SigScanner);
        Service.PluginInterface.UiBuilder.Draw += Service.WindowSystem.Draw;
        Service.PluginInterface.UiBuilder.OpenConfigUi += OnOpenConfigUi;
        Service.Language = Service.ClientState.ClientLanguage;
        _playerResources = new PlayerResources();
        _playerResources.Initialize();

        Service.Configuration = Configuration.Load();
        UIStrings.Culture = new CultureInfo(Service.Configuration.CurrentLanguage);
        _pluginUi = new PluginUi();
        _autoGig = new AutoGig();

        Service.Commands.AddHandler(CmdAhOff, new CommandInfo(OnCommand)
        {
            HelpMessage = UIStrings.Disables_AutoHook
        });

        Service.Commands.AddHandler(CmdAhOn, new CommandInfo(OnCommand)
        {
            HelpMessage = UIStrings.Enables_AutoHook
        });

        Service.Commands.AddHandler(CmdAhCfg, new CommandInfo(OnCommand)
        {
            HelpMessage = UIStrings.Opens_Config_Window
        });
        
        Service.Commands.AddHandler(CmdAh, new CommandInfo(OnCommand)
        {
            HelpMessage = UIStrings.Opens_Config_Window
        });

        /*Service.Commands.AddHandler(CmdAh, new CommandInfo(OnCommand)
        {
            HelpMessage = UIStrings.Opens_Config_Window
        });*/

        Service.Commands.AddHandler(CmdAhtg, new CommandInfo(OnCommand)
        {
            HelpMessage = UIStrings.Toggles_AutoHook_On_Off
        });

        _hookManager = new HookingManager();

#if (DEBUG)
        OnOpenConfigUi();
#endif
    }

    private static void OnCommand(string command, string args)
    {
        switch (command.Trim())
        {
            case CmdAhCfg:
            case CmdAh:
                OnOpenConfigUi();
                break;
            case CmdAhOn:
                Service.Chat.Print(UIStrings.AutoHook_Enabled);
                Service.Configuration.PluginEnabled = true;
                break;
            case CmdAhOff:
                Service.Chat.Print(UIStrings.AutoHook_Disabled);
                Service.Configuration.PluginEnabled = false;
                break;
            case CmdAhtg when Service.Configuration.PluginEnabled:
                Service.Chat.Print(UIStrings.AutoHook_Disabled);
                Service.Configuration.PluginEnabled = false;
                break;
            case CmdAhtg:
                Service.Chat.Print(UIStrings.AutoHook_Enabled);
                Service.Configuration.PluginEnabled = true;
                break;
        }
    }

    public void Dispose()
    {
        _pluginUi.Dispose();
        _autoGig.Dispose();
        _hookManager.Dispose();
        _playerResources.Dispose();
        AutoHookIPC.Dispose();
        Service.Save();
        Service.PluginInterface.UiBuilder.Draw -= Service.WindowSystem.Draw;
        Service.PluginInterface.UiBuilder.OpenConfigUi -= OnOpenConfigUi;
        Service.Commands.RemoveHandler(CmdAh);
        Service.Commands.RemoveHandler(CmdAhCfg);
        Service.Commands.RemoveHandler(CmdAhOn);
        Service.Commands.RemoveHandler(CmdAhOff);
        Service.Commands.RemoveHandler(CmdAhtg);
    }

    private static void OnOpenConfigUi() => _pluginUi.Toggle();
}

