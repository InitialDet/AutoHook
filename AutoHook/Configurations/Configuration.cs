using AutoHook.Configurations;
using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace AutoHook {
    [Serializable]
    public class Configuration : IPluginConfiguration {
        public int Version { get; set; } = 1;

        public GeneralSettings General = new();

        public void Save() {
            Service.PluginInterface!.SavePluginConfig(this);
        }

        public static Configuration Load() {
            if (Service.PluginInterface.GetPluginConfig() is Configuration config) {
                return config;
            }

            config = new Configuration();
            config.Save();
            return config;
        }
    }
}
