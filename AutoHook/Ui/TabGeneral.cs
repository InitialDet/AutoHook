using System.Diagnostics;
using AutoHook.Data;
using AutoHook.Utils;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;

namespace AutoHook.Ui;

internal class TabGeneral : TabBaseConfig
{
    public override bool Enabled => true;
    public override string TabName => "通用";

    public override void DrawHeader()
    {
        ImGui.Text("通用设置");

        ImGui.Separator();
        ImGui.Spacing();
        if (ImGui.Button("Click here to report an issue or make a suggestion"))
        {
            Process.Start(new ProcessStartInfo { FileName = "https://github.com/InitialDet/AutoHook/issues", UseShellExecute = true });
        }
        ImGui.Spacing();

#if DEBUG

        if (ImGui.Button("测试"))
        {
            PluginLog.Debug($"IdenticalCast = {PlayerResources.HasStatus(IDs.Status.IdenticalCast)}");

        }
#endif
    }
    public override void Draw()
    {

        if (ImGui.BeginTabBar("TabBarsGeneral", ImGuiTabBarFlags.NoTooltip))
        {
            if (ImGui.BeginTabItem("默认直接抛竿###DC1"))
            {
                ImGui.PushID("TabDefaultCast");
                DrawDefaultCast();
                ImGui.PopID();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("默认以小钓大###DM1"))
            {
                ImGui.PushID("TabDefaultMooch");
                DrawDefaultMooch();
                ImGui.PopID();
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }

    }

    public void DrawDefaultCast()
    {
        ImGui.Spacing();
        ImGui.Checkbox("使用默认抛竿", ref Service.Configuration.DefaultCastConfig.Enabled);
        ImGuiComponents.HelpMarker("找不到特定鱼饵的设置时使用该默认设置。");

        ImGui.Indent();

        DrawInputDoubleMinTime(Service.Configuration.DefaultCastConfig);
        DrawInputDoubleMaxTime(Service.Configuration.DefaultCastConfig);
        DrawHookCheckboxes(Service.Configuration.DefaultCastConfig);
        DrawFishersIntuitionConfig(Service.Configuration.DefaultCastConfig);
        DrawCheckBoxDoubleTripleHook(Service.Configuration.DefaultCastConfig);
        //DrawPatienceConfig(Service.Configuration.DefaultCastConfig);

        ImGui.Unindent();

    }

    public void DrawDefaultMooch()
    {
        ImGui.Spacing();
        ImGui.Checkbox("使用默认以小钓大", ref Service.Configuration.DefaultMoochConfig.Enabled);
        ImGuiComponents.HelpMarker("找不到特定鱼饵的以小钓大设置时使用该默认设置。");

        ImGui.Indent();

        DrawInputDoubleMinTime(Service.Configuration.DefaultMoochConfig);
        DrawInputDoubleMaxTime(Service.Configuration.DefaultMoochConfig);
        DrawHookCheckboxes(Service.Configuration.DefaultMoochConfig);
        DrawFishersIntuitionConfig(Service.Configuration.DefaultMoochConfig);
        DrawCheckBoxDoubleTripleHook(Service.Configuration.DefaultMoochConfig);
        //DrawPatienceConfig(Service.Configuration.DefaultMoochConfig);

        ImGui.Unindent();
    }
}