using Dalamud.Configuration;
using Dalamud.Logging;
using GatherBuddy.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Text;

namespace AutoHook.Configurations;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 2;

    public bool PluginEnabled = true;

    public AutoCastsConfig AutoCastsCfg = new AutoCastsConfig();

    public BaitConfig DefaultCastConfig = new("DefaultCast");
    public BaitConfig DefaultMoochConfig = new("DefaultMooch");

    public List<BaitConfig> CustomBait = new(); // old config, not deleting because im scared

    public BaitPresetConfig? CurrentPreset = null;
    public List<BaitPresetConfig> BaitPresetList = new List<BaitPresetConfig>()!;

    public bool AutoGigEnabled = false;
    public bool AutoGigHideOverlay = false;
    public bool AutoGigNaturesBountyEnabled = false;
    public bool AutoGigDrawFishHitbox = false;
    public bool AutoGigDrawGigHitbox = true;

    public SpearfishSpeed currentSpeed = SpearfishSpeed.All;
    public SpearfishSize currentSize = SpearfishSize.All;

    public Dictionary<string, int> GigSpacing = new Dictionary<string, int>();

    public void Save()
    {
        Service.PluginInterface!.SavePluginConfig(this);
    }

    public void UpdateVersion()
    {
        if (Version == 1)
        {
            PluginLog.Debug("Updating to Version 2");
            CurrentPreset = new("New Preset");
            CurrentPreset.AddListOfHook(CustomBait);
            BaitPresetList.Add(CurrentPreset);

            Version = 2;
        }
    }

    public static Configuration Load()
    {
        if (Service.PluginInterface.GetPluginConfig() is Configuration config)
        {
            config.UpdateVersion();
            return config;
        }

        config = new Configuration();
        config.Save();
        return config;

    }

    // Got the export/import function from the UnknownX7's ReAction repo
    public static string ExportActionStack(BaitPresetConfig preset)
        => CompressString(JsonConvert.SerializeObject(preset));

    public static BaitPresetConfig? ImportActionStack(string import)
        => JsonConvert.DeserializeObject<BaitPresetConfig>(DecompressString(import));

    private const string exportPrefix = "AH_";

    public static string CompressString(string s)
    {
        var bytes = Encoding.UTF8.GetBytes(s);
        using var ms = new MemoryStream();
        using (var gs = new GZipStream(ms, CompressionMode.Compress))
            gs.Write(bytes, 0, bytes.Length);
        return exportPrefix + Convert.ToBase64String(ms.ToArray());
    }

    public static string DecompressString(string s)
    {
        if (!s.StartsWith(exportPrefix))
            throw new ApplicationException("This is not a valid import.");
        var data = Convert.FromBase64String(s[exportPrefix.Length..]);
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

