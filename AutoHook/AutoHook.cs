using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
    private const string CmdAhPreset = "/ahpreset";

    public static string PunishRepo => "https://love.puni.sh/ment.json";


    private static readonly Dictionary<string, string> CommandHelp = new()
    {
        {CmdAhOff, UIStrings.Disables_AutoHook},
        {CmdAhOn, UIStrings.Enables_AutoHook},
        {CmdAhCfg, UIStrings.Opens_Config_Window},
        {CmdAh, UIStrings.Opens_Config_Window},
        {CmdAhtg, UIStrings.Toggles_AutoHook_On_Off},
        {CmdAhPreset, UIStrings.Set_preset_command}
    };
    
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
        
        foreach (var (command, help) in CommandHelp)
        {
            Service.Commands.AddHandler(command, new CommandInfo(OnCommand)
            {
                HelpMessage = help
            });
        }
        
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
            case CmdAhPreset:
                SetPreset(args);
                break;
        }
    }
    
    private static void SetPreset(string presetName)
    {
        var preset = Service.Configuration.HookPresets.CustomPresets.FirstOrDefault(x => x.PresetName == presetName);
        if (preset == null)
        {
            Service.Chat.Print(UIStrings.Preset_not_found);
            return;
        }
         
        Service.Configuration.HookPresets.SelectedPreset = preset;
        Service.Chat.Print(@$"{UIStrings.Preset_set_to_} {preset.PresetName}");
        Service.Save();
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
        
        foreach (var (command, _) in CommandHelp)
        {
            Service.Commands.RemoveHandler(command);
        }
    }

    private static void OnOpenConfigUi() => _pluginUi.Toggle();
}

