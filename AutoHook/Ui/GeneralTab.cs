using System.Diagnostics;
using AutoHook.Data;
using AutoHook.Utils;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;

namespace AutoHook.Ui;

internal class GeneralTab : TabConfig
{
    public override bool Enabled => true;
    public override string TabName => "General";

    public override void DrawHeader()
    {
        ImGui.Text("General settings");
        
        ImGui.Separator();
        ImGui.Spacing();
        if (ImGui.Button("Click here to report an issue or make an suggestion"))
        {
            Process.Start(new ProcessStartInfo { FileName = "https://github.com/InitialDet/AutoHook/issues", UseShellExecute = true });
        }
        ImGui.Spacing();

#if DEBUG

        if (ImGui.Button("Testing"))
        {
            PluginLog.Debug($"IdenticalCast = {PlayerResources.HasStatus(IDs.Status.IdenticalCast)}");
 
        }
#endif
    }
    public override void Draw()
    {
        DrawDefaultCast();
        ImGui.Separator();
        DrawDefaultMooch();
    }

    public void DrawDefaultCast()
    {
        ImGui.PushID($"{TabName}-DefaultCast");
        if (ImGui.CollapsingHeader("Default Cast Line Setting"))
        {
            ImGui.Checkbox("Use Default Cast", ref Service.Configuration.DefaultCastConfig.Enabled);
            ImGuiComponents.HelpMarker("This is the default hooking behavior if no specific Bait Config is found.");

            ImGui.Indent();

            DrawInputDoubleMinTime(Service.Configuration.DefaultCastConfig);
            DrawInputDoubleMaxTime(Service.Configuration.DefaultCastConfig);
            DrawHookCheckboxes(Service.Configuration.DefaultCastConfig);
            DrawFishersIntuitionConfig(Service.Configuration.DefaultCastConfig);
            DrawCheckBoxDoubleTripleHook(Service.Configuration.DefaultCastConfig);
            //DrawPatienceConfig(Service.Configuration.DefaultCastConfig);

            ImGui.Unindent();

            ImGui.EndTabBar();
        }
        ImGui.PopID();
    }

    public void DrawDefaultMooch()
    {
        ImGui.PushID($"{TabName}-DefaultMooch");
        if (ImGui.CollapsingHeader("Default Mooch Setting"))
        {
            ImGui.Checkbox("Use Default Mooch", ref Service.Configuration.DefaultMoochConfig.Enabled);
            ImGuiComponents.HelpMarker("This is the default hooking behavior if no specific Mooch Config is found.");

            ImGui.Indent();

            DrawInputDoubleMinTime(Service.Configuration.DefaultMoochConfig);
            DrawInputDoubleMaxTime(Service.Configuration.DefaultMoochConfig);
            DrawHookCheckboxes(Service.Configuration.DefaultMoochConfig);
            DrawFishersIntuitionConfig(Service.Configuration.DefaultMoochConfig);
            DrawCheckBoxDoubleTripleHook(Service.Configuration.DefaultMoochConfig);
            //DrawPatienceConfig(Service.Configuration.DefaultMoochConfig);

            ImGui.Unindent();

        }
        ImGui.PopID();
    }
}