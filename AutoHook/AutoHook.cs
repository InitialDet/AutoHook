using AutoHook.Configurations;
using AutoHook.Resources.Localization;
using AutoHook.SeFunctions;
using AutoHook.Spearfishing;
using AutoHook.Utils;
using Dalamud.Game.Command;
using Dalamud.Plugin;
using SeFunctions;
namespace AutoHook;

public class AutoHook : IDalamudPlugin
{
    public string Name => UIStrings.AutoHook;

    private const string CmdAhCfg = "/ahcfg";
    private const string CmdAh = "/ah";
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
        Service.EventFramework = new EventFramework(Service.SigScanner);
        Service.CurrentBait = new CurrentBait(Service.SigScanner);
        Service.TugType = new SeTugType(Service.SigScanner);
        Service.PluginInterface.UiBuilder.Draw += Service.WindowSystem.Draw;
        Service.PluginInterface.UiBuilder.OpenConfigUi += OnOpenConfigUi;
        Service.Configuration = Configuration.Load();
        Service.Language = Service.ClientState.ClientLanguage;

        _playerResources = new PlayerResources();
        _playerResources.Initialize();

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
        Service.Configuration.Save();
        Service.PluginInterface.UiBuilder.Draw -= Service.WindowSystem.Draw;
        Service.PluginInterface.UiBuilder.OpenConfigUi -= OnOpenConfigUi;
        Service.Commands.RemoveHandler(CmdAhCfg);
        Service.Commands.RemoveHandler(CmdAh);
        Service.Commands.RemoveHandler(CmdAhOn);
        Service.Commands.RemoveHandler(CmdAhOff);
        Service.Commands.RemoveHandler(CmdAhtg);
    }

    private static void OnOpenConfigUi() => _pluginUi.Toggle();
}

