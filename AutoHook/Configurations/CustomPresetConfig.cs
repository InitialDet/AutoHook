using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;

namespace AutoHook.Configurations;

public class BaitPresetConfig
{
    private string presetName = "New Preset";

    private List<BaitConfig> _listOfBaits = new();

    public string PresetName { get => presetName; set => presetName = value; }
    public List<BaitConfig> ListOfBaits { get => _listOfBaits; set => _listOfBaits = value; }

    public BaitPresetConfig(string presetName)
    {
        if (ListOfBaits == null) 
            ListOfBaits = new();

        PresetName = presetName;
    }

    public void AddBaitConfig(BaitConfig baitConfig)
    {
        if (ListOfBaits != null && !ListOfBaits.Contains(baitConfig))
        {
            ListOfBaits.Add(baitConfig);
        }
    }

    public void RemoveBaitConfig(BaitConfig baitConfig)
    {
        if (ListOfBaits != null && ListOfBaits.Contains(baitConfig))
        {
            ListOfBaits.Remove(baitConfig);
        }
    }

    // This is just for the conversion of the COnfig version 1 to version 2
    public void AddListOfHook(List<BaitConfig> listOfBaits) 
    {
        ListOfBaits.AddRange(listOfBaits);
    }

    public override bool Equals(object? obj)
    {
        return obj is BaitPresetConfig settings &&
               presetName == settings.presetName;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(presetName + "a");
    }

    public void RenamePreset(string name) {
        PresetName = name;
    }


}