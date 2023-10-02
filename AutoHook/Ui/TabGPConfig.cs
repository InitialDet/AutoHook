using AutoHook.Classes;
using AutoHook.Configurations;
using AutoHook.Resources.Localization;
using Dalamud.Interface;
using ImGuiNET;
using System.Collections.Generic;

namespace AutoHook.Ui;
internal class TabGPConfig : TabBaseConfig
{
    public override string TabName => UIStrings.TabNameGPConfig;
    public override bool Enabled => true;

    private static readonly AutoCastsConfig cfg = Service.Configuration.AutoCastsCfg;

    private static readonly List<BaseActionCast> _actionsAvailable = new()
            {
                cfg.AutoChum,
                cfg.AutoCordial,
                cfg.AutoHICordial,
                cfg.AutoHQCordial,
                cfg.AutoWateredCordial,
                cfg.AutoHQWateredCordial,
                cfg.AutoFishEyes,
                cfg.AutoIdenticalCast,
                cfg.AutoMakeShiftBait,
                cfg.AutoPatienceI,
                cfg.AutoPatienceII,
                cfg.AutoPrizeCatch,
                cfg.AutoSurfaceSlap,
                cfg.AutoThaliaksFavor,
            };

    public override void Draw()
    {
        DrawGPTab();
    }

    public override void DrawHeader()
    {
        ImGui.Spacing();
        ImGui.TextWrapped(UIStrings.TabGPConfig_TabDescription);
        ImGui.Spacing();
    }

    private void DrawGPTab()
    {
        foreach (var action in _actionsAvailable)
        {
            bool isAbove = action.GPThresholdAbove;
            int gpThreshold = (int)action.GPThreshold;

            ImGui.PushID(action.Name);
            ImGui.SetWindowFontScale(1.2f);
            ImGui.Text(action.Name);
            ImGui.SetWindowFontScale(1f);

            if (ImGui.IsItemHovered())
                ImGui.SetTooltip($"{action.Name} {UIStrings.WillBeUsedWhenYourGPIsEqualOr} {(isAbove ? UIStrings.Above : UIStrings.Below)} {gpThreshold}");

            if (ImGui.RadioButton($"{UIStrings.Above}##1", isAbove == true))
            {
                action.GPThresholdAbove = true;
                Service.Configuration.Save();
            }

            ImGui.SameLine();

            if (ImGui.RadioButton($"{UIStrings.Below}##1", isAbove == false))
            {
                action.GPThresholdAbove = false;
                Service.Configuration.Save();
            }

            ImGui.SameLine();

            ImGui.SetNextItemWidth(100 * ImGuiHelpers.GlobalScale);
            if (ImGui.InputInt(UIStrings.GP, ref gpThreshold, 1, 1))
            {
                action.SetThreshold((uint)gpThreshold);
            }

            ImGui.PopID();

            ImGui.Separator();
        }
    }
}
