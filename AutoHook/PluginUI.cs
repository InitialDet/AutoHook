using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using Dalamud.Interface.Colors;
using System.Collections.Generic;
using AutoHook.Ui;
using System.Numerics;
using System.Diagnostics;

namespace AutoHook;

public class PluginUI : Window, IDisposable
{

    private readonly List<TabBaseConfig> tabs = new()
        {
            new TabGeneral(),
            new TabPresets(),
            new TabAutoCasts(),
            new TabGPConfig(),
            new TabAutoGig()

        };

    public PluginUI() : base($"{Service.PluginName} Settings")
    {
        Service.WindowSystem.AddWindow(this);

        Flags |= ImGuiWindowFlags.NoScrollbar;
        Flags |= ImGuiWindowFlags.NoScrollWithMouse;
    }

    public void Dispose()
    {
        Service.Configuration.Save();

        foreach (var tab in tabs)
        {
            tab.Dispose();
        }

        Service.WindowSystem.RemoveWindow(this);
    }

    public override void Draw()
    {
        if (!IsOpen)
            return;

        Utils.DrawUtil.Checkbox("Enable AutoHook", ref Service.Configuration.PluginEnabled, "Enables/Disables the plugin for you");
        ShowKofi();
        //paypal bad madge
        //ShowPaypal();
        ImGui.Indent();

        if (Service.Configuration.PluginEnabled)
        {
            ImGui.TextColored(ImGuiColors.HealerGreen, "Plugin Enabled");
        }
        else
        {
            ImGui.TextColored(ImGuiColors.DalamudRed, "Plugin Disabled");
        }
        ImGui.Unindent();
        ImGui.Spacing();

        DrawTabs();
    }

    private void DrawTabs()
    {
        if (ImGui.BeginTabBar("AutoHook###TabBars", ImGuiTabBarFlags.NoTooltip))
        {
            foreach (var tab in tabs)
            {
                if (tab.Enabled == false) continue;

                if (ImGui.BeginTabItem(tab.TabName))
                {
                    ImGui.PushID(tab.TabName);

                    tab.DrawHeader();
                    if (ImGui.BeginChild("AutoHook###Childs", new Vector2(0, 0), true))
                    {
                        tab.Draw();
                        ImGui.EndChild();
                    }
                    ImGui.PopID();
                    ImGui.EndTabItem();
                }
            }
            ImGui.EndTabBar();
        }
    }

    public override void OnClose()
    {
        Service.Configuration.Save();
    }

    public static void ShowKofi()
    {
        string buttonText = "Support on Ko-fi";
        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Button, 0xFF000000 | 0x005E5BFF);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0xDD000000 | 0x005E5BFF);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0xAA000000 | 0x005E5BFF);

        if (ImGui.Button(buttonText))
        {
            OpenBrowser("https://ko-fi.com/initialdet");
        }

        ImGui.PopStyleColor(3);
    }

    public static void OpenBrowser(string url)
    {
        Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
    }
}
