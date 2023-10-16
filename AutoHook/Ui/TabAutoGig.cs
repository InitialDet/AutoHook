using System;
using System.Collections.Generic;
using System.Linq;
using AutoHook.Resources.Localization;
using AutoHook.Spearfishing.Enums;
using AutoHook.Utils;
using ImGuiNET;

namespace AutoHook.Ui;
internal class TabAutoGig : BaseTab
{
    public override string TabName => UIStrings.TabNameAutoGig;
    public override bool Enabled => true;

    private readonly List<SpearfishSpeed> _speedTypes = Enum.GetValues(typeof(SpearfishSpeed)).Cast<SpearfishSpeed>().ToList();
    private readonly List<SpearfishSize> _sizeTypes = Enum.GetValues(typeof(SpearfishSize)).Cast<SpearfishSize>().ToList();
    
    private bool _showDescription;
    public override void DrawHeader()
    {
        
        ImGui.Spacing();
        
        if (ImGui.TreeNodeEx(UIStrings.Tab_Description, ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.FramePadding))
        {
            _showDescription = true;
            ImGui.TreePop();
        }
        else
            _showDescription = false;

        // Ugly implementation, but it looks good enough for now.
        if (_showDescription)
        {
            ImGui.TextWrapped(UIStrings.TabAutoGigDescription);
        }
        ImGui.Spacing();
    }

    public override void Draw()
    {
        if (DrawUtil.Checkbox(UIStrings.EnableAutoGig, ref Service.Configuration.AutoGigEnabled))
        {
            if (Service.Configuration.AutoGigEnabled)
            {
                Service.Configuration.AutoGigHideOverlay = false;
                Service.Save();
            }
        }

        if (!Service.Configuration.AutoGigEnabled)
        {
            ImGui.Indent();
            if (DrawUtil.Checkbox(UIStrings.HideOverlayDuringSpearfishing, ref Service.Configuration.AutoGigHideOverlay, UIStrings.AutoGigHideOverlayHelpMarker))
            {
                Service.Save();
            }

            ImGui.Unindent();
        } else
        {
            ImGui.Indent();
            if (DrawUtil.Checkbox(UIStrings.DrawFishHitbox, ref Service.Configuration.AutoGigDrawFishHitbox, UIStrings.DrawFishHitboxHelpMarker))
            {
                Service.Save();
            }
            if (DrawUtil.Checkbox(UIStrings.DrawGigHitbox, ref Service.Configuration.AutoGigDrawGigHitbox))
            {
                Service.Save();
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
        if (ImGui.BeginCombo(UIStrings.Size, Service.Configuration.CurrentSize.ToName()))
        {

            foreach (SpearfishSize size in _sizeTypes.Where(size =>
                        ImGui.Selectable(size.ToName(), size == Service.Configuration.CurrentSize)))
            {
                Service.Configuration.CurrentSize = size;
            }
            ImGui.EndCombo();
        }

        ImGui.SameLine();

        ImGui.SetNextItemWidth(130);
        if (ImGui.BeginCombo(UIStrings.Speed, Service.Configuration.CurrentSpeed.ToName()))
        {
            foreach (SpearfishSpeed speed in _speedTypes.Where(speed =>
                        ImGui.Selectable(speed.ToName(), speed == Service.Configuration.CurrentSpeed)))
            {
                Service.Configuration.CurrentSpeed = speed;
            }
            ImGui.EndCombo();
        }
    }
}
