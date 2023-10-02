using ImGuiNET;
using AutoHook.Utils;
using AutoHook.Configurations;
using AutoHook.Data;
using System.Diagnostics;
using AutoHook.Resources.Localization;

namespace AutoHook.Ui;

internal class TabAutoCasts : TabBaseConfig
{
    public override bool Enabled => true;
    public override string TabName => UIStrings.Auto_Casts;

    private static readonly AutoCastsConfig Cfg = Service.Configuration.AutoCastsCfg;
    public override void DrawHeader()
    {
        //ImGui.TextWrapped("The new Auto Cast/Mooch is a experimental feature and can be a little confusing at first. I'll be trying to find a more simple and intuitive solution later\nPlease report any issues you encounter.");

        // Disable all casts
        ImGui.Spacing();
        if (DrawUtil.Checkbox(UIStrings.Enable_Auto_Casts, ref Cfg.EnableAll))
        {
            Service.Configuration.Save();
        }

        if (Cfg.EnableAll)
        {
            ImGui.SameLine();
            if (DrawUtil.Checkbox(UIStrings.Dont_Cancel_Mooch, ref Cfg.DontCancelMooch, UIStrings.TabAutoCasts_DrawHeader_HelpText))
            {
                Service.Configuration.Save();
            }
        }

        ImGui.Spacing();

        if (ImGui.Button(UIStrings.TabAutoCasts_DrawHeader_Guide_Collectables))
        {
            Process.Start(new ProcessStartInfo { FileName = "https://github.com/InitialDet/AutoHook/blob/main/AcceptCollectable.md", UseShellExecute = true });
        }
    }

    public override void Draw()
    {
        if (Cfg.EnableAll)
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
        if (DrawUtil.Checkbox(UIStrings.Global_Auto_Cast_Line, ref Cfg.EnableAutoCast, UIStrings.TabAutoCasts_DrawAutoCast_HelpText))
        {
            Service.Configuration.Save();
        }

        if (Cfg.EnableAutoCast)
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
        if (DrawUtil.Checkbox(UIStrings.Global_Auto_Mooch, ref Cfg.EnableMooch, UIStrings.TabAutoCasts_DrawAutoMooch_HelpText))
        {
            Service.Configuration.Save();
        }

        if (Cfg.EnableMooch)
        {
            ImGui.Indent();
            DrawExtraOptionsAutoMooch();
            ImGui.Unindent();
        }
    }

    private void DrawExtraOptionsAutoMooch()
    {
        if (ImGui.Checkbox(UIStrings.Use_Mooch_II, ref Cfg.EnableMooch2))
        {
            Service.Configuration.Save();
        }

        if (ImGui.Checkbox($"{UIStrings.TabAutoCasts_DrawExtraOptionsAutoMooch_Extra_Only_Active}##fi_mooch", ref Cfg.OnlyMoochIntuition))
        {
            Service.Configuration.Save();
        }
    }

