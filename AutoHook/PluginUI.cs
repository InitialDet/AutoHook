using AutoHook.Resources.Localization;
using AutoHook.Ui;
using AutoHook.Utils;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;

namespace AutoHook;

public class PluginUi : Window, IDisposable
{
    private readonly List<BaseTab> _tabs = new()
    {
        new TabDefaultPreset(),
        new TabCustomPresets(),
        //new CastAndGPChangeLater(),
        new TabAutoGig(),
        new TabConfigGuides()
    };

    public PluginUi() : base(string.Format(UIStrings.Plugin_Name_Settings, Service.PluginName))
    {
        Service.WindowSystem.AddWindow(this);

        Flags |= ImGuiWindowFlags.NoScrollbar;
        Flags |= ImGuiWindowFlags.NoScrollWithMouse;
    }

    public void Dispose()
    {
        Service.Save();

        foreach (var tab in _tabs)
        {
            tab.Dispose();
        }

        Service.WindowSystem.RemoveWindow(this);
    }

    private void DrawTransitionPopup()
    {
        var windowSize = new Vector2(1024 * ImGuiHelpers.GlobalScale,
           ImGui.GetTextLineHeightWithSpacing() * 1 + 2 * ImGui.GetFrameHeightWithSpacing());

        ImGui.SetNextWindowSize(windowSize);
        ImGui.SetNextWindowPos((ImGui.GetIO().DisplaySize - windowSize) / 2);

        var popup = ImRaii.Popup("TransitionPopup",
    ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.Modal);

        if (!popup)
            return;

        ImGui.TextWrapped($"Hello, thank you for updating AutoHook. This is a one-time message just to inform you that AutoHook has moved plugin repositories. This will be the last update on the current repository. You will have to transition to the new repository to recieve further updates.");

        ImGui.SetCursorPosY(ImGui.GetWindowSize().Y - ImGui.GetFrameHeight() - ImGui.GetStyle().WindowPadding.Y);
        if (ImGui.Button("Ok, I understand", new(ImGui.GetContentRegionAvail().X / 2, default)))
        {
            Service.Configuration.TransitionPopupViewed = true;
            ImGui.CloseCurrentPopup();
        }
        ImGui.SameLine();
        if (ImGui.Button("Give me more information!", new(ImGui.GetContentRegionAvail().X, default)))
        {
            Util.OpenLink($"https://github.com/InitialDet/AutoHook");
        }

    }
    public override void Draw()
    {
        if (Service.PluginInterface.SourceRepository != AutoHook.PunishRepo && !Service.Configuration.TransitionPopupViewed)
        {
            ImGui.OpenPopup("TransitionPopup");
        }

        if (!IsOpen)
            return;

        if (Service.PluginInterface.SourceRepository != AutoHook.PunishRepo)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudRed);
            ImGui.GetIO().FontGlobalScale = 1.25f;
            ImGui.PushFont(ImGui.GetFont());
            ImGui.TextWrapped($"AutoHook has moved repositories! You will not recieve further updates until you uninstall and transition. Click here for more information!");
            if (ImGui.IsItemClicked())
            {
                Util.OpenLink($"https://github.com/InitialDet/AutoHook");
            }
            ImGui.GetIO().FontGlobalScale = 1f;
            ImGui.PopFont();
            ImGui.PopStyleColor();
        }

        //ImGui.TextColored(ImGuiColors.DalamudYellow, "Major plugin rework!!! Please, recheck all of your presets");
        ImGui.Spacing();
        DrawUtil.Checkbox(UIStrings.Enable_AutoHook, ref Service.Configuration.PluginEnabled,
            UIStrings.PluginUi_Draw_Enables_Disables);


        ShowKofi();
        ShowPaypal();

        ImGui.Indent();

        if (Service.Configuration.PluginEnabled)
            ImGui.TextColored(ImGuiColors.HealerGreen, UIStrings.Plugin_Enabled);
        else
            ImGui.TextColored(ImGuiColors.DalamudRed, UIStrings.Plugin_Disabled);

        ImGui.Unindent();
        ImGui.Spacing();

        DrawChangelog();
        ImGui.SameLine();
        DrawLanguageSelector();
        ImGui.Spacing();
        if (Service.Configuration.ShowDebugConsole)
        {

            if (ImGui.Button(UIStrings.Open_Console))
            {
                Service.OpenConsole = !Service.OpenConsole;
            }

            ImGui.SameLine();
#if DEBUG
            TestButtons();
#endif

            Debug();

            ImGui.Spacing();
        }

        if (Service.Configuration.ShowStatusHeader)
            ImGui.TextColored(ImGuiColors.DalamudViolet, Service.Status);

        DrawTabs();
        DrawTransitionPopup();
    }

    private void Debug()
    {
        if (!Service.OpenConsole)
            return;

        ImGui.PushID(@"debug");
        ImGui.SetNextItemWidth(300);
        if (ImGui.Begin($"DebugWIndows", ref Service.OpenConsole))
        {
            var logs = Service.LogMessages.ToArray().Reverse().ToList();
            for (var i = 0; i < logs.Count; i++)
            {
                if (i == 0)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudYellow);
                    ImGui.TextWrapped($"{i + 1} - {logs[i]}");
                    ImGui.PopStyleColor();
                }
                else
                    ImGui.TextWrapped($"{i + 1} - {logs[i]}");

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();
            }
        }

        ImGui.End();
        ImGui.PopID();
    }

    private static unsafe void TestButtons()
    {
        if (ImGui.Button(@"Check"))
        {
        }
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
                    if (ImGui.BeginChild(tab.TabName, new Vector2(0, 0), true))
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
        Service.Save();
    }

    public static void ShowKofi()
    {

        ImGui.SameLine();
        string buttonText = UIStrings.Support_me_on_Ko_fi;
        ImGui.PushStyleColor(ImGuiCol.Button, 0xFF000000 | 0x005E5BFF);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0xDD000000 | 0x005E5BFF);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0xAA000000 | 0x005E5BFF);

        if (ImGui.Button(buttonText))
        {
            OpenBrowser(@"https://ko-fi.com/initialdet");
        }

        ImGui.PopStyleColor(3);
    }

    public static void ShowPaypal()
    {

        ImGui.SameLine();
        string buttonText = @"PayPal";
        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Button, 0xFFA06020);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0xDD000000 | 0x005E5BFF);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0xAA000000 | 0x005E5BFF);

        if (ImGui.Button(buttonText))
        {
            OpenBrowser(@"https://www.paypal.com/donate/?business=PDZJVTF5484UA&no_recurring=0&currency_code=USD");
        }

        ImGui.PopStyleColor(3);
    }

    private void DrawLanguageSelector()
    {
        ImGui.SetNextItemWidth(55);
        var languages = new List<string>
        {
            @"en",
            @"es",
            @"fr",
            @"de",
            @"ja",
            @"ko",
            @"zh"
        };
        var currentLanguage = languages.IndexOf(Service.Configuration.CurrentLanguage);

        if (!ImGui.Combo(UIStrings.PluginUi_Language, ref currentLanguage, languages.ToArray(), languages.Count))
            return;

        Service.Configuration.CurrentLanguage = languages[currentLanguage];
        UIStrings.Culture = new CultureInfo(Service.Configuration.CurrentLanguage);
        Service.Save();
        //Service.Chat.Print("Saved");
    }

    private static void OpenBrowser(string url)
    {
        Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
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
        if (ImGui.Begin($"{UIStrings.Changelog}", ref _openChangelog))
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
                    ImGui.TextWrapped("Minor Changes");
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
                            ImGui.TextWrapped("Minor Changes");

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
            new Version("3.0.4.0")
            {
                MainChanges =
                {
                    "(by Jaksuhn) Added auto cast collector's glove",
                    "(by Jaksuhn) Added option to refresh Patience early",
                }
            },
            new Version("3.0.3.0")
            {
                MainChanges =
                {
                    "(by Jaksuhn) Added swap preset/bait on Spectral Currents",
                    "(by Jaksuhn) Added more Surface Slap options",
                    "(by Jaksuhn) Added option to chum only when intution duration is greater than x seconds",
                },
                MinorChanges =
                {
                    "Fixed an issue with swapping both preset and bait at the same time",
                    "More IPC options"
                }
            },
            new Version("3.0.2.0")
            {
                MainChanges =
                {
                    "(by Jaksuhn) Added IPC",
                    "(by Jaksuhn) Added makeshift bait only under intuition option"
                },
                MinorChanges =
                {
                    "Added a new command to change the current preset"
                }
            },
            new Version("3.0.1.0")
            {
                MainChanges =
                {
                    "Added new subtab 'Extra' for extra options",
                    "Added options to change bait/presets when gaining/losing intuition",
                    "(Config) Added optional delay for hooking or auto casting",
                },
                MinorChanges =
                {
                    "Pantience I/II has priority over MakeShift Bait if both options are enabled",
                    "Added a new command to open the plugin menu",
                    "Minor text changes"
                }
            },
            new Version("3.0.0.0")
            {
                MainChanges =
                {
                    "Major plugin rework to try and support complex conditions",
                    "AutoCasts are now preset based, you can now have multiple presets with different AutoCasts",
                    "Merged AutoCast and Gp Config into a single tab",
                    "Bait and Mooch hook configs are now separated into different tabs for better organization",
                    "Added a new 'Fish' Tab, which contains new options related to fish caught",
                    "Its now possible to change the current bait (or preset) when a fish is caught X times",
                    "Localization Updates"
                },
                MinorChanges =
                {
                    "Fixed localization issues",
                    "Fixed AutoCast not working if not hooking after a bite"
                }
            },
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