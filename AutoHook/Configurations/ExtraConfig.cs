using AutoHook.Classes;

namespace AutoHook.Configurations;

public class ExtraConfig
{
    public bool Enabled = false;
    
    public bool SwapBaitIntuitionGain = false;
    public BaitFishClass BaitToSwapIntuitionGain = new();
    
    public bool SwapBaitIntuitionLost = false;
    public BaitFishClass BaitToSwapIntuitionLost = new();
    
    public bool SwapPresetIntuitionGain = false;
    public string PresetToSwapIntuitionGain = "-";
    
    public bool SwapPresetIntuitionLost = false;
    public string PresetToSwapIntuitionLost = "-";
}