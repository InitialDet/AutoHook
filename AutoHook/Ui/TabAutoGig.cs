using AutoHook.Classes;
using AutoHook.Configurations;
using AutoHook.Utils;
using Dalamud.Interface;
using GatherBuddy.Enums;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoHook.Ui;
internal class TabAutoGig : TabBaseConfig
{
    public override string TabName => "AutoGig";
    public override bool Enabled => true;

    private readonly List<SpearfishSpeed> _speedTypes = Enum.GetValues(typeof(SpearfishSpeed)).Cast<SpearfishSpeed>().ToList();
    private readonly List<SpearfishSize> _sizeTypes = Enum.GetValues(typeof(SpearfishSize)).Cast<SpearfishSize>().ToList();

    public override void DrawHeader()
    {
        ImGui.Spacing();
        ImGui.TextWrapped("This is an experimental feature and it might miss the fish. If you find it missing too much, try adjusting the SpearFishing window scale to something different");
        ImGui.Spacing();
    }

    public override void Draw()
    {
        if (DrawUtil.Checkbox("Enable AutoGig", ref Service.Configuration.AutoGigEnabled))
        {
            if (Service.Configuration.AutoGigEnabled)
            {
                Service.Configuration.AutoGigHideOverlay = false;
                Service.Configuration.Save();
            }
        }

        if (!Service.Configuration.AutoGigEnabled)
        {
            ImGui.Indent();
            if (DrawUtil.Checkbox("Hide overlay during Spearfishing", ref Service.Configuration.AutoGigHideOverlay, "It'll only hide if the AutoGig option is disabled"))
            {
                Service.Configuration.Save();
            }

            ImGui.Unindent();
        } else
        {
            ImGui.Indent();
            if (DrawUtil.Checkbox("Draw fish hitbox", ref Service.Configuration.AutoGigDrawFishHitbox, "The hitbox its only available for the fish of the Size and Speed selected"))
            {
                Service.Configuration.Save();
            }
            if (DrawUtil.Checkbox("Draw gig hitbox", ref Service.Configuration.AutoGigDrawGigHitbox))
            {
                Service.Configuration.Save();
            }
            ImGui.Unindent();
        }

        ImGui.Separator();

        DrawSpeedSize();
    }

    private void DrawSpeedSize()
    {
        ImGui.Spacing();
        ImGui.TextWrapped("Select the Size and Speed of the fish you want (Gatherbuddy's Spearfishing overlay helps a lot)");
        ImGui.Spacing();

        ImGui.SetNextItemWidth(130);
        if (ImGui.BeginCombo("Size", Service.Configuration.currentSize.ToName()))
        {

            foreach (SpearfishSize size in _sizeTypes.Where(size =>
                        ImGui.Selectable(size.ToName(), size == Service.Configuration.currentSize)))
            {
                Service.Configuration.currentSize = size;
            }
            ImGui.EndCombo();
        }

        ImGui.SameLine();

        ImGui.SetNextItemWidth(130);
        if (ImGui.BeginCombo("Speed", Service.Configuration.currentSpeed.ToName()))
        {
            foreach (SpearfishSpeed speed in _speedTypes.Where(speed =>
                        ImGui.Selectable(speed.ToName(), speed == Service.Configuration.currentSpeed)))
            {
                Service.Configuration.currentSpeed = speed;
            }
            ImGui.EndCombo();
        }
    }
}
