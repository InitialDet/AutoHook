using System;
using System.Numerics;
using AutoHook.Resources.Localization;
using AutoHook.Utils;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;

namespace AutoHook.Classes;

public abstract class BaseActionCast
{
    protected BaseActionCast(string name, uint id, ActionType actionType = ActionType.Action)
    {
        Name = name;
        Id = id;
        Enabled = false;
        
        ActionType = actionType;

        if (actionType == ActionType.Action)
            GpThreshold = (int)PlayerResources.CastActionCost(Id, ActionType);
    }

    public string Name { get; set; }

    public bool Enabled = false;

    public uint Id { get; set; }

    public int GpThreshold = 0;

    public bool GpThresholdAbove { get; set; } = true;

    public bool DoesCancelMooch { get; set; }

    public bool DontCancelMooch  = true;

    public string HelpText = "";

    public ActionType ActionType { get; protected init; }

    public virtual void SetThreshold(int newCost)
    {
        var actionCost = (int) PlayerResources.CastActionCost(Id, ActionType);

        GpThreshold = (newCost < 0) ? 0 : Math.Max(newCost, actionCost);

        Service.Save();
    }

    public bool IsAvailableToCast()
    {
        if (!Enabled)
            return false;

        if (DoesCancelMooch && PlayerResources.IsMoochAvailable() && DontCancelMooch)
            return false;
        
        var condition = CastCondition();
        
        var currentGp = PlayerResources.GetCurrentGp();

        bool hasGp;

        if (GpThresholdAbove)
            hasGp = currentGp >= GpThreshold;
        else
            hasGp = currentGp <= GpThreshold;

        var actionAvailable = PlayerResources.ActionTypeAvailable(Id, ActionType);
        
        return hasGp && actionAvailable && condition;
    }

    public abstract bool CastCondition();

    public virtual string GetName() => "";

    protected delegate void DrawOptionsDelegate();

    protected virtual DrawOptionsDelegate? DrawOptions => null;

    public virtual void DrawConfig()
    {
        ImGui.PushID($"{GetName()}_cfg");
        
        if (DrawOptions != null)
        {
            if (DrawUtil.Checkbox("", ref Enabled, HelpText, true))
                Service.Save();

            ImGui.SameLine();
            
            if (ImGui.TreeNodeEx($"{GetName()}",  ImGuiTreeNodeFlags.FramePadding))
            {
                ImGui.SameLine();
                DrawGpThreshold();
                DrawOptions?.Invoke();
                ImGui.Separator();
                ImGui.TreePop();
            }
            else
            {
                ImGui.SameLine();
                DrawGpThreshold();
            }
        }
        else
        {
            if (DrawUtil.Checkbox(GetName(), ref Enabled, HelpText, true))
                Service.Save();
            
            ImGui.SameLine();
            DrawGpThreshold();
        }
        ImGui.PopID();
    }

    public virtual void DrawGpThreshold()
    {
        ImGui.PushID($"{GetName()}_gp");
        if (ImGui.Button("GP"))
        {
            ImGui.OpenPopup(str_id: @"gp_cfg");
        }

        if (ImGui.BeginPopup(@"gp_cfg"))
        {
            if (ImGui.BeginChild("gp_cfg2", new Vector2(175, 125), true))
            {
                if (ImGui.Button(" X "))
                    ImGui.CloseCurrentPopup();
                ImGui.SameLine();
                ImGui.TextColored(ImGuiColors.DalamudYellow, $"GP - {GetName()}");
                
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip(
                        @$"{GetName()} {UIStrings.WillBeUsedWhenYourGPIsEqualOr} {(GpThresholdAbove ? UIStrings.Above : UIStrings.Below)} {GpThreshold}");
                
                ImGui.Separator();
                if (ImGui.RadioButton(UIStrings.Above, GpThresholdAbove))
                {
                    GpThresholdAbove = true;
                    Service.Save();
                }

                //ImGui.SameLine();

                if (ImGui.RadioButton(UIStrings.Below, GpThresholdAbove == false))
                {
                    GpThresholdAbove = false;
                    Service.Save();
                }

                //ImGui.SameLine();

                ImGui.SetNextItemWidth(100 * ImGuiHelpers.GlobalScale);
                if (ImGui.InputInt(UIStrings.GP, ref GpThreshold, 1, 1))
                {
                    GpThreshold = Math.Max(GpThreshold, 0);
                    SetThreshold(GpThreshold);
                    Service.Save();
                }
                
                // add a button to close the pop up
               
                ImGui.EndChild();
            }
            
            ImGui.EndPopup();
        }
        
        ImGui.PopID();
    }
}