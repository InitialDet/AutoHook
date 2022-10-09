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
    public override string TabName => "自动刺鱼";
    public override bool Enabled => true;

    List<SpearfishSpeed> speedTypes = Enum.GetValues(typeof(SpearfishSpeed)).Cast<SpearfishSpeed>().ToList();
    List<SpearfishSize> sizeTypes = Enum.GetValues(typeof(SpearfishSize)).Cast<SpearfishSize>().ToList();

    public override void DrawHeader()
    {
        ImGui.Spacing();
        ImGui.TextWrapped("实验性功能，可能会错过一些鱼，如果你错过了太多鱼，可能需要调节不同的窗口尺寸");
        ImGui.Spacing();
    }

    public override void Draw()
    {
        if (DrawUtil.Checkbox("启用自动刺鱼", ref Service.Configuration.AutoGigEnabled))
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
            if (DrawUtil.Checkbox("刺鱼时显示覆盖层", ref Service.Configuration.AutoGigHideOverlay, "仅当关闭自动刺鱼时显示"))
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
        ImGui.TextWrapped("选择你需要鱼的尺寸和速度 (Gatherbuddy的刺鱼悬浮窗非常有用)");
        ImGui.Spacing();

        ImGui.SetNextItemWidth(130);
        if (ImGui.BeginCombo("尺寸", Service.Configuration.currentSize.ToName()))
        {

            foreach (SpearfishSize size in sizeTypes.Where(size =>
                        ImGui.Selectable(size.ToName(), size == Service.Configuration.currentSize)))
            {
                Service.Configuration.currentSize = size;
            }
            ImGui.EndCombo();
        }

        ImGui.SameLine();

        ImGui.SetNextItemWidth(130);
        if (ImGui.BeginCombo("速度", Service.Configuration.currentSpeed.ToName()))
        {
            foreach (SpearfishSpeed speed in speedTypes.Where(speed =>
                        ImGui.Selectable(speed.ToName(), speed == Service.Configuration.currentSpeed)))
            {
                Service.Configuration.currentSpeed = speed;
            }
            ImGui.EndCombo();
        }
    }

}
