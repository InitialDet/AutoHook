using AutoHook.Ui;
using Dalamud.Interface.Components;
using ImGuiNET;

namespace AutoHook
{
    internal class GeneralConfigurationTab : TabConfig
    {
        public override bool Enabled => true;
        public override string TabName => "General";

        public override void Draw()
        {
            DrawDefaultCast();
            ImGui.Separator();
            DrawDefaultMooch();
        }

        public override void DrawHeader()
        {
            ImGui.Text("General settings");
        }

        public void DrawDefaultMooch()
        {
            ImGui.PushID($"{TabName}-DefaultMooch");
            ImGui.Checkbox("Use Default Mooch", ref Service.Configuration.DefaultMoochSettings.Enabled);
            ImGuiComponents.HelpMarker("This is the default hooking behavior if no specific Mooch Config is found.");

            ImGui.Indent();
            DrawInputDoubleMaxTime(Service.Configuration.DefaultMoochSettings);
            DrawHookCheckboxes(Service.Configuration.DefaultMoochSettings);
            ImGui.Unindent();

            ImGui.PopID();
        }

        public void DrawDefaultCast()
        {
            ImGui.PushID($"{TabName}-DefaultCast");
            ImGui.Checkbox("Use Default Cast", ref Service.Configuration.DefaultCastSettings.Enabled);
            ImGuiComponents.HelpMarker("This is the default hooking behavior if no specific Bait Config is found.");

            ImGui.Indent();
            DrawInputDoubleMaxTime(Service.Configuration.DefaultCastSettings);
            DrawHookCheckboxes(Service.Configuration.DefaultCastSettings);
            ImGui.Unindent();

            ImGui.PopID();
        }
    }
}