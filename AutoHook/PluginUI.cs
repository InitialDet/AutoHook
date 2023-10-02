using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using Dalamud.Interface.Colors;
using System.Collections.Generic;
using AutoHook.Ui;
using System.Numerics;
using System.Diagnostics;
using AutoHook.Resources.Localization;

namespace AutoHook;

public class PluginUi : Window, IDisposable
{

    private readonly List<TabBaseConfig> _tabs = new()
        {
            new TabGeneral(),
            new TabPresets(),
            new TabAutoCasts(),
            new TabGPConfig(),
            new TabAutoGig()
        };

    public PluginUi() : base(string.Format(UIStrings.Plugin_Name_Settings, Service.PluginName))
    {
        Service.WindowSystem.AddWindow(this);

        Flags |= ImGuiWindowFlags.NoScrollbar;
        Flags |= ImGuiWindowFlags.NoScrollWithMouse;
    }

    public void Dispose()
    {
        Service.Configuration.Save();

        foreach (var tab in _tabs)
        {
            tab.Dispose();
        }

        Service.WindowSystem.RemoveWindow(this);
    }

    public override void Draw()
    {
        if (!IsOpen)
            return;

        Utils.DrawUtil.Checkbox(UIStrings.Enable_AutoHook, ref Service.Configuration.PluginEnabled, UIStrings.PluginUi_Draw_Enables_Disables);
        ShowKofi();
        //paypal bad madge
        //ShowPaypal();
        ImGui.Indent();

        if (Service.Configuration.PluginEnabled)
        {
            ImGui.TextColored(ImGuiColors.HealerGreen, UIStrings.Plugin_Enabled);
        }
        else
        {
            ImGui.TextColored(ImGuiColors.DalamudRed, UIStrings.Plugin_Disabled);
        }
        ImGui.Unindent();
        ImGui.Spacing();

        DrawTabs();
    }

    private void DrawTabs()
    {
        if (ImGui.BeginTabBar(@"AutoHook###TabBars", ImGuiTabBarFlags.NoTooltip))
        {
            foreach (var tab in _tabs)
            {
                if (tab.Enabled == false) continue;

                if (ImGui.BeginTabItem(tab.TabName))
                {
                    ImGui.PushID(tab.TabName);

                    tab.DrawHeader();
                    if (ImGui.BeginChild(@"AutoHook###Child", new Vector2(0, 0), true))
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

    private static void ShowKofi()
    {
        string buttonText = UIStrings.Support_me_on_Ko_fi;
        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Button, 0xFF000000 | 0x005E5BFF);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0xDD000000 | 0x005E5BFF);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0xAA000000 | 0x005E5BFF);

        if (ImGui.Button(buttonText))
        {
            OpenBrowser(@"https://ko-fi.com/initialdet");
        }

        ImGui.PopStyleColor(3);
    }

    private static void OpenBrowser(string url)
    {
        Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
    }
}
