using System;
using System.Numerics;
using AutoHook.Configurations;
using AutoHook.Enums;
using AutoHook.Resources.Localization;
using AutoHook.Utils;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using ImGuiNET;

namespace AutoHook.Ui;

internal abstract class TabBaseConfig : IDisposable
{
    public abstract string TabName { get; }
    public abstract bool Enabled { get; }
    public static string StrHookWeak => UIStrings.HookWeakExclamation;
    public static string StrHookStrong => UIStrings.HookStrongExclamation;
    public static string StrHookLegendary => UIStrings.HookLegendaryExclamation;

    public abstract void DrawHeader();

    public abstract void Draw();

    public virtual void Dispose()
    {
    }

    public void DrawDeleteBaitButton(BaitConfig cfg)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        if (ImGui.Button($"{FontAwesomeIcon.Trash.ToIconChar()}", new Vector2(ImGui.GetFrameHeight(), 0)) &&
            ImGui.GetIO().KeyShift)
        {
            Service.Configuration.CurrentPreset?.ListOfBaits.RemoveAll(x => x.BaitName == cfg.BaitName);
            Service.Configuration.Save();
        }

        ImGui.PopFont();

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(UIStrings.HoldShiftToDelete);
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
            ImGui.SetTooltip(UIStrings.HookWillBeUsedIfPatienceIsNotUp);

