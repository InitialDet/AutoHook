using Dalamud.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using AutoHook.Classes;
using AutoHook.Configurations.old_config;
using AutoHook.Resources.Localization;
using AutoHook.Spearfishing.Enums;
using AutoHook.Utils;

namespace AutoHook.Configurations;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 3;
    public string CurrentLanguage { get; set; } = "en";

    public bool PluginEnabled = true;

    public HookPresets HookPresets = new();

    public bool AutoGigEnabled = false;
    public bool AutoGigHideOverlay = false;
    public bool AutoGigNaturesBountyEnabled = false;
    public bool AutoGigDrawFishHitbox = false;
    public bool AutoGigDrawGigHitbox = true;

    public SpearfishSpeed CurrentSpeed = SpearfishSpeed.All;
    public SpearfishSize CurrentSize = SpearfishSize.All;

    public Dictionary<string, int> GigSpacing = new Dictionary<string, int>();

    public bool ShowDebugConsole = false;
    
    public bool ShowChatLogs = true;

    public int DelayBetweenCastsMin = 600;
    public int DelayBetweenCastsMax = 1000;
    
    public int DelayBetweenHookMin = 0;
    public int DelayBetweenHookMax = 0;

    public bool ShowStatusHeader = true;

    public bool TransitionPopupViewed = false;
    
    // old config
    public List<BaitPresetConfig> BaitPresetList = new List<BaitPresetConfig>()!;

    public void Save()
    {
        Service.PluginInterface!.SavePluginConfig(this);
    }

    public void UpdateVersion()
    {
        if (Version == 1)
        {
            /*
            Service.PluginLog.Debug(@"Updating to Version 2");
            PresetConfig temp = new(UIStrings.New_Preset);
            temp.AddListOfHook(CustomBait);
            BaitPresetList.Add(temp);
            */
            Version = 2;
        }
        else if (Version == 2)
        {
            try
            {
                foreach (var preset in BaitPresetList)
                {
                    var newPreset = ConvertOldPreset(preset);
                    if (newPreset != null)
                        HookPresets.CustomPresets.Add(newPreset);
                }

                Version = 3;
            }
            catch (Exception e)
            {
                Service.PrintDebug($"[Configuration] {e.Message}");
            }
        }
    }

    private static PresetConfig? ConvertOldPreset(BaitPresetConfig? preset)
    {
        if (preset == null)
            return null; 
        
        var filteredBaits = new List<HookConfig>();
        var filteredMooch = new List<HookConfig>();
        foreach (var old in preset.ListOfBaits)
        {
            var matchingBait = PlayerResources.Baits.FirstOrDefault(b => b.Name == old.BaitName);
            var matchingFish = PlayerResources.Fishes.FirstOrDefault(f => f.Name == old.BaitName);

            if (matchingBait != null)
            {
                var newOne = new HookConfig(matchingBait);
                SetFieldNewClass(newOne, old);
                filteredBaits.Add(newOne);
            }
            else if (matchingFish != null)
            {
                var newOne = new HookConfig(matchingFish);
                SetFieldNewClass(newOne, old);
                filteredMooch.Add(newOne);
            }
        }

        PresetConfig newPreset = new($"[Old Version] {preset.PresetName}");
        newPreset.ListOfBaits = filteredBaits;
        newPreset.ListOfMooch = filteredMooch;
        return newPreset;
    }

    private static void SetFieldNewClass(HookConfig newOne, BaitConfig old)
    {
        var oldType = old.GetType();
        var newType = newOne.GetType();

        var oldFields = oldType.GetFields();
        var newFields = newType.GetFields();

        foreach (var sourceField in oldFields)
        {
            var targetField = newFields.FirstOrDefault(f => f.Name == sourceField.Name && f.FieldType == sourceField.FieldType);
            if (targetField != null)
            {
                var value = sourceField.GetValue(old);
                targetField.SetValue(newOne, value);
            }
        }
    }

    public void Initiate()
    {
        if (HookPresets.DefaultPreset.ListOfBaits.Count != 0)
            return;


        var bait = new BaitFishClass(UIStrings.All_Baits, 0);
        var mooch = new BaitFishClass(UIStrings.All_Mooches, 0);

        Service.PrintDebug($"Bait: {bait.Id} {bait.Name}, Mooch: {mooch.Id} {mooch.Name}");

        HookPresets.DefaultPreset.AddBaitConfig(new HookConfig(bait));
        HookPresets.DefaultPreset.AddMoochConfig(new HookConfig(mooch));
    }

    public static Configuration Load()
    {
        try
        {
            if (Service.PluginInterface.GetPluginConfig() is Configuration config)
            {
                config.Initiate();
                config.UpdateVersion();
                return config;
            }

            config = new Configuration();
            config.Initiate();
            config.Save();
            return config;
        }
        catch (Exception e)
        {
            Service.PrintDebug($"[Configuration] {e.Message}");
            throw;
        }
    }

    public static void ResetConfig()
    {
    }

    // Got the export/import function from the UnknownX7's ReAction repo
    public static string ExportActionStack(PresetConfig preset)
    {
        return CompressString(JsonConvert.SerializeObject(preset));
    }

    public static PresetConfig? ImportActionStack(string import)
    {
        if (import.StartsWith(OldV2ExportPrefix))
        {
            var old = JsonConvert.DeserializeObject<BaitPresetConfig>(DecompressString(import));
            return ConvertOldPreset(old);
        }
        
        return JsonConvert.DeserializeObject<PresetConfig>(DecompressString(import));
    }

    private const string ExportPrefix = "AH3_";
    private const string OldV2ExportPrefix = "AH_";
    
    public static string CompressString(string s)
    {
        var bytes = Encoding.UTF8.GetBytes(s);
        using var ms = new MemoryStream();
        using (var gs = new GZipStream(ms, CompressionMode.Compress))
            gs.Write(bytes, 0, bytes.Length);
        return ExportPrefix + Convert.ToBase64String(ms.ToArray());
    }

    public static string DecompressString(string s)
    {
        if (!s.StartsWith(ExportPrefix) && !s.StartsWith(OldV2ExportPrefix))
            throw new ApplicationException(UIStrings.DecompressString_Invalid_Import);
        
        var prefix = s.StartsWith(ExportPrefix) ? ExportPrefix : OldV2ExportPrefix;
        var data = Convert.FromBase64String(s[prefix.Length..]);
        var lengthBuffer = new byte[4];
        Array.Copy(data, data.Length - 4, lengthBuffer, 0, 4);
        var uncompressedSize = BitConverter.ToInt32(lengthBuffer, 0);

        var buffer = new byte[uncompressedSize];
        using (var ms = new MemoryStream(data))
        {
            using var gzip = new GZipStream(ms, CompressionMode.Decompress);
            gzip.Read(buffer, 0, uncompressedSize);
        }

        return Encoding.UTF8.GetString(buffer);
    }
}