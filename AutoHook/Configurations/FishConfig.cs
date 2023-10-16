using AutoHook.Classes;
using AutoHook.Classes.AutoCasts;
using AutoHook.Enums;
using AutoHook.Resources.Localization;

namespace AutoHook.Configurations;

public class FishConfig
{
    public bool Enabled = true;

    public BaitFishClass Fish;
    
    public bool StopAfterCaught = false;
    public int StopAfterCaughtLimit = 1;
    
    public AutoIdenticalCast IdenticalCast = new(UIStrings.UseIdenticalCast);
    public AutoSurfaceSlap SurfaceSlap = new(UIStrings.UseSurfaceSlap);
    public AutoMooch Mooch = new();
    
    public bool SwapBait = false;
    public BaitFishClass BaitToSwap = new();
    public int SwapBaitCount = 1;
    
    public bool SwapPresets = false;
    public string PresetToSwap = "-"; 
    public int SwapPresetCount = 1;
    
    public bool NeverMooch = false;
    
    public CatchSteps StopFishingStep = CatchSteps.None;

    public FishConfig(BaitFishClass fish)
    {
        Fish = fish;
        // ok this is not the best way, but im tired and it works for now so be nice to me
        Mooch.Name = UIStrings.Always_Mooch; 
    }
}