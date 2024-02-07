using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AutoHook.Classes;
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

public class SubTabBaitMooch
{
    public bool IsDefault { get; set; } = false;
    public bool IsMooch { get; set; } = false;

    private static string StrHookWeak => UIStrings.HookWeakExclamation;
    private static string StrHookStrong => UIStrings.HookStrongExclamation;
    private static string StrHookLegendary => UIStrings.HookLegendaryExclamation;
    private static string StrHookWhenActiveSlap => UIStrings.OnlyUseWhenActiveSurfaceSlap;

    //private PresetConfig _selectedPreset;
    private List<HookConfig> _listOfHooks = new();

    public void DrawHookTab(PresetConfig presetCfg)
    {
        if (IsMooch)
            _listOfHooks = presetCfg.ListOfMooch;
        else
            _listOfHooks = presetCfg.ListOfBaits;

        if (!IsDefault)
        {
            ImGui.Spacing();
            DrawDescription();
            ImGui.Separator();
        }

        ImGui.BeginGroup();

        for (int idx = 0; idx < _listOfHooks?.Count; idx++)
        {
            var bait = _listOfHooks[idx];
            ImGui.PushID($"id###{idx}");
            ImGui.Spacing();
            
            var count = HookingManager.FishingCounter.GetCount(bait.GetUniqueId());
            var hookCounter = count > 0 ? $"({UIStrings.Hooked_Counter} {count})" : "";
            if (ImGui.CollapsingHeader($"{bait.BaitFish.Name} {hookCounter}###{idx}"))
            {
                DrawEnabledButtonCustomBait(bait);
                ImGui.Indent();

                DrawDeleteButton(bait);
                DrawInputSearchBar(bait);
                ImGui.Spacing();
                DrawHookCheckboxes(bait);
                ImGui.Spacing();
                DrawInputDoubleMinTime(bait);
                ImGui.Spacing();
                DrawInputDoubleMaxTime(bait);
                ImGui.Spacing();
                DrawChumMinMaxTime(bait);
                ImGui.Spacing();

                DrawFishersIntuitionConfig(bait);
                ImGui.Spacing();
                DrawCheckBoxDoubleTripleHook(bait);
                ImGui.Spacing();
                DrawStopAfter(bait);

                ImGui.Unindent();
            }

            ImGui.Spacing();
            ImGui.PopID();
        }

        ImGui.EndGroup();
    }

    private void DrawDescription()
    {
        if (ImGui.Button(UIStrings.Add))
        {
            if (_listOfHooks.All(x => x.BaitFish.Id != -1))
            {
                _listOfHooks.Add(new HookConfig(new BaitFishClass()));
                Service.Save();
            }
        }

        var bait = IsMooch ? UIStrings.Add_new_mooch : UIStrings.Add_new_bait;

        ImGui.SameLine();
        ImGui.Text($"{bait} ({_listOfHooks.Count})");
        ImGui.SameLine();
        ImGuiComponents.HelpMarker(UIStrings.TabPresets_DrawHeader_CorrectlyEditTheBaitMoochName);
        ImGui.Spacing();
    }

    private void DrawDeleteButton(HookConfig hookConfig)
    {
        if (IsDefault)
            return;
        ImGui.SameLine();
        ImGui.PushFont(UiBuilder.IconFont);
        if (ImGui.Button($"{FontAwesomeIcon.Trash.ToIconChar()}", new Vector2(ImGui.GetFrameHeight(), 0)) &&
            ImGui.GetIO().KeyShift)
        {
            _listOfHooks.RemoveAll(x => x.BaitFish.Id == hookConfig.BaitFish.Id);
            Service.Save();
        }

        ImGui.PopFont();

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(UIStrings.HoldShiftToDelete);
    }

    private void DrawHookCheckboxes(HookConfig hookConfig)
    {
        DrawSelectTugs(StrHookWeak, ref hookConfig.HookWeakEnabled, ref hookConfig.HookTypeWeak, ref hookConfig.HookWeakOnlyWhenActiveSlap, ref hookConfig.HookWeakOnlyWhenNOTActiveSlap);
        DrawSelectTugs(StrHookStrong, ref hookConfig.HookStrongEnabled, ref hookConfig.HookTypeStrong, ref hookConfig.HookStrongOnlyWhenActiveSlap, ref hookConfig.HookStrongOnlyWhenNOTActiveSlap);
        DrawSelectTugs(StrHookLegendary, ref hookConfig.HookLegendaryEnabled, ref hookConfig.HookTypeLegendary, ref hookConfig.HookLegendaryOnlyWhenActiveSlap, ref hookConfig.HookLegendaryOnlyWhenNOTActiveSlap);
    }

