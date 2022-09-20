using System;

namespace GatherBuddy.Enums;

public enum SpearfishSpeed : ushort
{
    All = 0,
    SuperSlow     = 100,
    ExtremelySlow = 150,
    VerySlow      = 200,
    Slow          = 250,
    Average       = 300,
    Fast          = 350,
    VeryFast      = 400,
    ExtremelyFast = 450,
    SuperFast     = 500,
    HyperFast     = 550,
    LynFast       = 600,

    
}

public static class SpearFishSpeedExtensions
{
    public static string ToName(this SpearfishSpeed speed)
        => speed switch
        {
            SpearfishSpeed.All => "All",
            SpearfishSpeed.SuperSlow     => "Super Slow",
            SpearfishSpeed.ExtremelySlow => "Extremely Slow",
            SpearfishSpeed.VerySlow      => "Very Slow",
            SpearfishSpeed.Slow          => "Slow",
            SpearfishSpeed.Average       => "Average",
            SpearfishSpeed.Fast          => "Fast",
            SpearfishSpeed.VeryFast      => "Very Fast",
            SpearfishSpeed.ExtremelyFast => "Extremely Fast",
            SpearfishSpeed.SuperFast     => "Super Fast",
            SpearfishSpeed.HyperFast     => "Hyper Fast",
            SpearfishSpeed.LynFast       => "Mega Fast",
            
            _                            => $"{(ushort)speed}",
        };
}
