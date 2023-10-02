using AutoHook.Classes;
using AutoHook.Configurations;
using AutoHook.Resources.Localization;
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
    public override string TabName => UIStrings.TabNameAutoGig;
    public override bool Enabled => true;

    private readonly List<SpearfishSpeed> _speedTypes = Enum.GetValues(typeof(SpearfishSpeed)).Cast<SpearfishSpeed>().ToList();
    private readonly List<SpearfishSize> _sizeTypes = Enum.GetValues(typeof(SpearfishSize)).Cast<SpearfishSize>().ToList();

    public override void DrawHeader()
    {
        ImGui.Spacing();
        ImGui.TextWrapped(UIStrings.TabAutoGigDescription);
        ImGui.Spacing();
    }

    public override void Draw()
    {
        if (DrawUtil.Checkbox(UIStrings.EnableAutoGig, ref Service.Configuration.AutoGigEnabled))
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
            if (DrawUtil.Checkbox(UIStrings.HideOverlayDuringSpearfishing, ref Service.Configuration.AutoGigHideOverlay, UIStrings.AutoGigHideOverlayHelpMarker))
            {
                Service.Configuration.Save();
            }

            ImGui.Unindent();
        } else
        {
            ImGui.Indent();
            if (DrawUtil.Checkbox(UIStrings.DrawFishHitbox, ref Service.Configuration.AutoGigDrawFishHitbox, UIStrings.DrawFishHitboxHelpMarker))
            {
                Service.Configuration.Save();
            }
            if (DrawUtil.Checkbox(UIStrings.DrawGigHitbox, ref Service.Configuration.AutoGigDrawGigHitbox))
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
        ImGui.TextWrapped(UIStrings.SelectTheSizeAndSpeed);
        ImGui.Spacing();

        ImGui.SetNextItemWidth(130);
        if (ImGui.BeginCombo(UIStrings.Size, Service.Configuration.currentSize.ToName()))
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
        if (ImGui.BeginCombo(UIStrings.Speed, Service.Configuration.currentSpeed.ToName()))
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
