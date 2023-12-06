using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AutoHook.Classes;
using AutoHook.Configurations;
using AutoHook.Enums;
using AutoHook.Resources.Localization;
using AutoHook.Utils;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using ImGuiNET;

namespace AutoHook.Ui;

public class SubTabFish
{
    private List<FishConfig> _listOfFish = new();

    public void DrawFishTab(PresetConfig presetCfg)
    {
        _listOfFish = presetCfg.ListOfFish;

        if (_listOfFish == null)
            return;

        ImGui.Spacing();
        DrawDescription(_listOfFish);
        ImGui.Spacing();

        ImGui.Separator();

        ImGui.BeginGroup();
        for (var idx = 0; idx < _listOfFish.Count; idx++)
        {
            var fish = _listOfFish[idx];
            ImGui.PushID($"fishTab###{idx}");

            var count = HookingManager.FishingCounter.GetCount(fish.GetUniqueId());
            var fishCount = count > 0 ? $"({UIStrings.Caught_Counter} {count})" : "";
            if (ImGui.CollapsingHeader($"{fish.Fish.Name} {fishCount}###a{idx}"))
            {
                ImGui.Spacing();
                ImGui.Checkbox(UIStrings.Enable, ref fish.Enabled);
                DrawDeleteButton(fish);
                ImGui.Spacing();
                ImGui.Indent();

                DrawFishSearchBar(fish);
                DrawUtil.SpacingSeparator();

                DrawSurfaceSlapIdenticalCast(fish);
                ImGui.Spacing();

                DrawMooch(fish);
                ImGui.Spacing();

                DrawSwapBait(fish);
                ImGui.Spacing();

                DrawSwapPreset(fish);
                ImGui.Spacing();

                DrawStopAfter(fish);
                ImGui.Spacing();
                
                /*DrawNeverRelease(fish);
                ImGui.Spacing();*/

                ImGui.Unindent();
            }

            ImGui.Spacing();
            ImGui.PopID();
        }

        ImGui.EndGroup();
    }

    private void DrawDescription(List<FishConfig> list)
    {
        if (ImGui.Button(UIStrings.Add))
        {
            if (list.All(x => x.Fish.Id != -1))
            {
                list.Add(new FishConfig(new BaitFishClass()));
                Service.Save();
            }
        }

        ImGui.SameLine();
        ImGui.Text($"{UIStrings.Add_new_fish} ({list.Count})");
        ImGui.SameLine();
        
        ImGui.SameLine();

        if (ImGui.Button($"{UIStrings.AddLastCatch} {Service.LastCatch.Name ?? "-"}"))
        {
            if (Service.LastCatch.Id == 0 || Service.LastCatch.Id == -1)
                return;
            if (list.Any(x => x.Fish.Id == Service.LastCatch.Id))
                return;
            
            list.Add(new FishConfig(Service.LastCatch));
            Service.Save();
        }
    }

    private void DrawDeleteButton(FishConfig fishConfig)
    {
        if (IsDefault)
            return;
        ImGui.SameLine();
        ImGui.PushFont(UiBuilder.IconFont);
        if (ImGui.Button($"{FontAwesomeIcon.Trash.ToIconChar()}", new Vector2(ImGui.GetFrameHeight(), 0)) &&
            ImGui.GetIO().KeyShift)
        {
            _listOfFish.RemoveAll(x => x.Fish.Id == fishConfig.Fish.Id);
            Service.Save();
        }

        ImGui.PopFont();

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(UIStrings.HoldShiftToDelete);
    }

    private void DrawFishSearchBar(FishConfig fishConfig)
    {
        ImGui.PushID("DrawFishSearchBar");

        ImGui.Spacing();
        DrawUtil.DrawComboSelector<BaitFishClass>(
            PlayerResources.Fishes,
            (BaitFishClass fish) => fish.Name,
            fishConfig.Fish.Name,
            (BaitFishClass fish) => fishConfig.Fish = fish);

        ImGui.PopID();
    }

    private void DrawSurfaceSlapIdenticalCast(FishConfig fishConfig)
    {
        ImGui.PushID($"{UIStrings.SurfaceSlapIdenticalCast}");

        if (ImGui.TreeNodeEx(UIStrings.SurfaceSlapIdenticalCast, ImGuiTreeNodeFlags.FramePadding))
        {
            fishConfig.SurfaceSlap.DrawConfig();
            
            fishConfig.IdenticalCast.DrawConfig();
            
            ImGui.TreePop();
        }

        ImGui.PopID();
    }

