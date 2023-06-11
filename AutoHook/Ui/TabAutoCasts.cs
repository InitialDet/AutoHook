using ImGuiNET;
using AutoHook.Utils;
using AutoHook.Configurations;
using AutoHook.Data;
using System.Numerics;
using System.Collections.Generic;
using AutoHook.Classes;
using System;
using System.Diagnostics;
using ECommons.LanguageHelpers;

namespace AutoHook.Ui;

internal class TabAutoCasts : TabBaseConfig
{
    public override bool Enabled => true;
    public override string TabName => "Auto Casts".Loc();

    private readonly static AutoCastsConfig cfg = Service.Configuration.AutoCastsCfg;
    public override void DrawHeader()
    {
        //ImGui.TextWrapped("The new Auto Cast/Mooch is a experimental feature and can be a little confusing at first. I'll be trying to find a more simple and intuitive solution later\nPlease report any issues you encounter.");

        // Disable all casts
        ImGui.Spacing();
        if (DrawUtil.Checkbox("Enable Auto Casts".Loc(), ref cfg.EnableAll))
        {
            Service.Configuration.Save();
        }

        if (cfg.EnableAll)
        {
            ImGui.SameLine();
            if (DrawUtil.Checkbox("Don't Cancel Mooch".Loc(), ref cfg.DontCancelMooch, "Actions that cancel mooch wont be used (e.g. Chum, Fish Eyes, Prize Catch etc.)"))
            {
                Service.Configuration.Save();
            }
        }

        ImGui.Spacing();

        if (ImGui.Button("Guide: How to auto accept Collectables".Loc()))
        {
            Process.Start(new ProcessStartInfo { FileName = "https://github.com/InitialDet/AutoHook/blob/main/AcceptCollectable.md", UseShellExecute = true });
        }
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
        if (DrawUtil.Checkbox("Global Auto Cast Line".Loc(), ref cfg.EnableAutoCast, "Cast (FSH Action) will be used after a fish bite\n\nIMPORTANT!!!\nIf you have this option enabled and you don't have a Custom Auto Mooch or the Global Auto Mooch option enabled, the line will be casted normally and you'll lose your mooch oportunity (If available).".Loc()))
        {
            Service.Configuration.Save();
        }

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
        if (DrawUtil.Checkbox("Global Auto Mooch".Loc(), ref cfg.EnableMooch, "This option have priority over Auto Cast Line\n\nIf you want to Auto Mooch only a especific fish and ignore others, disable this option and add Custom Preset.".Loc()))
        {
            Service.Configuration.Save();
        }

        if (cfg.EnableMooch)
        {
            ImGui.Indent();
            DrawExtraOptionsAutoMooch();
            ImGui.Unindent();
        }
    }

    private void DrawExtraOptionsAutoMooch()
    {
        if (ImGui.Checkbox("Use Mooch II".Loc(), ref cfg.EnableMooch2))
        {
            Service.Configuration.Save();
        }

        if (ImGui.Checkbox("Only use when Fisher's Intution is active##fi_mooch".Loc(), ref cfg.OnlyMoochIntuition))
        {
            Service.Configuration.Save();
        }
    }

    private void DrawPatience()
    {

        var enabled = cfg.AutoPatienceII.Enabled;
        if (DrawUtil.Checkbox("Use Patience I/II".Loc(), ref enabled, "Patience I/II will be used when your current GP is equal (or higher) to the action cost +20 (Ex: 220 for I, 580 for II), this helps to avoid not having GP for the hooksets".Loc()))
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

        var enabled = cfg.EnableMakeshiftPatience;

        if (DrawUtil.Checkbox("Use when Makeshift Bait is active##patience_makeshift".Loc(), ref enabled))
        {
            cfg.EnableMakeshiftPatience = enabled;
            Service.Configuration.Save();
        }

        if (ImGui.RadioButton("Patience I###1".Loc(), cfg.SelectedPatienceID == IDs.Actions.Patience))
        {
            cfg.SelectedPatienceID = IDs.Actions.Patience;
            Service.Configuration.Save();
        }

        if (ImGui.RadioButton("Patience II###2".Loc(), cfg.SelectedPatienceID == IDs.Actions.Patience2))
        {
            cfg.SelectedPatienceID = IDs.Actions.Patience2;
            Service.Configuration.Save();
        }
    }

