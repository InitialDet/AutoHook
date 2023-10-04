using System.Collections.Generic;
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
        ImGui.TextWrapped("Localization options were added, but currently only English is available. If you want to help with the translation, please visit the link below");
        ImGui.PopStyleColor();

        ImGui.Spacing();
        

        if (ImGui.Button(UIStrings.TabGeneral_DrawHeader_Localization_Help))
        {
            Process.Start(new ProcessStartInfo
                { FileName = "https://crowdin.com/project/autohook-plugin-localization", UseShellExecute = true });
        }

        ImGui.Spacing();
        
        DrawChangelog();

        ImGui.Spacing();

#if DEBUG

       ImGui.Separator();
       ImGui.Spacing();
        if (ImGui.Button(UIStrings.DrawHeader_Testing))
        {
           
        }
        ImGui.Spacing();

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

    private bool _openChangelog = false;

    [Localizable(false)]
    private void DrawChangelog()
    {
        if (ImGui.Button(UIStrings.Changelog))
            _openChangelog = !_openChangelog;
        
        if (!_openChangelog)
            return;
        
        ImGui.SetNextWindowSize(new Vector2(400, 0));
        if (ImGui.Begin($"{UIStrings.Changelog}", ref _openChangelog, ImGuiWindowFlags.AlwaysAutoResize))
        {
            var changes = PluginChangeLog.Versions;
            
            if (changes.Count > 0)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudYellow);
                ImGui.TextWrapped($"{changes[0].VersionNumber}");
                ImGui.PopStyleColor();
                ImGui.Separator();

                //First value is the current Version
                foreach (var mainChange in changes[0].MainChanges)
                {
                    ImGui.TextWrapped($"- {mainChange}");
                }

                ImGui.Spacing();

                if (changes[0].MinorChanges.Count > 0)
                {
                    ImGui.TextWrapped("Bug Fixes");
                    foreach (var minorChange in changes[0].MinorChanges)
                    {
                        ImGui.TextWrapped($"- {minorChange}");
                    }
                }

                ImGui.Separator();
                
                if (ImGui.BeginChild("old_versions", new Vector2(0, 150), true))
                {
                    for (var i = 1; i < changes.Count; i++)
                    {
                        if (!ImGui.TreeNode($"{changes[i].VersionNumber}"))
                            continue;
                        
                        foreach (var mainChange in changes[i].MainChanges)
                            ImGui.TextWrapped($"- {mainChange}");

                        if (changes[i].MinorChanges.Count > 0)
                        {
                            ImGui.Spacing();
                            ImGui.TextWrapped("Bug Fixes");
                            
                            foreach (var minorChange in changes[i].MinorChanges)
                                ImGui.TextWrapped($"- {minorChange}");
                        }
                        ImGui.TreePop();
                    }
                }
                ImGui.EndChild();
            }
        }

        ImGui.End();
    }

    [Localizable(false)]
    public static class PluginChangeLog
    {
        public static readonly List<Version> Versions = new()
        {
            new Version("2.5.0.0")
            {
                MainChanges =
                {
                    "Added localization for Chinese, French, German,Japanese and Korean",
                    "API9 update"
                }
            },
            new Version("2.4.4.0")
            {
                MainChanges =
                {
                    "It's now possible to enable both Double and Triple hook (hold shift when selecting the options)",
                },
                MinorChanges =
                {
                    "Removed captalization for bait names",
                }
            },
            new Version("2.4.3.0")
            {
                MainChanges =
                {
                    "Added Watered Cortials for AutoCasts"
                },
                MinorChanges =
                {
                    "Fixed duplicated GP Configs"
                }
            },
            new Version("2.4.2.0")
            {
                MainChanges =
                {
                    "Added customizable hitbox for autogig",
                    "Added an option to see the fish hitbox when spearfishing",
                    "(experimental) Nature's Bounty will be used when the target fish appears on screen",
                    "Added changelog button"
                },
                MinorChanges =
                {
                    "Gig hitbox is now enabled by default",
                    "Fixed the order of the Chum Timer Min/Max fields",
                    "Fixed some options not saving correctly"
                }
            },
            new Version("2.4.1.0")
            {
                MainChanges = { "Added options to cast Mooch only when under the effect of Fisher's Intuition" }
            },
            new Version("2.4.0.0")
            {
                MainChanges =
                {
                    "Presets for custom baits added, you can now swap configs without needing to recreate it every time",
                    "Added options to cast Chum only when under the effect of Fisher's Intuition",
                    "Added an option to only cast Prize Catch when Mooch II is not available, saving you 100gp if all you want is to mooch",
                    "Added Custom Timer when under the effect of Chum",
                    "Added an option to only use Prize Catch when under the effect of Identical Cast",
                    "Upgrade to .net7 and API8"
                }
            }
        };


        public class Version
        {
            public string VersionNumber { get; set; }
            public List<string> MainChanges { get; set; }
            public List<string> MinorChanges { get; set; }

            public Version(string versionNumber)
            {
                VersionNumber = versionNumber;
                MainChanges = new List<string>();
                MinorChanges = new List<string>();
            }
        }
    }
}