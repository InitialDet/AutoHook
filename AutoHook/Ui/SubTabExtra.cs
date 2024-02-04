using AutoHook.Configurations;
using AutoHook.Resources.Localization;
using AutoHook.Utils;
using Dalamud.Interface.Colors;
using ImGuiNET;

namespace AutoHook.Ui;

public class SubTabExtra
{
    public bool IsDefaultPreset { get; set; }
    
    public void DrawExtraTab(ExtraConfig config)
    {
        DrawHeader(config);

        if (config.Enabled)
            DrawBody(config);
    }

    public void DrawHeader(ExtraConfig config)
    {
        ImGui.Spacing();
        
        if (DrawUtil.Checkbox(UIStrings.Enable_Extra_Configs, ref config.Enabled))
        {
            Service.Save();
        }

        if (!IsDefaultPreset)
        {
            if (Service.Configuration.HookPresets.DefaultPreset.ExtraCfg.Enabled && !config.Enabled)
                ImGui.TextColored(ImGuiColors.DalamudViolet, UIStrings.Default_Extra_Being_Used);
            else if (!config.Enabled)
                ImGui.TextColored(ImGuiColors.ParsedBlue, UIStrings.SubExtra_Disabled);
        }
        else
        {
            if (Service.Configuration.HookPresets.SelectedPreset?.ExtraCfg.Enabled ?? false)
                ImGui.TextColored(ImGuiColors.DalamudViolet,
                    string.Format(UIStrings.Custom_Extra_Being_Used, Service.Configuration.HookPresets.SelectedPreset.PresetName));
            else if (!config.Enabled)
                ImGui.TextColored(ImGuiColors.ParsedBlue, UIStrings.SubExtra_Disabled);
        }
    }

    public void DrawBody(ExtraConfig config)
    {
        ImGui.BeginGroup();
        ImGui.Spacing();
        if (ImGui.TreeNodeEx(UIStrings.When_gaining_fishers_intuition, ImGuiTreeNodeFlags.FramePadding))
        {
            ImGui.PushID("gaining_intuition");
            ImGui.Spacing();
            DrawSwapPresetIntuitionGain(config);
            DrawSwapBaitIntuitionGain(config);
            ImGui.PopID();
            ImGui.TreePop();
        }
        
        DrawUtil.SpacingSeparator();
        
        if (ImGui.TreeNodeEx(UIStrings.When_losing_fishers_intuition, ImGuiTreeNodeFlags.FramePadding))
        {
            ImGui.PushID("losing_intuition");
            ImGui.Spacing();
            DrawSwapPresetIntuitionLost(config);
            DrawSwapBaitIntuitionLost(config);
            ImGui.PopID();
            ImGui.TreePop();
        }
        
        DrawUtil.SpacingSeparator();
        
        ImGui.EndGroup();

        ImGui.BeginGroup();
        ImGui.Spacing();
        if (ImGui.TreeNodeEx(UIStrings.When_gaining_spectral_current, ImGuiTreeNodeFlags.FramePadding))
        {
            ImGui.PushID("gaining_spectral");
            ImGui.Spacing();
            DrawSwapPresetSpectralGain(config);
            DrawSwapBaitSpectralGain(config);
            ImGui.PopID();
            ImGui.TreePop();
        }

        DrawUtil.SpacingSeparator();

        if (ImGui.TreeNodeEx(UIStrings.When_losing_spectral_current, ImGuiTreeNodeFlags.FramePadding))
        {
            ImGui.PushID("losing_spectral");
            ImGui.Spacing();
            DrawSwapPresetSpectralLost(config);
            DrawSwapBaitSpectralLost(config);
            ImGui.PopID();
            ImGui.TreePop();
        }

        DrawUtil.SpacingSeparator();

        ImGui.EndGroup();
    }

    #region Fishers Intuition
    private void DrawSwapPresetIntuitionGain(ExtraConfig config)
    {
        ImGui.PushID("DrawSwapPresetIntuitionGain");
        DrawUtil.DrawCheckboxTree(UIStrings.Swap_Preset, ref config.SwapPresetIntuitionGain,
            () =>
            {
                DrawUtil.DrawComboSelector(
                    Service.Configuration.HookPresets.CustomPresets,
                    preset => preset.PresetName,
                    config.PresetToSwapIntuitionGain,
                    preset => config.PresetToSwapIntuitionGain = preset.PresetName);
            }
        );
        ImGui.PopID();
    }

