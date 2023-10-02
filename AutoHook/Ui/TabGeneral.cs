using System.ComponentModel;
using System.Diagnostics;
using System.Numerics;
using AutoHook.Resources.Localization;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using ImGuiNET;

namespace AutoHook.Ui;

internal class TabGeneral : TabBaseConfig
{
    public override bool Enabled => true;
    public override string TabName => UIStrings.TabnameGeneral;

    public override void DrawHeader()
    {
        ImGui.Text(UIStrings.DrawHeader_GeneralSettings);

        ImGui.Separator();

        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudYellow);
        ImGui.TextWrapped(UIStrings.DrawHeader_CheckChangelog);
        ImGui.PopStyleColor();

        ImGui.Spacing();

        DrawChangelog();

        ImGui.Spacing();

        if (ImGui.Button(UIStrings.DrawHeader_ClickToReportAnIssue))
        {
            Process.Start(new ProcessStartInfo { FileName = "https://github.com/InitialDet/AutoHook/issues", UseShellExecute = true });
        }

        ImGui.Spacing();

#if DEBUG

        ImGui.SameLine();
        if (ImGui.Button(UIStrings.DrawHeader_Testing))
        {
            ImGui.OpenPopup("changelog");
        }

#endif
    }

    public override void Draw()
    {

        if (ImGui.BeginTabBar(@"TabBarsGeneral", ImGuiTabBarFlags.NoTooltip))
        {
            if (ImGui.BeginTabItem($"{UIStrings.DefaultCast}###DC1"))
            {
                ImGui.PushID("TabDefaultCast");
                DrawDefaultCast();
                ImGui.PopID();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem($"{UIStrings.DefaultMooch}###DM1"))
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
        ImGui.Checkbox(UIStrings.UseDefaultCast, ref Service.Configuration.DefaultCastConfig.Enabled);
        ImGuiComponents.HelpMarker(UIStrings.DefaultHookingBehavior);

        ImGui.Indent();

        DrawInputDoubleMinTime(Service.Configuration.DefaultCastConfig);
        DrawInputDoubleMaxTime(Service.Configuration.DefaultCastConfig);
        DrawChumMinMaxTime(Service.Configuration.DefaultCastConfig);
        DrawHookCheckboxes(Service.Configuration.DefaultCastConfig);
        DrawFishersIntuitionConfig(Service.Configuration.DefaultCastConfig);
        DrawCheckBoxDoubleTripleHook(Service.Configuration.DefaultCastConfig);

        ImGui.Unindent();
    }

    public void DrawDefaultMooch()
    {
        ImGui.Spacing();
        ImGui.Checkbox(UIStrings.UseDefaultMooch, ref Service.Configuration.DefaultMoochConfig.Enabled);
        ImGuiComponents.HelpMarker(UIStrings.DefaultMoochingBehavior);

        ImGui.Indent();

        DrawInputDoubleMinTime(Service.Configuration.DefaultMoochConfig);
        DrawInputDoubleMaxTime(Service.Configuration.DefaultMoochConfig);
        DrawChumMinMaxTime(Service.Configuration.DefaultMoochConfig);
        DrawHookCheckboxes(Service.Configuration.DefaultMoochConfig);
        DrawFishersIntuitionConfig(Service.Configuration.DefaultMoochConfig);
        DrawCheckBoxDoubleTripleHook(Service.Configuration.DefaultMoochConfig);

        ImGui.Unindent();
    }

    bool _openChangelog = false;
    [Localizable(false)]
    private void DrawChangelog()
    {
        if (ImGui.Button(UIStrings.Changelog))
        {
            //ImGui.OpenPopup("changelog");
            _openChangelog = !_openChangelog;

            if (_openChangelog)
            {
                //ImGui.SetNextWindowSize(new Vector2(400, 250));
            }
        }

        if (_openChangelog)
        {
            ImGui.SetNextWindowSize(new Vector2(400, 0));
            if (ImGui.Begin($"{UIStrings.Changelog}", ref _openChangelog, ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudYellow);
                ImGui.TextWrapped("2.4.4.0");
                ImGui.PopStyleColor();
                ImGui.Separator();
                //Current Version
                ImGui.TextWrapped("- It's now possible to enable both Double and Triple hook (hold shift when selecting the options)");
      
                ImGui.Spacing();

                if (ImGui.TreeNode("2.4.4.x"))
                {
                    ImGui.TextWrapped("- Removed captalization for bait names");
                    ImGui.TextWrapped("- Bug fixes");

                    ImGui.TreePop();
                }

                if (ImGui.BeginChild("old_versions", new Vector2(0, 150), true))
                {
                    if (ImGui.TreeNode("2.4.3.0"))
                    {
                        ImGui.TextWrapped("- Added Watered Cortials for AutoCasts");
                        ImGui.Spacing();
                        ImGui.TextWrapped("2.4.3.x");
                        ImGui.TextWrapped("- Fixed duplicated GP Configs");

                        ImGui.TreePop();
                    }

                    if (ImGui.TreeNode("2.4.2.0"))
                    {
                        ImGui.TextWrapped("- Added customizable hitbox for autogig");
                        ImGui.Indent();
                        ImGui.TextWrapped("Each Size and Speed combination has its own hitbox config");
                        ImGui.Unindent();
                        ImGui.TextWrapped("- Added an option to see the fish hitbox when spearfishing");
                        ImGui.TextWrapped("- (experimental) Nature's Bounty will be used when the target fish appears on screen ");
                        ImGui.TextWrapped("- Added changelog button");

                        ImGui.Spacing();
                        ImGui.Separator();
                        ImGui.Spacing();

                        ImGui.TextWrapped("2.4.2.x");
                        ImGui.TextWrapped("- Gig hitbox is now enabled by default");
                        ImGui.TextWrapped("- Fixed the order of the Chum Timer Min/Max fields");
                        ImGui.TextWrapped("- Fixed some options not saving correctly");

                        ImGui.TreePop();
                    }

                    if (ImGui.TreeNode("2.4.1.0"))
                    {
                        ImGui.TextWrapped("- Added options to cast Mooch only when under the effect of Fisher's Intuition");
                        ImGui.TreePop();
                    }

                    if (ImGui.TreeNode("2.4.0.0"))
                    {
                        ImGui.TextWrapped("- Presets for custom baits added, you can now swap configs without needing to recreate it every time");
                        ImGui.TextWrapped("- Added options to cast Chum only when under the effect of Fisher's Intuition");
                        ImGui.TextWrapped("- Added an option to only cast Prize Catch when Mooch II is not available, saving you 100gp if all you want is to mooch");
                        ImGui.TextWrapped("- Added Custom Timer when under the effect of Chum");
                        ImGui.TextWrapped("- Added an option to only use Prize Catch when under the effect of Identical Cast");
                        ImGui.TextWrapped("- Upgrade to .net7 and API8");
                        ImGui.TreePop();
                    }
                    ImGui.EndChild();
                }
            }
            ImGui.End();
        }
    }
}