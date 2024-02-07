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

    public bool SwapBaitSpectralCurrentGain = false;
    public BaitFishClass BaitToSwapSpectralCurrentGain = new();

    public bool SwapBaitSpectralCurrentLost = false;
    public BaitFishClass BaitToSwapSpectralCurrentLost = new();

    public bool SwapPresetSpectralCurrentGain = false;
    public string PresetToSwapSpectralCurrentGain = "-";

    public bool SwapPresetSpectralCurrentLost = false;
    public string PresetToSwapSpectralCurrentLost = "-";
}