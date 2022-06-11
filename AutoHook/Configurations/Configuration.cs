using Dalamud.Configuration;
using System;
using System.Collections.Generic;

namespace AutoHook.Configurations;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public bool PluginEnabled = true;
   
    public AutoCastsConfig AutoCastsCfg = new AutoCastsConfig();

    public HookConfig DefaultCastConfig = new("DefaultCast");
    public HookConfig DefaultMoochConfig = new("DefaultMooch");
    public List<HookConfig> CustomBait = new();

    public void Save()
    {
        Service.PluginInterface!.SavePluginConfig(this);
    }

    public static Configuration Load()
    {
        if (Service.PluginInterface.GetPluginConfig() is Configuration config)
        {
            return config;
        }

        config = new Configuration();
        config.Save();
        return config;
    }
}

