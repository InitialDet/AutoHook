using ImGuiNET;
using AutoHook.Utils;
using AutoHook.Configurations;
using AutoHook.Data;
using System.Numerics;
using System.Collections.Generic;
using AutoHook.Classes;
using System;

namespace AutoHook.Ui;

internal class TabAutoCasts : TabBaseConfig
{
    public override bool Enabled => true;
    public override string TabName => "自动抛竿";

    private static AutoCastsConfig cfg = Service.Configuration.AutoCastsCfg;

    public override void DrawHeader()
    {
        //ImGui.TextWrapped("The new Auto Cast/Mooch is a experimental feature and can be a little confusing at first. I'll be trying to find a more simple and intuitive solution later\nPlease report any issues you encounter.");

        // Disable all casts
        ImGui.Spacing();
        if (DrawUtil.Checkbox("启用自动抛竿", ref cfg.EnableAll, "反选后下面的技能一个也不会使用"))
        { }

        if (cfg.EnableAll)
        {
            ImGui.SameLine();
            if (DrawUtil.Checkbox("总是以小钓大", ref AutoCastsConfig.DontCancelMooch, "如果是以小钓大的机会并且自动以小钓大启用是，任何会取消以小钓大机会的技能不会被使用。 (例如：撒饵，鱼眼，大鱼猎手等)"))
            { }
        }
        ImGui.Spacing();

    }

    public override void Draw()
    {
        if (cfg.EnableAll)
        {
            DrawAutoCast();
            DrawAutoMooch();
            DrawChum();
            DrawCordials();
            DrawFishEyes();
            DrawMakeShiftBait();
            DrawPatience();
            DrawPrizeCatch();
            DrawThaliaksFavor();
        }
    }

    private void DrawAutoCast()
    {
        if (DrawUtil.Checkbox("全局自动抛竿", ref cfg.EnableAutoCast, "Cast (FSH Action) will be used after a fish bite\n\nIMPORTANT!!!\nIf you have this option enabled and you don't have a Custom Auto Mooch or the Global Auto Mooch option enabled, the line will be casted normally and you'll lose your mooch oportunity (If available)."))
        { }

        if (cfg.EnableAutoCast)
        {
            ImGui.Indent();
            DrawExtraOptionsAutoCast();
            ImGui.Unindent();
        }
    }

    private void DrawExtraOptionsAutoCast()
    {

    }

    private void DrawAutoMooch()
    {
        if (DrawUtil.Checkbox("全局自动以小钓大", ref cfg.EnableMooch, "All fish will be mooched if available. This option have priority over Auto Cast Line\n\nIf you want to Auto Mooch only a especific fish and ignore others, disable this option and add the fish you want in the bait/fish tab"))
        { }

        if (cfg.EnableMooch)
        {
            ImGui.Indent();
            DrawExtraOptionsAutoMooch();
            ImGui.Unindent();
        }
    }

    private void DrawExtraOptionsAutoMooch()
    {
        ImGui.Checkbox("使用以小钓大II", ref cfg.EnableMooch2);
    }

    private void DrawPatience()
    {

        var enabled = cfg.AutoPatienceII.Enabled;
        if (DrawUtil.Checkbox("使用耐心I/II", ref enabled, "Patience I/II will be used when your current GP is equal (or higher) to the action cost +20 (Ex: 220 for I, 580 for II), this helps to avoid not having GP for the hooksets"))
        {
            cfg.AutoPatienceII.Enabled = enabled;
            cfg.AutoPatienceI.Enabled = enabled;
            Service.Configuration.Save();
        }

        if (enabled)
        {
            ImGui.Indent();
            DrawExtraOptionsPatience();
            ImGui.Unindent();
        }
    }

    private void DrawExtraOptionsPatience()
    {

        var enabled = AutoCastsConfig.EnableMakeshiftPatience;
       
        if (DrawUtil.Checkbox("当熟练渔技启用时", ref enabled))
        {
            AutoCastsConfig.EnableMakeshiftPatience = enabled;
            Service.Configuration.Save();
        }

        if (ImGui.RadioButton("耐心I###1", cfg.SelectedPatienceID == IDs.Actions.Patience))
        {
            cfg.SelectedPatienceID = IDs.Actions.Patience;
            Service.Configuration.Save();
        }

        if (ImGui.RadioButton("耐心II###2", cfg.SelectedPatienceID == IDs.Actions.Patience2))
        {
            cfg.SelectedPatienceID = IDs.Actions.Patience2;
            Service.Configuration.Save();
        }
    }

