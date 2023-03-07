using System;
using System.Numerics;
using AutoHook.Configurations;
using AutoHook.Enums;
using AutoHook.Utils;
using Dalamud.Hooking;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using ImGuiNET;

namespace AutoHook.Ui;
abstract class TabBaseConfig : IDisposable
{
    public abstract string TabName { get; }
    public abstract bool Enabled { get; }
    public static string StrHookWeak => "Hook Weak (!)";
    public static string StrHookStrong => "Hook Strong (!!)";
    public static string StrHookLegendary => "Hook Legendary (!!!)";

    public abstract void DrawHeader();

    public abstract void Draw();

    public virtual void Dispose() { }

    public void DrawDeleteBaitButton(BaitConfig cfg)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        if (ImGui.Button($"{FontAwesomeIcon.Trash.ToIconChar()}", new Vector2(ImGui.GetFrameHeight(), 0)) && ImGui.GetIO().KeyShift)
        {
            Service.Configuration.CurrentPreset?.ListOfBaits.RemoveAll(x => x.BaitName == cfg.BaitName);
            Service.Configuration.Save();
        }
        ImGui.PopFont();

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Hold SHIFT to delete.");
    }

    public void DrawHookCheckboxes(BaitConfig cfg)
    {
        DrawSelectTugs(StrHookWeak, ref cfg.HookWeakEnabled, ref cfg.HookTypeWeak);
        DrawSelectTugs(StrHookStrong, ref cfg.HookStrongEnabled, ref cfg.HookTypeStrong);
        DrawSelectTugs(StrHookLegendary, ref cfg.HookLegendaryEnabled, ref cfg.HookTypeLegendary);
    }

    public void DrawSelectTugs(string hook, ref bool enabled, ref HookType type)
    {
       
        ImGui.Checkbox(hook, ref enabled);
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("\"Hook\" will be used if Patience is not up");

        if (enabled)
        {
            ImGui.Indent();
            if (ImGui.RadioButton($"Precision Hookset###{TabName}{hook}1", type == HookType.Precision))
            {
                type = HookType.Precision;
                Service.Configuration.Save();
            }

            if (ImGui.RadioButton($"Powerful Hookset###{TabName}{hook}2", type == HookType.Powerful))
            {
                type = HookType.Powerful;
                Service.Configuration.Save();
            }
            ImGui.Unindent();
        }
    }

    public void DrawInputTextName(BaitConfig cfg)
    {
        string matchText = new string(cfg.BaitName);
        ImGui.SetNextItemWidth(-260 * ImGuiHelpers.GlobalScale);
        if (ImGui.InputText("Mooch/Bait Name", ref matchText, 64, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
        {
            if (cfg.BaitName != matchText && Service.Configuration.CurrentPreset != null && Service.Configuration.CurrentPreset.ListOfBaits.Contains(new BaitConfig(matchText)))
                cfg.BaitName = "Bait already exists";
            else
                cfg.BaitName = matchText;

            Service.Configuration.Save();
        };
    }

    public void DrawInputDoubleMaxTime(BaitConfig cfg)
    {
        ImGui.SetNextItemWidth(100 * ImGuiHelpers.GlobalScale);
        if (ImGui.InputDouble("Max. Wait", ref cfg.MaxTimeDelay, .1, 1, "%.1f%"))
        {
            switch (cfg.MaxTimeDelay)
            {
                case 0.1:
                    cfg.MaxTimeDelay = 2;
                    break;
                case <= 0:
                case <= 1.9: //This makes the option turn off if delay = 2 seconds when clicking the minus.
                    cfg.MaxTimeDelay = 0;
                    break;
                case > 99:
                    cfg.MaxTimeDelay = 99;
                    break;
            }
        }
        ImGui.SameLine();
        ImGuiComponents.HelpMarker("Hook will be used after the defined amount of time has passed\nMin. time: 2s (because of animation lock)\n\nSet Zero (0) to disable, and dont make this lower than the Min. Wait");
    }

    public void DrawInputDoubleMinTime(BaitConfig cfg)
    {
        ImGui.SetNextItemWidth(100 * ImGuiHelpers.GlobalScale);
        if (ImGui.InputDouble("Min. Wait", ref cfg.MinTimeDelay, .1, 1, "%.1f%"))
        {
            switch (cfg.MinTimeDelay)
            {
                case <= 0:
                    cfg.MinTimeDelay = 0;
                    break;
                case > 99:
                    cfg.MinTimeDelay = 99;
                    break;
            }
        }

        ImGui.SameLine();
        ImGuiComponents.HelpMarker("Hook will NOT be used until the minimum time has passed.\n\nEx: If you set the number as 14 and something bites after 8 seconds, the fish will not to be hooked\n\nSet Zero (0) to disable");
    }

    public void DrawChumMinMaxTime(BaitConfig cfg)
    {

        if (ImGui.Button("Chum Timer"))
        {
            ImGui.OpenPopup(str_id: "chum_timer");
        }

        if (ImGui.BeginPopup("chum_timer"))
        {
            ImGui.Spacing();
            Utils.DrawUtil.Checkbox("Enable Chum Timers", ref cfg.UseChumTimer, "Enable Min/Max times when under the effect of Chum");
            ImGui.Separator();

            ImGui.SetNextItemWidth(100 * ImGuiHelpers.GlobalScale);
            if (ImGui.InputDouble("Min. Wait", ref cfg.MinChumTimeDelay, .1, 1, "%.1f%"))
            {
                switch (cfg.MinTimeDelay)
                {
                    case <= 0:
                        cfg.MinTimeDelay = 0;
                        break;
                    case > 99:
                        cfg.MinTimeDelay = 99;
                        break;
                }
            }

            ImGui.SameLine();
            ImGuiComponents.HelpMarker("Hook will NOT be used until the minimum time has passed.\n\nEx: If you set the number as 14 and something bites after 8 seconds, the fish will not to be hooked\n\nSet Zero (0) to disable");


            ImGui.SetNextItemWidth(100 * ImGuiHelpers.GlobalScale);
            if (ImGui.InputDouble("Max. Wait", ref cfg.MaxChumTimeDelay, .1, 1, "%.1f%"))
            {
                switch (cfg.MaxTimeDelay)
                {
                    case 0.1:
                        cfg.MaxTimeDelay = 2;
                        break;
                    case <= 0:
                    case <= 1.9: //This makes the option turn off if delay = 2 seconds when clicking the minus.
                        cfg.MaxTimeDelay = 0;
                        break;
                    case > 99:
                        cfg.MaxTimeDelay = 99;
                        break;
                }
            }
            
            ImGui.SameLine();
            
            ImGuiComponents.HelpMarker("Hook will be used after the defined amount of time has passed\nMin. time: 2s (because of animation lock)\n\nSet Zero (0) to disable, and dont make this lower than the Min. Wait");

        

            ImGui.EndPopup();
        }
    }


    public void DrawEnabledButtonCustomBait(BaitConfig cfg)
    {
        ImGui.Checkbox("Enabled Config ->", ref cfg.Enabled);
        ImGuiComponents.HelpMarker("Important!!!\n\nIf disabled, the fish will NOT be hooked or Mooched.\nTo use the default behavior (General Tab), please delete this configuration.");
    }

    public void DrawCheckBoxDoubleTripleHook(BaitConfig cfg)
    {

        if (ImGui.Button("Double/Triple Hook Settings###DHTH"))
        {
            ImGui.OpenPopup("Double/Triple SettingsHook###DHTH");
        }
        if (ImGui.BeginPopup("Double/Triple SettingsHook###DHTH"))
        {

            ImGui.TextColored(ImGuiColors.DalamudYellow, "Double/Triple Hook Settings");
            ImGui.Spacing();

            ImGui.Checkbox("Only use when Identical Cast is active##surface_slap", ref cfg.UseDHTHOnlySurfaceSlap);
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            if (ImGui.Checkbox("Use Double Hook (If gp > 400)", ref cfg.UseDoubleHook))
            {
                if (cfg.UseDoubleHook && !ImGui.GetIO().KeyShift) cfg.UseTripleHook = false;
                Service.Configuration.Save();
            }
            ImGuiComponents.HelpMarker("Hold SHIFT to select both Double and Triple Hook (not recommended)");

            if (ImGui.Checkbox("Use Triple Hook (If gp > 700)", ref cfg.UseTripleHook))
            {
                if (cfg.UseTripleHook && !ImGui.GetIO().KeyShift) cfg.UseDoubleHook = false;
                Service.Configuration.Save();
            }
            ImGuiComponents.HelpMarker("Hold SHIFT to select both Double and Triple Hook (not recommended)");

            if (cfg.UseTripleHook || cfg.UseDoubleHook)
            {
                ImGui.Indent();

                ImGui.Checkbox("Use when Patience is active (not recommended)", ref cfg.UseDHTHPatience);
                ImGuiComponents.HelpMarker("Important!!!\n\nIf disabled, Precision/Powerful hook will be used instead when Patience is up.");
                ImGui.Checkbox("Let the fish escape if GP is below the required", ref cfg.LetFishEscape);
                ImGui.Unindent();

                ImGui.Separator();
                ImGui.Spacing();

                ImGui.Checkbox(StrHookWeak, ref cfg.HookWeakDHTHEnabled);
                ImGui.Checkbox(StrHookStrong, ref cfg.HookStrongDHTHEnabled);
                ImGui.Checkbox(StrHookLegendary, ref cfg.HookLegendaryDHTHEnabled);
            }

            ImGui.EndPopup();
        }

    }

    public void DrawFishersIntuitionConfig(BaitConfig cfg)
    {
        if (ImGui.Button("Fisher's Intuition Settings###FishersIntuition"))
        {
            ImGui.OpenPopup("fisher_intuition_settings");
        }

        if (ImGui.BeginPopup("fisher_intuition_settings"))
        {
            ImGui.TextColored(ImGuiColors.DalamudYellow, "Fisher's Intuition Settings");
            ImGui.Spacing();
            Utils.DrawUtil.Checkbox("Enable", ref cfg.UseCustomIntuitionHook, "Enable Custom Hooks when Fisher's Intuition is detected");
            ImGui.Separator();

            DrawSelectTugs(StrHookWeak, ref cfg.HookWeakIntuitionEnabled, ref cfg.HookTypeWeakIntuition);
            DrawSelectTugs(StrHookStrong, ref cfg.HookStrongIntuitionEnabled, ref cfg.HookTypeStrongIntuition);
            DrawSelectTugs(StrHookLegendary, ref cfg.HookLegendaryIntuitionEnabled, ref cfg.HookTypeLegendaryIntuition);

            ImGui.EndPopup();
        }
    }

    public void DrawAutoMooch(BaitConfig cfg)
    {

        if (ImGui.Button("Auto Mooch"))
        {
            ImGui.OpenPopup("auto_mooch");
        }

        if (ImGui.BeginPopup("auto_mooch"))
        {
            ImGui.TextColored(ImGuiColors.DalamudYellow, "Auto Mooch");
            ImGui.Spacing();
            ImGui.Text("- If this is a Bait (Ex: Versatile Lure), all fish caught by this bait will be mooched");
            ImGui.Text("- If this is a Fish/Mooch (Ex: Harbor Herring), it'll be mooched when caught");
            ImGui.Text("If this option is disabled, the fish will NOT be mooched even if Auto Mooch is also enabled in the general tab");
            if (Utils.DrawUtil.Checkbox("Auto Mooch", ref cfg.UseAutoMooch, "This option takes priority over the Auto Cast Line"))
            {
                if (!cfg.UseAutoMooch)
                    cfg.UseAutoMooch2 = false;
            }

            if (cfg.UseAutoMooch)
            {
                ImGui.Indent();

                if (ImGui.Checkbox("Use Mooch II", ref cfg.UseAutoMooch2))
                {
                    Service.Configuration.Save();
                }

                if (ImGui.Checkbox("Only use when Fisher's Intution is active##Mooch", ref cfg.OnlyMoochIntuition))
                {
                    Service.Configuration.Save();
                }
                ImGui.Unindent();
            }
            ImGui.EndPopup();
        }
    }

    public void DrawSurfaceSlapIdenticalCast(BaitConfig cfg)
    {

        if (ImGui.Button("Surface Slap & Identical Cast"))
        {
            ImGui.OpenPopup("surface_slap_identical_cast");
        }

        if (ImGui.BeginPopup("surface_slap_identical_cast"))
        {
            ImGui.TextColored(ImGuiColors.DalamudYellow, "Surface Slap & Identical Cast");
            ImGui.Spacing();
            if (DrawUtil.Checkbox("Use Surface Slap", ref cfg.UseSurfaceSlap, "Overrides Identical Cast"))
            {
                cfg.UseIdenticalCast = false;
            }

            if (DrawUtil.Checkbox("Use Identical Cast", ref cfg.UseIdenticalCast, "Overrides Surface Slap"))
            {
                cfg.UseSurfaceSlap = false;
            }

            ImGui.EndPopup();
        }
    }

    public void DrawStopAfter(BaitConfig cfg)
    {

        if (ImGui.Button("Stop fishing after..."))
        {
            ImGui.OpenPopup(str_id: "stop_after");
        }

        if (ImGui.BeginPopup("stop_after"))
        {
            ImGui.TextColored(ImGuiColors.DalamudYellow, "Stop fishing");
            ImGui.Spacing();
            if (DrawUtil.Checkbox("After being caught...", ref cfg.StopAfterCaught, "- If this config is a bait: Stops fishing after X amount of fish is caught\n- If this config is a fish: Stops fishing after it being caught X amount of times"))
            {

            }

            if (cfg.StopAfterCaught)
            {
                ImGui.Indent();
                ImGui.SetNextItemWidth(90 * ImGuiHelpers.GlobalScale);
                if (ImGui.InputInt("Time(s)", ref cfg.StopAfterCaughtLimit))
                {
                    if (cfg.StopAfterCaughtLimit < 1)
                        cfg.StopAfterCaughtLimit = 1;
                }

                ImGui.Unindent();
            }

            ImGui.EndPopup();
        }
    }
}
