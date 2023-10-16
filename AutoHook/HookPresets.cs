using System.Collections.Generic;
using System.Linq;
using AutoHook.Configurations;

namespace AutoHook;

public class HookPresets
{
    public PresetConfig DefaultPreset = new(@"DefaultPreset");
    
    public List<PresetConfig> CustomPresets = new();
    
    public PresetConfig? SelectedPreset = null;
    
    // create two methods to add and remove presets
    public void AddPreset(PresetConfig presetConfig)
    {
        if (CustomPresets.All(preset => preset.PresetName != presetConfig.PresetName))
        {
            CustomPresets.Add(presetConfig);
        }
    }
    
    public void RemovePreset(PresetConfig presetConfig)
    {
        if (CustomPresets.Any(preset => preset.PresetName == presetConfig.PresetName))
        {
            CustomPresets.Remove(presetConfig);
        }
    }
}