        if (enabled)
        {
            ImGui.Indent();
            if (ImGui.RadioButton($"{UIStrings.PrecisionHookset}###{TabName}{hook}1", type == HookType.Precision))
            {
                type = HookType.Precision;
                Service.Configuration.Save();
            }

            if (ImGui.RadioButton($"{UIStrings.PowerfulHookset}###{TabName}{hook}2", type == HookType.Powerful))
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
        if (ImGui.InputText(UIStrings.MoochBaitName, ref matchText, 64,
                ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
        {
            if (cfg.BaitName != matchText && Service.Configuration.CurrentPreset != null &&
                Service.Configuration.CurrentPreset.ListOfBaits.Contains(new BaitConfig(matchText)))
                cfg.BaitName = UIStrings.BaitAlreadyExists;
            else
                cfg.BaitName = matchText;

            Service.Configuration.Save();
        }

        ;
    }

    public void DrawInputDoubleMaxTime(BaitConfig cfg)
    {
        ImGui.SetNextItemWidth(100 * ImGuiHelpers.GlobalScale);
        if (ImGui.InputDouble(UIStrings.MaxWait, ref cfg.MaxTimeDelay, .1, 1, "%.1f%"))
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
        ImGuiComponents.HelpMarker(UIStrings.HelpMarkerMaxWaitTimer);
    }

    public void DrawInputDoubleMinTime(BaitConfig cfg)
    {
        ImGui.SetNextItemWidth(100 * ImGuiHelpers.GlobalScale);
        if (ImGui.InputDouble(UIStrings.MinWait, ref cfg.MinTimeDelay, .1, 1, "%.1f%"))
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
        ImGuiComponents.HelpMarker(UIStrings.HelpMarkerMinWaitTimer);
    }

    public void DrawChumMinMaxTime(BaitConfig cfg)
    {
        if (ImGui.Button(UIStrings.ChumTimer))
        {
            ImGui.OpenPopup(str_id: "chum_timer");
        }

        if (ImGui.BeginPopup("chum_timer"))
        {
            ImGui.Spacing();
            Utils.DrawUtil.Checkbox(UIStrings.EnableChumTimers, ref cfg.UseChumTimer,
                UIStrings.EnableChumTimersHelpMarker);
            ImGui.Separator();

            ImGui.SetNextItemWidth(100 * ImGuiHelpers.GlobalScale);
            if (ImGui.InputDouble(UIStrings.MinWait, ref cfg.MinChumTimeDelay, .1, 1, "%.1f%"))
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
            ImGuiComponents.HelpMarker(UIStrings.HelpMarkerMinWaitTimer);


            ImGui.SetNextItemWidth(100 * ImGuiHelpers.GlobalScale);
            if (ImGui.InputDouble(UIStrings.MaxWait, ref cfg.MaxChumTimeDelay, .1, 1, "%.1f%"))
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

            ImGuiComponents.HelpMarker(UIStrings.HelpMarkerMaxWaitTimer);

            ImGui.EndPopup();
        }
    }

    public void DrawEnabledButtonCustomBait(BaitConfig cfg)
    {
        ImGui.Checkbox(UIStrings.EnabledConfigArrow, ref cfg.Enabled);
        ImGuiComponents.HelpMarker(UIStrings.EnabledConfigArrowhelpMarker);
    }

    public void DrawCheckBoxDoubleTripleHook(BaitConfig cfg)
    {
        if (ImGui.Button($"{UIStrings.DoubleTripleHookSettings}###DHTH"))
        {
            ImGui.OpenPopup("DHTHPopup###DHTH");
        }

        if (ImGui.BeginPopup("DHTHPopup###DHTH"))
        {
            ImGui.TextColored(ImGuiColors.DalamudYellow, UIStrings.DoubleTripleHookSettings);
            ImGui.Spacing();

            ImGui.Checkbox($"{UIStrings.OnlyUseWhenIdenticalCastIsActive}##surface_slap",
                ref cfg.UseDHTHOnlySurfaceSlap);
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            if (ImGui.Checkbox(UIStrings.UseDoubleHookIfGp400, ref cfg.UseDoubleHook))
            {
                if (cfg.UseDoubleHook && !ImGui.GetIO().KeyShift)
                    cfg.UseTripleHook = false;
                Service.Configuration.Save();
            }

            ImGuiComponents.HelpMarker(UIStrings.HoldShiftBothDHTH);

            if (ImGui.Checkbox(UIStrings.UseTripleHookIfGp700, ref cfg.UseTripleHook))
            {
                if (cfg.UseTripleHook && !ImGui.GetIO().KeyShift)
                    cfg.UseDoubleHook = false;
                Service.Configuration.Save();
            }

            ImGuiComponents.HelpMarker(UIStrings.HoldShiftBothDHTH);

            if (cfg.UseTripleHook || cfg.UseDoubleHook)
            {
                ImGui.Indent();

                ImGui.Checkbox(UIStrings.UseWhenPatienceIsActiveNotRecommended, ref cfg.UseDHTHPatience);
                ImGuiComponents.HelpMarker(UIStrings.DHTHPatienceHelpMarker);
                ImGui.Checkbox(UIStrings.LetTheFishEscape, ref cfg.LetFishEscape);
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
        if (ImGui.Button($"{UIStrings.FisherSIntuitionSettings}###FishersIntuition"))
        {
            ImGui.OpenPopup("fisher_intuition_settings");
        }

        if (ImGui.BeginPopup("fisher_intuition_settings"))
        {
            ImGui.TextColored(ImGuiColors.DalamudYellow, UIStrings.FisherSIntuitionSettings);
            ImGui.Spacing();
            Utils.DrawUtil.Checkbox(UIStrings.Enable, ref cfg.UseCustomIntuitionHook,
                UIStrings.FisherSIntuitionSettingsHelpMarker);
            ImGui.Separator();

            DrawSelectTugs(StrHookWeak, ref cfg.HookWeakIntuitionEnabled, ref cfg.HookTypeWeakIntuition);
            DrawSelectTugs(StrHookStrong, ref cfg.HookStrongIntuitionEnabled, ref cfg.HookTypeStrongIntuition);
            DrawSelectTugs(StrHookLegendary, ref cfg.HookLegendaryIntuitionEnabled, ref cfg.HookTypeLegendaryIntuition);

            ImGui.EndPopup();
        }
    }

    public void DrawAutoMooch(BaitConfig cfg)
    {
        if (ImGui.Button(UIStrings.AutoMooch))
        {
            ImGui.OpenPopup("auto_mooch");
        }

        if (ImGui.BeginPopup("auto_mooch"))
        {
            ImGui.TextColored(ImGuiColors.DalamudYellow, UIStrings.AutoMooch);
            ImGui.Spacing();
            ImGui.Text(UIStrings.AutoMoochPresetDescription);
            if (Utils.DrawUtil.Checkbox(UIStrings.AutoMooch, ref cfg.UseAutoMooch, UIStrings.AutoMoochPresetHelpMarker))
            {
                if (!cfg.UseAutoMooch)
                    cfg.UseAutoMooch2 = false;
            }

            if (cfg.UseAutoMooch)
            {
                ImGui.Indent();

                if (ImGui.Checkbox(UIStrings.UseMoochII, ref cfg.UseAutoMooch2))
                {
                    Service.Configuration.Save();
                }

                if (ImGui.Checkbox($"{UIStrings.OnlyUseWhenFisherSIntutionIsActive}##Mooch",
                        ref cfg.OnlyMoochIntuition))
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
        if (ImGui.Button(UIStrings.SurfaceSlapIdenticalCast))
        {
            ImGui.OpenPopup("surface_slap_identical_cast");
        }

        if (ImGui.BeginPopup("surface_slap_identical_cast"))
        {
            ImGui.TextColored(ImGuiColors.DalamudYellow, UIStrings.SurfaceSlapIdenticalCast);
            ImGui.Spacing();
            if (DrawUtil.Checkbox(UIStrings.UseSurfaceSlap, ref cfg.UseSurfaceSlap, UIStrings.OverridesIdenticalCast))
            {
                cfg.UseIdenticalCast = false;
            }

            if (DrawUtil.Checkbox(UIStrings.UseIdenticalCast, ref cfg.UseIdenticalCast, UIStrings.OverridesSurfaceSlap))
            {
                cfg.UseSurfaceSlap = false;
            }

            ImGui.EndPopup();
        }
    }

    public void DrawStopAfter(BaitConfig cfg)
    {
        if (ImGui.Button(UIStrings.StopFishingAfter))
        {
            ImGui.OpenPopup(str_id: "stop_after");
        }

        if (ImGui.BeginPopup("stop_after"))
        {
            ImGui.TextColored(ImGuiColors.DalamudYellow, UIStrings.StopFishing);
            ImGui.Spacing();
            if (DrawUtil.Checkbox(UIStrings.AfterBeingCaught, ref cfg.StopAfterCaught,
                    UIStrings.AfterBeingCaughtDescription))
            {
            }

            if (cfg.StopAfterCaught)
            {
                ImGui.Indent();
                ImGui.SetNextItemWidth(90 * ImGuiHelpers.GlobalScale);
                if (ImGui.InputInt(UIStrings.TimeS, ref cfg.StopAfterCaughtLimit))
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