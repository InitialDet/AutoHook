using AutoHook.Classes;
using AutoHook.Configurations;
using Dalamud.Interface;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoHook.Ui;
internal class TabGPConfig : TabBaseConfig
{
    public override string TabName => "GP Config";
    public override bool Enabled => true;

    private static AutoCastsConfig cfg = Service.Configuration.AutoCastsCfg;

    static List<BaseActionCast> ActionsAbailable = new()
            {
                cfg.AutoChum,
                cfg.AutoCordial,
                cfg.AutoHICordial,
                cfg.AutoHQCordial,
                cfg.AutoFishEyes,
                cfg.AutoPatienceI,
                cfg.AutoPatienceII,
                cfg.AutoPrizeCatch,
            };

    public override void Draw()
    {
        DrawGPTab();
    }

    public override void DrawHeader()
    {
       
    }

    private void DrawGPTab()
    {
        foreach (var action in ActionsAbailable)
        {
            var above = action.GPThresholdAbove;
            int gpThreshold = (int)action.GPThreshold;

            ImGui.PushID(action.Name);
            ImGui.SetWindowFontScale(1.2f);
            ImGui.Text(action.Name);
            ImGui.SetWindowFontScale(1f);

            if (ImGui.IsItemHovered())
                ImGui.SetTooltip($"{action.Name} will be used when your GP is equal or {(above ? "above" : "below")} {gpThreshold}");

            if (ImGui.RadioButton($"Above##1", above == true))
            {
                action.GPThresholdAbove = true;
                Service.Configuration.Save();
            }

            ImGui.SameLine();

            if (ImGui.RadioButton($"Below##1", above == false))
            {
                action.GPThresholdAbove = false;
                Service.Configuration.Save();
            }

            ImGui.SameLine();

            ImGui.SetNextItemWidth(100 * ImGuiHelpers.GlobalScale);
            if (ImGui.InputInt("GP", ref gpThreshold, 1, 1))
            {
                action.SetThreshold((uint)gpThreshold);
            }

            ImGui.PopID();

            ImGui.Separator();
        }
    }
}