    private void DrawSwapBaitIntuitionGain(ExtraConfig config)
    {
        ImGui.PushID("DrawSwapBaitIntuitionGain");
        DrawUtil.DrawCheckboxTree(UIStrings.Swap_Bait, ref config.SwapBaitIntuitionGain,
            () =>
            {
                DrawUtil.DrawComboSelector(
                    PlayerResources.Baits,
                    bait => bait.Name,
                    config.BaitToSwapIntuitionGain.Name,
                    bait => config.BaitToSwapIntuitionGain = bait);
            }
        );

        ImGui.PopID();
    }
    
    private void DrawSwapPresetIntuitionLost(ExtraConfig config)
    {
        ImGui.PushID("DrawSwapPresetIntuitionLost");
        DrawUtil.DrawCheckboxTree(UIStrings.Swap_Preset, ref config.SwapPresetIntuitionLost,
            () =>
            {
                DrawUtil.DrawComboSelector(
                    Service.Configuration.HookPresets.CustomPresets,
                    preset => preset.PresetName,
                    config.PresetToSwapIntuitionLost,
                    preset => config.PresetToSwapIntuitionLost = preset.PresetName);
            }
        );
        ImGui.PopID();
    }

    private void DrawSwapBaitIntuitionLost(ExtraConfig config)
    {
        ImGui.PushID("DrawSwapBaitIntuitionLost");
        DrawUtil.DrawCheckboxTree(UIStrings.Swap_Bait, ref config.SwapBaitIntuitionLost,
            () =>
            {
                DrawUtil.DrawComboSelector(
                    PlayerResources.Baits,
                    bait => bait.Name,
                    config.BaitToSwapIntuitionLost.Name,
                    bait => config.BaitToSwapIntuitionLost = bait);
            }
        );

        ImGui.PopID();
    }
    #endregion

    #region Spectral Current
    private void DrawSwapPresetSpectralGain(ExtraConfig config)
    {
        ImGui.PushID($"{nameof(DrawSwapPresetSpectralGain)}");
        DrawUtil.DrawCheckboxTree(UIStrings.Swap_Preset, ref config.SwapPresetSpectralCurrentGain,
            () =>
            {
                DrawUtil.DrawComboSelector(
                    Service.Configuration.HookPresets.CustomPresets,
                    preset => preset.PresetName,
                    config.PresetToSwapSpectralCurrentGain,
                    preset => config.PresetToSwapSpectralCurrentGain = preset.PresetName);
            }
        );
        ImGui.PopID();
    }

    private void DrawSwapBaitSpectralGain(ExtraConfig config)
    {
        ImGui.PushID($"{nameof(DrawSwapBaitSpectralGain)}");
        DrawUtil.DrawCheckboxTree(UIStrings.Swap_Bait, ref config.SwapBaitSpectralCurrentGain,
            () =>
            {
                DrawUtil.DrawComboSelector(
                    PlayerResources.Baits,
                    bait => bait.Name,
                    config.BaitToSwapSpectralCurrentGain.Name,
                    bait => config.BaitToSwapSpectralCurrentGain = bait);
            }
        );

        ImGui.PopID();
    }

    private void DrawSwapPresetSpectralLost(ExtraConfig config)
    {
        ImGui.PushID($"{nameof(DrawSwapPresetSpectralLost)}");
        DrawUtil.DrawCheckboxTree(UIStrings.Swap_Preset, ref config.SwapPresetSpectralCurrentLost,
            () =>
            {
                DrawUtil.DrawComboSelector(
                    Service.Configuration.HookPresets.CustomPresets,
                    preset => preset.PresetName,
                    config.PresetToSwapSpectralCurrentLost,
                    preset => config.PresetToSwapSpectralCurrentLost = preset.PresetName);
            }
        );
        ImGui.PopID();
    }

    private void DrawSwapBaitSpectralLost(ExtraConfig config)
    {
        ImGui.PushID($"{nameof(DrawSwapBaitSpectralLost)}");
        DrawUtil.DrawCheckboxTree(UIStrings.Swap_Bait, ref config.SwapBaitSpectralCurrentLost,
            () =>
            {
                DrawUtil.DrawComboSelector(
                    PlayerResources.Baits,
                    bait => bait.Name,
                    config.BaitToSwapSpectralCurrentLost.Name,
                    bait => config.BaitToSwapSpectralCurrentLost = bait);
            }
        );

        ImGui.PopID();
    }
    #endregion
}