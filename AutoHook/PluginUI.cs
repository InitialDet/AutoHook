using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using Dalamud.Interface.Colors;
using System.Collections.Generic;
using AutoHook.Ui;
using System.Numerics;

namespace AutoHook;

public class PluginUI : Window, IDisposable
{

    private readonly List<TabConfig> tabs = new()
        {
            new GeneralTab(),
            new MoochTab(),
            new AutoCastsTab()
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
        }
    }

    public override void OnClose()
    {
        Service.Configuration.Save();
    }
}