    private void DrawSelectTugs(string hook, ref bool enabled, ref HookType type, ref bool hookOnlyWhenActiveSlap, ref bool hookOnlyWhenNOTActiveSlap)
    {
        ImGui.PushID($"{hook}");
        if(ImGui.Checkbox($"", ref enabled))
        {
            Service.Save();
        }
        
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(UIStrings.HookWillBeUsedIfPatienceIsNotUp);

        ImGui.SameLine();
        if (ImGui.TreeNode($"{hook}"))
        {
            if (ImGui.RadioButton(UIStrings.Normal_Hook, type == HookType.Normal))
            {
                type = HookType.Normal;
                Service.Save();
            }

            if (ImGui.RadioButton(UIStrings.PrecisionHookset, type == HookType.Precision))
            {
                type = HookType.Precision;
                Service.Save();
            }

            if (ImGui.RadioButton(UIStrings.PowerfulHookset, type == HookType.Powerful))
            {
                type = HookType.Powerful;
                Service.Save();
            }

            ImGui.Spacing();
            
            if (ImGui.TreeNodeEx(UIStrings.Surface_Slap_Options, ImGuiTreeNodeFlags.FramePadding))
            {
                if (DrawUtil.Checkbox(UIStrings.OnlyUseWhenActiveSurfaceSlap, ref hookOnlyWhenActiveSlap))
                {
                    hookOnlyWhenNOTActiveSlap = false;
                    Service.Save();
                }

                if (DrawUtil.Checkbox(UIStrings.OnlyUseWhenNOTActiveSurfaceSlap, ref hookOnlyWhenNOTActiveSlap))
                {
                    hookOnlyWhenActiveSlap = false;
                    Service.Save();
                }
            
                ImGui.TreePop();
            }
            
            ImGui.TreePop();
        }

        ImGui.PopID();
    }

    private void DrawInputSearchBar(HookConfig hookConfig)
    {
        if (IsDefault)
            return;

        var list = IsMooch ? PlayerResources.Fishes : PlayerResources.Baits;

        DrawUtil.DrawComboSelector(
            list,
            (BaitFishClass item) => item.Name,
            hookConfig.BaitFish.Name,
            (BaitFishClass item) => hookConfig.BaitFish = item);
    }

    private void DrawInputDoubleMaxTime(HookConfig hookConfig)
    {
        ImGui.SetNextItemWidth(100 * ImGuiHelpers.GlobalScale);
        if (ImGui.InputDouble(UIStrings.MaxWait, ref hookConfig.MaxTimeDelay, .1, 1, "%.1f%"))
        {
            hookConfig.MaxTimeDelay = hookConfig.MaxTimeDelay switch
            {
                0.1 => 2,
                <= 0 or <= 1.9 => 0,
                > 99 => 99,
                _ => hookConfig.MaxTimeDelay
            };
            Service.Save();
        }

        ImGui.SameLine();
        ImGuiComponents.HelpMarker(UIStrings.HelpMarkerMaxWaitTimer);
    }

    private void DrawInputDoubleMinTime(HookConfig hookConfig)
    {
        ImGui.SetNextItemWidth(100 * ImGuiHelpers.GlobalScale);
        if (ImGui.InputDouble(UIStrings.MinWait, ref hookConfig.MinTimeDelay, .1, 1, "%.1f%"))
        {
            switch (hookConfig.MinTimeDelay)
            {
                case <= 0:
                    hookConfig.MinTimeDelay = 0;
                    break;
                case > 99:
                    hookConfig.MinTimeDelay = 99;
                    break;
            }
            Service.Save();
        }

        ImGui.SameLine();
        ImGuiComponents.HelpMarker(UIStrings.HelpMarkerMinWaitTimer);
    }

