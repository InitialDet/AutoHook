using AutoHook.Configurations;
using System.Linq;

namespace AutoHook.IPC
{
    internal class AutoHookIPC
    {
        private const string SetPluginStateStr = $"{nameof(AutoHook)}.{nameof(SetPluginState)}";
        private const string SetAutoGigStateStr = $"{nameof(AutoHook)}.{nameof(SetAutoGigState)}";
        private const string SetPresetStr = $"{nameof(AutoHook)}.{nameof(SetPreset)}";
        private const string CreateAndSelectPresetStr = $"{nameof(AutoHook)}.{nameof(CreateAndSelectAnonymousPreset)}";
        private const string DeletePresetStr = $"{nameof(AutoHook)}.{nameof(DeleteSelectedPreset)}";
        private const string SetAutoGigSizeStr = $"{nameof(AutoHook)}.{nameof(SetAutoGigSize)}";
        private const string SetAutoGigSpeedStr = $"{nameof(AutoHook)}.{nameof(SetAutoGigSpeed)}";
        private const string DeleteAllAnonymousPresetsStr = $"{nameof(AutoHook)}.{nameof(DeleteAllAnonymousPresets)}";

        internal static void Init()
        {
            Service.PluginInterface.GetIpcProvider<bool, object>(SetPluginStateStr).RegisterAction(SetPluginState);
            Service.PluginInterface.GetIpcProvider<bool, object>(SetAutoGigStateStr).RegisterAction(SetAutoGigState);
            Service.PluginInterface.GetIpcProvider<int, object>(SetAutoGigSizeStr).RegisterAction(SetAutoGigSize);
            Service.PluginInterface.GetIpcProvider<int, object>(SetAutoGigSpeedStr).RegisterAction(SetAutoGigSpeed);
            Service.PluginInterface.GetIpcProvider<string, object>(SetPresetStr).RegisterAction(SetPreset);
            Service.PluginInterface.GetIpcProvider<string, object>(CreateAndSelectPresetStr).RegisterAction(CreateAndSelectAnonymousPreset);
            Service.PluginInterface.GetIpcProvider<object>(DeletePresetStr).RegisterAction(DeleteSelectedPreset);
            Service.PluginInterface.GetIpcProvider<object>(DeleteAllAnonymousPresetsStr).RegisterAction(DeleteAllAnonymousPresets);
        }

        private static void SetPluginState(bool state) { Service.Configuration.PluginEnabled = state; Service.Save(); }
        private static void SetAutoGigState(bool state) { Service.Configuration.AutoGigEnabled = state; Service.Save(); }
        private static void SetAutoGigSize(int size) { Service.Configuration.CurrentSize = (Spearfishing.Enums.SpearfishSize)size; Service.Save(); }
        private static void SetAutoGigSpeed(int speed) { Service.Configuration.CurrentSpeed = (Spearfishing.Enums.SpearfishSpeed)speed; Service.Save(); }

        private static void SetPreset(string preset)
        {
            Service.Configuration.HookPresets.SelectedPreset = Service.Configuration.HookPresets.CustomPresets.FirstOrDefault(x => x.PresetName == preset);
            Service.Save();
        }

        private static void CreateAndSelectAnonymousPreset(string preset)
        {
            var _import = Configuration.ImportActionStack(preset);
            if (_import == null) return;
            var name = $"anon_{_import.PresetName}";
            _import.RenamePreset(name);
            Service.Configuration.HookPresets.AddPreset(_import);
            Service.Configuration.HookPresets.SelectedPreset = Service.Configuration.HookPresets.CustomPresets.FirstOrDefault(x => x.PresetName == name);
            Service.Save();
        }

        private static void DeleteSelectedPreset()
        {
            var selected = Service.Configuration.HookPresets.SelectedPreset;
            if (selected == null) return;
            Service.Configuration.HookPresets.RemovePreset(selected);
            Service.Configuration.HookPresets.SelectedPreset = null;
            Service.Save();
        }

        private static void DeleteAllAnonymousPresets()
        {
            Service.Configuration.HookPresets.CustomPresets.RemoveAll(p => p.PresetName.StartsWith("anon_"));
            Service.Save();
        }

        internal static void Dispose()
        {
            Service.PluginInterface.GetIpcProvider<bool, object>(SetPluginStateStr).UnregisterAction();
            Service.PluginInterface.GetIpcProvider<bool, object>(SetAutoGigStateStr).UnregisterAction();
            Service.PluginInterface.GetIpcProvider<int, object>(SetAutoGigSizeStr).UnregisterAction();
            Service.PluginInterface.GetIpcProvider<int, object>(SetAutoGigSpeedStr).UnregisterAction();
            Service.PluginInterface.GetIpcProvider<string, object>(SetPresetStr).UnregisterAction();
            Service.PluginInterface.GetIpcProvider<string, object>(CreateAndSelectPresetStr).UnregisterAction();
            Service.PluginInterface.GetIpcProvider<object>(DeletePresetStr).UnregisterAction();
            Service.PluginInterface.GetIpcProvider<object>(DeleteAllAnonymousPresetsStr).UnregisterAction();
        }
    }
}