    private void DrawThaliaksFavor()
    {
        ImGui.PushID("ThaliaksFavor");
        var enabled = cfg.AutoThaliaksFavor.Enabled;
        if (DrawUtil.Checkbox("使用沙利亚克的恩宠", ref enabled, "与熟练渔技冲突"))
        {
            cfg.AutoThaliaksFavor.Enabled = enabled;
            Service.Configuration.Save();
        }

        if (enabled)
        {
            ImGui.Indent();
            DrawExtraOptionsThaliaksFavor();
            ImGui.Unindent();
        }
        ImGui.PopID();
    }

    private void DrawExtraOptionsThaliaksFavor()
    {
        var stack = cfg.AutoThaliaksFavor.ThaliaksFavorStacks;
        if (DrawUtil.EditNumberField("当捕鱼人之计层数 =", ref stack))
        {
            if (stack < 3)
                cfg.AutoThaliaksFavor.ThaliaksFavorStacks = 3;
            else if (stack > 10)
                cfg.AutoThaliaksFavor.ThaliaksFavorStacks = 10;
            else
                cfg.AutoThaliaksFavor.ThaliaksFavorStacks = stack;

            Service.Configuration.Save();
        }
    }

    private void DrawMakeShiftBait()
    {
        ImGui.PushID("MakeShiftBait");

        var enabled = cfg.AutoMakeShiftBait.Enabled;
        if (DrawUtil.Checkbox("使用熟练渔技", ref enabled, "与沙利亚克的恩宠冲突"))
        {
            cfg.AutoMakeShiftBait.Enabled = enabled;
            Service.Configuration.Save();
        }

        if (enabled)
        {
            ImGui.Indent();
            DrawExtraOptionsMakeShiftBait();
            ImGui.Unindent();
        }
        ImGui.PopID();
    }

    private void DrawExtraOptionsMakeShiftBait()
    {
        var stack = cfg.AutoMakeShiftBait.MakeshiftBaitStacks;
        if (DrawUtil.EditNumberField($"当捕鱼人之计层数 = ", ref stack))
        {
            if (stack < 5)
                cfg.AutoMakeShiftBait.MakeshiftBaitStacks = 5;
            else if (stack > 10)
                cfg.AutoMakeShiftBait.MakeshiftBaitStacks = 10;
            else
                cfg.AutoMakeShiftBait.MakeshiftBaitStacks = stack;


            Service.Configuration.Save();

        }
    }

    private void DrawPrizeCatch()
    {
        var enabled = cfg.AutoPrizeCatch.Enabled;
        if (DrawUtil.Checkbox("使用大鱼猎手", ref enabled, "Cancels Current Mooch. Patience and Makeshift Bait will not be used when Prize Catch active"))
        {
            cfg.AutoPrizeCatch.Enabled = enabled;
            Service.Configuration.Save();

        }
    }

    private void DrawChum()
    {
        var enabled = cfg.AutoChum.Enabled;
        if (DrawUtil.Checkbox("使用撒饵", ref enabled, "Cancels Current Mooch"))
        {
            cfg.AutoChum.Enabled = enabled;
            Service.Configuration.Save();

        }
    }

    private void DrawFishEyes()
    {
        var enabled = cfg.AutoFishEyes.Enabled;
        if (DrawUtil.Checkbox("使用鱼眼", ref enabled, "Cancels Current Mooch"))
        {
            cfg.AutoFishEyes.Enabled = enabled;
            Service.Configuration.Save();

        }
    }

    private void DrawCordials()
    {

        var enabled = cfg.AutoHICordial.Enabled;
        if (DrawUtil.Checkbox("使用强心剂 (高强优先)", ref enabled, "If theres no Hi-Cordials, Cordials will be used instead"))
        {
            cfg.AutoHICordial.Enabled = enabled;
            cfg.AutoHQCordial.Enabled = enabled;
            cfg.AutoCordial.Enabled = enabled;
        }

        if (enabled)
        {
            ImGui.Indent();
            DrawExtraOptionsCordials();
            ImGui.Unindent();
        }
    }

    private void DrawExtraOptionsCordials()
    {
        if (DrawUtil.Checkbox("修改优先级: 强心剂 > 高级强心剂", ref cfg.EnableCordialFirst, "If theres no Cordials, Hi-Cordials will be used instead"))
        { }
    }
}
