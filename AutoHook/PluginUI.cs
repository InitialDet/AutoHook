using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using Dalamud.Interface.Colors;

namespace AutoHook {
    public class PluginUI : Window, IDisposable {
      
        public PluginUI() : base($"{Service.PluginName} Settings") {
            Service.WindowSystem.AddWindow(this);

            Flags |= ImGuiWindowFlags.NoScrollbar;
            Flags |= ImGuiWindowFlags.NoScrollWithMouse;
        }

        public void Dispose() {
            Service.Configuration.Save();
            Service.WindowSystem.RemoveWindow(this);
        }

        public override void Draw() {
            if (!IsOpen)
                return;

            Utils.Draw.Checkbox("Enable AutoHook", ref Service.Configuration.General.AutoHookEnabled, "Enables AutoHook");

            ImGui.Indent(28.0f * ImGuiHelpers.GlobalScale);

            if (Service.Configuration.General.AutoHookEnabled) {
                ImGui.TextColored(ImGuiColors.HealerGreen, "AutoHook Enabled");
            } else {
                ImGui.TextColored(ImGuiColors.DalamudRed, "AutoHook Disabled");
            }

            ImGui.Indent(-25.0f * ImGuiHelpers.GlobalScale);

            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, ImGuiHelpers.ScaledVector2(10, 8));

            ImGui.Spacing();

            ImGui.End();
        }

        public override void OnClose() {
            Service.Configuration.Save();
        }
    }
}