    private void DrawMooch(FishConfig fishConfig)
    {
        ImGui.PushID(@"DrawMooch");
        if (ImGui.TreeNodeEx(UIStrings.Mooch_Setting, ImGuiTreeNodeFlags.FramePadding))
        {
            fishConfig.Mooch.DrawConfig();

            if (DrawUtil.Checkbox(UIStrings.Never_Mooch, ref fishConfig.NeverMooch))
            {
                fishConfig.Mooch.Enabled = false;
                Service.Save();
            }

            ImGui.TreePop();
        }

        ImGui.PopID();
    }

    private void DrawSwapBait(FishConfig fishConfig)
    {
        ImGui.PushID("DrawSwapBait");
        DrawUtil.DrawCheckboxTree(UIStrings.Swap_Bait, ref fishConfig.SwapBait,
            () =>
            {
                DrawUtil.DrawComboSelector(
                    PlayerResources.Baits,
                    bait => bait.Name,
                    fishConfig.BaitToSwap.Name,
                    bait => fishConfig.BaitToSwap = bait);

                ImGui.Spacing();

                DrawUtil.DrawWordWrappedString(UIStrings.AfterBeingCaught);

                ImGui.SameLine();
                ImGui.SetNextItemWidth(90 * ImGuiHelpers.GlobalScale);
                if (ImGui.InputInt(UIStrings.TimeS, ref fishConfig.SwapBaitCount))
                {
                    if (fishConfig.SwapBaitCount < 1)
                        fishConfig.SwapBaitCount = 1;

                    Service.Save();
                }
            }
        );

        ImGui.PopID();
    }

    private void DrawSwapPreset(FishConfig fishConfig)
    {
        ImGui.PushID("DrawSwapPreset");

        DrawUtil.DrawCheckboxTree(UIStrings.Swap_Preset, ref fishConfig.SwapPresets,
            () =>
            {
                DrawUtil.DrawComboSelector(
                    Service.Configuration.HookPresets.CustomPresets,
                    preset => preset.PresetName,
                    fishConfig.PresetToSwap,
                    preset => fishConfig.PresetToSwap = preset.PresetName);

                ImGui.Spacing();

                DrawUtil.DrawWordWrappedString(UIStrings.AfterBeingCaught);

                ImGui.SameLine();
                ImGui.SetNextItemWidth(90 * ImGuiHelpers.GlobalScale);
                if (ImGui.InputInt(UIStrings.TimeS, ref fishConfig.SwapPresetCount))
                {
                    if (fishConfig.SwapPresetCount < 1)
                        fishConfig.SwapPresetCount = 1;

                    Service.Save();
                }
            }
        );

        ImGui.PopID();
    }

    private void DrawStopAfter(FishConfig fishConfig)
    {
        ImGui.PushID("DrawStopAfter");
        if (DrawUtil.Checkbox("", ref fishConfig.StopAfterCaught))
            Service.Save();

        ImGui.SameLine();
        if (ImGui.TreeNodeEx(UIStrings.Stop_After_Caught, ImGuiTreeNodeFlags.FramePadding))
        {
            ImGui.Indent();
            ImGui.SetNextItemWidth(90 * ImGuiHelpers.GlobalScale);
            if (ImGui.InputInt(UIStrings.TimeS, ref fishConfig.StopAfterCaughtLimit))
            {
                if (fishConfig.StopAfterCaughtLimit < 1)
                    fishConfig.StopAfterCaughtLimit = 1;

                Service.Save();
            }
            
            if (ImGui.RadioButton(UIStrings.Stop_Casting, fishConfig.StopFishingStep == FishingSteps.None))
            {
                fishConfig.StopFishingStep = FishingSteps.None;
                Service.Save();
            }
            
            ImGui.SameLine();
            ImGuiComponents.HelpMarker(UIStrings.Auto_Cast_Stopped);
            
            if (ImGui.RadioButton(UIStrings.Quit_Fishing, fishConfig.StopFishingStep == FishingSteps.Quitting))
            {
                fishConfig.StopFishingStep = FishingSteps.Quitting;
                Service.Save();
            }
            
            ImGui.SameLine();
            ImGuiComponents.HelpMarker(UIStrings.Quit_Action_HelpText);

            ImGui.Unindent();
            ImGui.Separator();
            ImGui.TreePop();
        }

        ImGui.PopID();
    }

    private void DrawNeverRelease(FishConfig fishConfig)
    {
        ImGui.PushID("DrawNeverRelease");
        
        if (DrawUtil.Checkbox(UIStrings.NeverRelease, ref fishConfig.NeverRelease, UIStrings.NeverReleaseHelptext))
        {
            Service.Save();
        }
         
        ImGui.PopID();
    }
    
    public bool IsDefault { get; set; }
}