    private void DrawChumMinMaxTime(HookConfig hookConfig)
    {
        DrawUtil.DrawCheckboxTree(UIStrings.EnableChumTimers, ref hookConfig.UseChumTimer,
            () =>
            {
                ImGui.SetNextItemWidth(100 * ImGuiHelpers.GlobalScale);
                if (ImGui.InputDouble(UIStrings.MinWait, ref hookConfig.MinChumTimeDelay, .1, 1, "%.1f%"))
                {
                    switch (hookConfig.MinChumTimeDelay)
                    {
                        case <= 0:
                            hookConfig.MinChumTimeDelay = 0;
                            break;
                        case > 99:
                            hookConfig.MinChumTimeDelay = 99;
                            break;
                    }
                    Service.Save();
                }

                ImGui.SameLine();
                ImGuiComponents.HelpMarker(UIStrings.HelpMarkerMinWaitTimer);

                ImGui.SetNextItemWidth(100 * ImGuiHelpers.GlobalScale);
                if (ImGui.InputDouble(UIStrings.MaxWait, ref hookConfig.MaxChumTimeDelay, .1, 1, "%.1f%"))
                {
                    switch (hookConfig.MaxChumTimeDelay)
                    {
                        case 0.1:
                            hookConfig.MaxChumTimeDelay = 2;
                            break;
                        case <= 0:
                        case <= 1.9: //This makes the option turn off if delay = 2 seconds when clicking the minus.
                            hookConfig.MaxChumTimeDelay = 0;
                            break;
                        case > 99:
                            hookConfig.MaxChumTimeDelay = 99;
                            break;
                    }
                    Service.Save();
                }

                ImGui.SameLine();

                ImGuiComponents.HelpMarker(UIStrings.HelpMarkerMaxWaitTimer);
            }
        , UIStrings.EnableChumTimersHelpMarker);
        
    }

    private void DrawEnabledButtonCustomBait(HookConfig hookConfig)
    {
        if (ImGui.Checkbox(UIStrings.Enabled, ref hookConfig.Enabled))
        {
            Service.Save();
        }

        ImGuiComponents.HelpMarker(UIStrings.EnabledConfigArrowhelpMarker);
    }