    private void DrawThaliaksFavor()
    {
        ImGui.PushID("ThaliaksFavor");
        var enabled = cfg.AutoThaliaksFavor.Enabled;
        if (DrawUtil.Checkbox("Use Thaliak's Favor".Loc(), ref enabled, "This might conflict with Auto MakeShift Bait".Loc()))
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
        if (DrawUtil.EditNumberField("When Stacks = ".Loc(), ref stack))
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
        if (DrawUtil.Checkbox("Use Makeshift Bait".Loc(), ref enabled, "This might conflict with Auto Thaliak's Favor".Loc()))
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
        if (DrawUtil.EditNumberField("When Stacks =  ".Loc(), ref stack))
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
        if (DrawUtil.Checkbox("Use Prize Catch".Loc(), ref enabled, "Cancels Current Mooch. Patience and Makeshift Bait will not be used when Prize Catch active".Loc()))
        {
            cfg.AutoPrizeCatch.Enabled = enabled;
            Service.Configuration.Save();
        }

        if (enabled)
        {
            ImGui.Indent();
            DrawExtraOptionPrizeCatch();
            ImGui.Unindent();
        }
    }

    private void DrawExtraOptionPrizeCatch()
    {
        if (DrawUtil.Checkbox("Only use when Mooch II is on NOT available - READ >>>".Loc(), ref cfg.AutoPrizeCatch.UseWhenMoochIIOnCD, ">Make sure 'Use Mooch II' is enabled or else it wont work<\nThis could save you 100gp if going only for mooches".Loc()))
        { }

        if (DrawUtil.Checkbox("Only use when Identical Cast is active##ic_prize_catch".Loc(), ref cfg.AutoPrizeCatch.UseOnlyWithIdenticalCast))
        { }
    }

    private void DrawChum()
    {
        var enabled = cfg.AutoChum.Enabled;
        if (DrawUtil.Checkbox("Use Chum".Loc(), ref enabled, "Cancels Current Mooch".Loc()))
        {
            cfg.AutoChum.Enabled = enabled;
            Service.Configuration.Save();
        }

        if (enabled)
        {
            ImGui.Indent();
            DrawExtraOptionsChum();
            ImGui.Unindent();
        }
    }

    private void DrawExtraOptionsChum()
    {
        if (DrawUtil.Checkbox("Only use when Fisher's Intution is active##fi_chum".Loc(), ref cfg.AutoChum.OnlyUseWithIntuition))
        { }
    }

    private void DrawFishEyes()
    {
        var enabled = cfg.AutoFishEyes.Enabled;
        if (DrawUtil.Checkbox("Use Fish Eyes".Loc(), ref enabled, "Cancels Current Mooch".Loc()))
        {
            cfg.AutoFishEyes.Enabled = enabled;
            Service.Configuration.Save();

        }
    }

    private void DrawCordials()
    {

        var enabled = cfg.AutoHICordial.Enabled;
        if (DrawUtil.Checkbox("Use Cordials (Hi-Cordial First)".Loc(), ref enabled, "If theres no Hi-Cordials, Cordials will be used instead".Loc()))
        {
            cfg.AutoHICordial.Enabled = enabled;
            cfg.AutoHQCordial.Enabled = enabled;
            cfg.AutoCordial.Enabled = enabled;
            cfg.AutoHQWateredCordial.Enabled = enabled;
            cfg.AutoWateredCordial.Enabled = enabled;
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
        if (DrawUtil.Checkbox("Change Priority: Watered-Cordial > Cordial > HI-Cordials".Loc(), ref cfg.EnableCordialFirst, "If theres no Cordials, Hi-Cordials will be used instead".Loc()))
        { }
    }
}
