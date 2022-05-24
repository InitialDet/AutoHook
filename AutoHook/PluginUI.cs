using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using Dalamud.Interface.Colors;
using System.Collections.Generic;
using AutoHook.Ui;
using System.Numerics;

namespace AutoHook
{
    public class PluginUI : Window, IDisposable
    {

        private readonly List<TabConfig> tabs = new()
        {
            new GeneralConfigurationTab(),
            new MoochTab()
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

            Utils.Draw.Checkbox("Enable AutoHook", ref Service.Configuration.AutoHookEnabled, "Enables AutoHook");

            ImGui.Indent(28.0f * ImGuiHelpers.GlobalScale);

            if (Service.Configuration.AutoHookEnabled)
            {
                ImGui.TextColored(ImGuiColors.HealerGreen, "AutoHook Enabled");
            }
            else
            {
                ImGui.TextColored(ImGuiColors.DalamudRed, "AutoHook Disabled");
            }
            ImGui.Indent(-25.0f * ImGuiHelpers.GlobalScale);
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, ImGuiHelpers.ScaledVector2(10, 8));
            ImGui.Spacing();

            DrawTabs();
        }

        private void DrawTabs()
        {
            if (ImGui.BeginTabBar("ChillFramesTabBar", ImGuiTabBarFlags.NoTooltip))
            {
                foreach (var tab in tabs)
                {
                    if (tab.Enabled == false) continue;

                    if (ImGui.BeginTabItem(tab.TabName))
                    {
                        ImGui.PushID(tab.TabName);

                        tab.DrawHeader();
                        if (ImGui.BeginChild("ChillFramesSettings", new Vector2(0, 0), true))
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
}