    private void DrawCheckBoxDoubleTripleHook(HookConfig hookConfig)
    {
        if (ImGui.Button($"{UIStrings.DoubleTripleHookSettings}###DHTH"))
        {
            ImGui.OpenPopup("DHTHPopup###DHTH");
        }

        if (ImGui.BeginPopup("DHTHPopup###DHTH"))
        {
            ImGui.TextColored(ImGuiColors.DalamudYellow, UIStrings.DoubleTripleHookSettings);
            ImGui.Spacing();

            if (ImGui.Checkbox($"{UIStrings.OnlyUseWhenIdenticalCastIsActive}##identical_cast", ref hookConfig.UseDHTHOnlyIdenticalCast))
            {
                Service.Save();
            }

            if (ImGui.Checkbox($"{UIStrings.OnlyUseWhenActiveSurfaceSlap}##surface_slap", ref hookConfig.UseDHTHOnlySurfaceSlap))
            {
                Service.Save();
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            if (ImGui.Checkbox(UIStrings.UseDoubleHookIfGp400, ref hookConfig.UseDoubleHook))
            {
                if (hookConfig.UseDoubleHook && !ImGui.GetIO().KeyShift)
                    hookConfig.UseTripleHook = false;
                Service.Save();
            }

            ImGuiComponents.HelpMarker(UIStrings.HoldShiftBothDHTH);

            if (ImGui.Checkbox(UIStrings.UseTripleHookIfGp700, ref hookConfig.UseTripleHook))
            {
                if (hookConfig.UseTripleHook && !ImGui.GetIO().KeyShift)
                    hookConfig.UseDoubleHook = false;
                Service.Save();
            }

            ImGuiComponents.HelpMarker(UIStrings.HoldShiftBothDHTH);

            if (hookConfig.UseTripleHook || hookConfig.UseDoubleHook)
            {
                ImGui.Indent();

                if (ImGui.Checkbox(UIStrings.UseWhenPatienceIsActiveNotRecommended, ref hookConfig.UseDHTHPatience))
                {
                    Service.Save();
                }

                ImGuiComponents.HelpMarker(UIStrings.DHTHPatienceHelpMarker);
                if (ImGui.Checkbox(UIStrings.LetTheFishEscape, ref hookConfig.LetFishEscape))
                {
                    Service.Save();
                }

                ImGui.Unindent();

                ImGui.Separator();
                ImGui.Spacing();

                if (ImGui.Checkbox(StrHookWeak, ref hookConfig.HookWeakDHTHEnabled))
                {
                    Service.Save();
                }

                if (ImGui.Checkbox(StrHookStrong, ref hookConfig.HookStrongDHTHEnabled))
                {
                    Service.Save();
                }

                if (ImGui.Checkbox(StrHookLegendary, ref hookConfig.HookLegendaryDHTHEnabled))
                {
                    Service.Save();
                }
            }

            ImGui.EndPopup();
        }
    }

    public void DrawFishersIntuitionConfig(HookConfig cfg)
    {
        if (ImGui.Button(@$"{UIStrings.FisherSIntuitionSettings}###FishersIntuition"))
        {
            ImGui.OpenPopup(@"fisher_intuition_settings");
        }

        if (ImGui.BeginPopup(@"fisher_intuition_settings"))
        {
            ImGui.TextColored(ImGuiColors.DalamudYellow, UIStrings.FisherSIntuitionSettings);
            ImGui.Spacing();
            if (DrawUtil.Checkbox(UIStrings.Enable, ref cfg.UseCustomIntuitionHook,
                    UIStrings.FisherSIntuitionSettingsHelpMarker))
            {
                Service.Save();
            }

            ImGui.Separator();

            DrawSelectTugs(StrHookWeak, ref cfg.HookWeakIntuitionEnabled, ref cfg.HookTypeWeakIntuition, ref cfg.HookWeakOnlyWhenActiveSlap, ref cfg.HookWeakOnlyWhenNOTActiveSlap);
            DrawSelectTugs(StrHookStrong, ref cfg.HookStrongIntuitionEnabled, ref cfg.HookTypeStrongIntuition, ref cfg.HookStrongOnlyWhenActiveSlap, ref cfg.HookStrongOnlyWhenNOTActiveSlap);
            DrawSelectTugs(StrHookLegendary, ref cfg.HookLegendaryIntuitionEnabled, ref cfg.HookTypeLegendaryIntuition, ref cfg.HookLegendaryOnlyWhenActiveSlap, ref cfg.HookLegendaryOnlyWhenNOTActiveSlap);

            ImGui.EndPopup();
        }
    }

    private void DrawStopAfter(HookConfig hookConfig)
    {
        if (ImGui.Button(UIStrings.StopFishingAfter))
        {
            ImGui.OpenPopup(str_id: @"stop_after");
        }

        if (ImGui.BeginPopup(@"stop_after"))
        {
            ImGui.TextColored(ImGuiColors.DalamudYellow, UIStrings.StopFishing);
            ImGui.Spacing();
            if (DrawUtil.Checkbox(UIStrings.StopFishing_After_hooking, ref hookConfig.StopAfterCaught))
            {
                Service.Save();
            }

            if (hookConfig.StopAfterCaught)
            {
                ImGui.Indent();
                ImGui.SetNextItemWidth(90 * ImGuiHelpers.GlobalScale);
                if (ImGui.InputInt(UIStrings.TimeS, ref hookConfig.StopAfterCaughtLimit))
                {
                    if (hookConfig.StopAfterCaughtLimit < 1)
                        hookConfig.StopAfterCaughtLimit = 1;

                    Service.Save();
                }

                ImGui.Unindent();
            }
            
            if (ImGui.RadioButton(UIStrings.Stop_Casting, hookConfig.StopFishingStep == FishingSteps.None))
            {
                hookConfig.StopFishingStep = FishingSteps.None;
                Service.Save();
            }
            
            ImGui.SameLine();
            ImGuiComponents.HelpMarker(UIStrings.Auto_Cast_Stopped);
            
            if (ImGui.RadioButton(UIStrings.Quit_Fishing, hookConfig.StopFishingStep == FishingSteps.Quitting))
            {
                hookConfig.StopFishingStep = FishingSteps.Quitting;
                Service.Save();
            }
            ImGui.SameLine();
            ImGuiComponents.HelpMarker(UIStrings.Quit_Action_HelpText);

            ImGui.EndPopup();
        }
    }
}