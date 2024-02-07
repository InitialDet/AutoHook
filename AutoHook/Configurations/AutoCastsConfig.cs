using System.Collections.Generic;
using AutoHook.Classes;
using AutoHook.Classes.AutoCasts;

namespace AutoHook.Configurations;

public class AutoCastsConfig
{
    public bool EnableAll = false;
    
    public bool DontCancelMooch = true;
    
    public AutoCastLine CastLine = new();
    public AutoMooch CastMooch = new();
    public AutoChum CastChum = new();
    public AutoCollect CastCollect = new();
    public AutoCordial CastCordial = new();
    public AutoFishEyes CastFishEyes = new();
    //public AutoIdenticalCast CastIdenticalCast = new();
    public AutoMakeShiftBait CastMakeShiftBait= new();
    public AutoPatience CastPatience = new();
    public AutoPrizeCatch CastPrizeCatch = new();
    //public AutoSurfaceSlap CastSurfaceSlap = new();
    public AutoThaliaksFavor CastThaliaksFavor = new();
    
    //public AutoReleaseFish CastReleaseFish = new(); 
    
    public List<BaseActionCast> GetAutoActions()
    { 
        return new List<BaseActionCast>
        {
            CastThaliaksFavor,
            CastCordial,
            CastPatience,
            CastMakeShiftBait,
            CastChum,
            CastFishEyes,
            CastPrizeCatch,
            CastCollect,
        };
    }
    
}