    private void DrawPatience()
    {

        var enabled = Cfg.AutoPatienceII.Enabled;
        if (DrawUtil.Checkbox(UIStrings.Use_Patience_I_II, ref enabled, UIStrings.TabAutoCasts_DrawPatience_HelpText))
        {
            Cfg.AutoPatienceII.Enabled = enabled;
            Cfg.AutoPatienceI.Enabled = enabled;
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

        var enabled = Cfg.EnableMakeshiftPatience;

        if (DrawUtil.Checkbox($"{UIStrings.TabAutoCasts_DrawExtraOptionsPatience}##patience_makeshift", ref enabled))
        {
            Cfg.EnableMakeshiftPatience = enabled;
            Service.Configuration.Save();
        }

        if (ImGui.RadioButton($"{UIStrings.Patience_I}###1", Cfg.SelectedPatienceID == IDs.Actions.Patience))
        {
            Cfg.SelectedPatienceID = IDs.Actions.Patience;
            Service.Configuration.Save();
        }

        if (ImGui.RadioButton($"{$"{UIStrings.Patience_II}###2"}", Cfg.SelectedPatienceID == IDs.Actions.Patience2))
        {
            Cfg.SelectedPatienceID = IDs.Actions.Patience2;
            Service.Configuration.Save();
        }
    }

    private void DrawThaliaksFavor()
    {
        ImGui.PushID("ThaliaksFavor");
        var enabled = Cfg.AutoThaliaksFavor.Enabled;
        if (DrawUtil.Checkbox(UIStrings.Use_Thaliaks_Favor, ref enabled, UIStrings.TabAutoCasts_DrawThaliaksFavor_HelpText))
        {
            Cfg.AutoThaliaksFavor.Enabled = enabled;
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
        var stack = Cfg.AutoThaliaksFavor.ThaliaksFavorStacks;
        if (DrawUtil.EditNumberField(UIStrings.TabAutoCasts_DrawExtraOptionsThaliaksFavor_, ref stack))
        {
            if (stack < 3)
                Cfg.AutoThaliaksFavor.ThaliaksFavorStacks = 3;
            else if (stack > 10)
                Cfg.AutoThaliaksFavor.ThaliaksFavorStacks = 10;
            else
                Cfg.AutoThaliaksFavor.ThaliaksFavorStacks = stack;

            Service.Configuration.Save();
        }
    }

    private void DrawMakeShiftBait()
    {
        ImGui.PushID("MakeShiftBait");

        var enabled = Cfg.AutoMakeShiftBait.Enabled;
        if (DrawUtil.Checkbox(UIStrings.Use_Makeshift_Bait, ref enabled, UIStrings.TabAutoCasts_DrawMakeShiftBait_HelpText))
        {
            Cfg.AutoMakeShiftBait.Enabled = enabled;
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
        var stack = Cfg.AutoMakeShiftBait.MakeshiftBaitStacks;
        if (DrawUtil.EditNumberField(UIStrings.TabAutoCasts_When_Stack_Equals, ref stack))
        {
            if (stack < 5)
                Cfg.AutoMakeShiftBait.MakeshiftBaitStacks = 5;
            else if (stack > 10)
                Cfg.AutoMakeShiftBait.MakeshiftBaitStacks = 10;
            else
                Cfg.AutoMakeShiftBait.MakeshiftBaitStacks = stack;

            Service.Configuration.Save();
        }
    }

    private void DrawPrizeCatch()
    {
        var enabled = Cfg.AutoPrizeCatch.Enabled;
        if (DrawUtil.Checkbox(UIStrings.Use_Prize_Catch, ref enabled, UIStrings.Use_Prize_Catch_HelpText))
        {
            Cfg.AutoPrizeCatch.Enabled = enabled;
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
        if (DrawUtil.Checkbox(UIStrings.AutoCastExtraOptionPrizeCatch, ref Cfg.AutoPrizeCatch.UseWhenMoochIIOnCD, UIStrings.ExtraOptionPrizeCatchHelpMarker))
        { }

        if (DrawUtil.Checkbox($"{UIStrings.OnlyUseWhenIdenticalCastIsActive}##ic_prize_catch", ref Cfg.AutoPrizeCatch.UseOnlyWithIdenticalCast))
        { }
    }

    private void DrawChum()
    {
        var enabled = Cfg.AutoChum.Enabled;
        if (DrawUtil.Checkbox(UIStrings.AutoCastUseChum, ref enabled, UIStrings.CancelsCurrentMooch))
        {
            Cfg.AutoChum.Enabled = enabled;
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
        if (DrawUtil.Checkbox($"{UIStrings.OnlyUseWhenFisherSIntutionIsActive}##fi_chum", ref Cfg.AutoChum.OnlyUseWithIntuition))
        { }
    }

    private void DrawFishEyes()
    {
        var enabled = Cfg.AutoFishEyes.Enabled;
        if (DrawUtil.Checkbox(UIStrings.AutoCastUseFishEyes, ref enabled, UIStrings.CancelsCurrentMooch))
        {
            Cfg.AutoFishEyes.Enabled = enabled;
            Service.Configuration.Save();

        }
    }

    private void DrawCordials()
    {
        var enabled = Cfg.AutoHICordial.Enabled;
        if (DrawUtil.Checkbox(UIStrings.AutoCastUseCordial, ref enabled, UIStrings.AutoCastUseCordialHelpMarker))
        {
            Cfg.AutoHICordial.Enabled = enabled;
            Cfg.AutoHQCordial.Enabled = enabled;
            Cfg.AutoCordial.Enabled = enabled;
            Cfg.AutoHQWateredCordial.Enabled = enabled;
            Cfg.AutoWateredCordial.Enabled = enabled;
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
        if (DrawUtil.Checkbox(UIStrings.AutoCastCordialPriority, ref Cfg.EnableCordialFirst, UIStrings.AutoCastCordialPriorityHelpMarker))
        { }
    }
}
