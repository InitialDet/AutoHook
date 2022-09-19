using ImGuiNET;
using AutoHook.Utils;
using AutoHook.Configurations;
using AutoHook.Data;
using System.Numerics;
using System.Collections.Generic;
using AutoHook.Classes;
using System;
using AutoHook.Enums;
using Dalamud.Hooking;
using Dalamud.Interface;

namespace AutoHook.Ui;

internal partial class TabAutoCasts
{
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

    private void DrawGPTab()
    {
        foreach (var action in ActionsAbailable)
        {
            var above = action.GPThresholdAbove;

            int gpThreshold = (int) action.GPThreshold;
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
                action.SetThreshold((uint) gpThreshold);

            }

            ImGui.PopID();

            ImGui.Separator();
        }
    }
}
