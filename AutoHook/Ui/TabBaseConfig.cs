using System;
using System.Numerics;
using AutoHook.Configurations;
using AutoHook.Enums;
using AutoHook.Utils;
using Dalamud.Hooking;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using ImGuiNET;

namespace AutoHook.Ui;
abstract class TabBaseConfig : IDisposable
{
    public abstract string TabName { get; }
    public abstract bool Enabled { get; }

    public static string StrHookWeak => "咬钩轻杆 (!)";
    public static string StrHookStrong => "咬钩中杆 (!!)";
    public static string StrHookLegendary => "咬钩重杆 (!!!)";

    public abstract void DrawHeader();

    public abstract void Draw();

    public virtual void Dispose() { }

    public void DrawDeleteBaitButton(HookConfig cfg)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        if (ImGui.Button($"{FontAwesomeIcon.Trash.ToIconChar()}", new Vector2(ImGui.GetFrameHeight(), 0)) && ImGui.GetIO().KeyShift)
        {
            Service.Configuration.CustomBait.RemoveAll(x => x.BaitName == cfg.BaitName);
            Service.Configuration.Save();
        }
        ImGui.PopFont();

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("按住 SHIFT 删除。");
    }

    public void DrawHookCheckboxes(HookConfig cfg)
    {
        DrawSelectTugs(StrHookWeak, ref cfg.HookWeakEnabled, ref cfg.HookTypeWeak);
        DrawSelectTugs(StrHookStrong, ref cfg.HookStrongEnabled, ref cfg.HookTypeStrong);
        DrawSelectTugs(StrHookLegendary, ref cfg.HookLegendaryEnabled, ref cfg.HookTypeLegendary);
    }

    public void DrawSelectTugs(string hook, ref bool enabled, ref HookType type)
    {
       
        ImGui.Checkbox(hook, ref enabled);
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("未开启耐心时会直接使用 \"提钩\"");

        if (enabled)
        {
            ImGui.Indent();
            if (ImGui.RadioButton($"精准提钩###{TabName}{hook}1", type == HookType.Precision))
            {
                type = HookType.Precision;
                Service.Configuration.Save();
            }

            if (ImGui.RadioButton($"强力提钩###{TabName}{hook}2", type == HookType.Powerful))
            {
                type = HookType.Powerful;
                Service.Configuration.Save();
            }
            ImGui.Unindent();
        }
    }

    public void DrawInputTextName(HookConfig cfg)
    {
        string matchText = new string(cfg.BaitName);
        ImGui.SetNextItemWidth(-260 * ImGuiHelpers.GlobalScale);
        if (ImGui.InputText("以小钓大/鱼饵 名称", ref matchText, 64, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
        {
            if (cfg.BaitName != matchText && Service.Configuration.CustomBait.Contains(new HookConfig(matchText)))
                cfg.BaitName = "鱼饵已存在";
            else
                cfg.BaitName = matchText;

            Service.Configuration.Save();
        };
    }

    public void DrawInputDoubleMaxTime(HookConfig cfg)
    {
        ImGui.SetNextItemWidth(100 * ImGuiHelpers.GlobalScale);
        if (ImGui.InputDouble("最长等待", ref cfg.MaxTimeDelay, .1, 1, "%.1f%"))
        {
            switch (cfg.MaxTimeDelay)
            {
                case 0.1:
                    cfg.MaxTimeDelay = 2;
                    break;
                case <= 0:
                case <= 1.9: //This makes the option turn off if delay is 2 seconds when clicking the minus.
                    cfg.MaxTimeDelay = 0;
                    break;
                case > 99:
                    cfg.MaxTimeDelay = 99;
                    break;
            }
        }
        ImGui.SameLine();
        ImGuiComponents.HelpMarker("Hook will be used after the defined amount of time has passed\nMin. time: 2s (because of animation lock)\n\nSet Zero (0) to disable, and dont make this lower than the Min. Wait");
    }

    public void DrawInputDoubleMinTime(HookConfig cfg)
    {
        ImGui.SetNextItemWidth(100 * ImGuiHelpers.GlobalScale);
        if (ImGui.InputDouble("最长等待", ref cfg.MinTimeDelay, .1, 1, "%.1f%"))
        {
            switch (cfg.MinTimeDelay)
            {
                case <= 0:
                    cfg.MinTimeDelay = 0;
                    break;
                case > 99:
                    cfg.MinTimeDelay = 99;
                    break;
            }
        }

        ImGui.SameLine();
        ImGuiComponents.HelpMarker("Hook will NOT be used until the minimum time has passed.\n\nEx: If you set the number as 14 and something bites after 8 seconds, the fish will not to be hooked\n\nSet Zero (0) to disable");
    }

    public void DrawEnabledButtonCustomBait(HookConfig cfg)
    {
        ImGui.Checkbox("启用设置 ->", ref cfg.Enabled);
        ImGuiComponents.HelpMarker("Important!!!\n\nIf disabled, the fish will NOT be hooked or Mooched.\nTo use the default behavior (General Tab), please delete this configuration.");
    }

    public void DrawCheckBoxDoubleTripleHook(HookConfig cfg)
    {

        if (ImGui.Button("多重提钩 设置###DHTH"))
        {
            ImGui.OpenPopup("多重提钩###DHTH");
        }
        if (ImGui.BeginPopup("多重提钩###DHTH"))
        {

            ImGui.TextColored(ImGuiColors.DalamudYellow, "多重提钩设置");
            ImGui.Spacing();

            ImGui.Checkbox("仅在专一垂钓时启用", ref cfg.UseDHTHOnlySurfaceSlap);
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            if (ImGui.Checkbox("使用双提 (当 gp > 400)", ref cfg.UseDoubleHook))
            {
                if (cfg.UseDoubleHook) cfg.UseTripleHook = false;
                Service.Configuration.Save();
            }
            if (ImGui.Checkbox("使用双提 (当 gp > 700)", ref cfg.UseTripleHook))
            {
                if (cfg.UseTripleHook) cfg.UseDoubleHook = false;
                Service.Configuration.Save();
            }

            if (cfg.UseTripleHook || cfg.UseDoubleHook)
            {
                ImGui.Indent();

                ImGui.Checkbox("启用耐心时也使用 (不推荐)", ref cfg.UseDHTHPatience);
                ImGuiComponents.HelpMarker("注意!!!\n\n禁用时，耐心中在使用精准提钩和强力提钩。");
                ImGui.Checkbox("当 GP 低于要求时把鱼放跑", ref cfg.LetFishEscape);
                ImGui.Unindent();

                ImGui.Separator();
                ImGui.Spacing();

                ImGui.Checkbox(StrHookWeak, ref cfg.HookWeakDHTHEnabled);
                ImGui.Checkbox(StrHookStrong, ref cfg.HookStrongDHTHEnabled);
                ImGui.Checkbox(StrHookLegendary, ref cfg.HookLegendaryDHTHEnabled);
            }

            ImGui.EndPopup();
        }

    }

    public void DrawFishersIntuitionConfig(HookConfig cfg)
    {
        if (ImGui.Button("捕鱼人之识 设置###FishersIntuition"))
        {
            ImGui.OpenPopup("fisher_intuition_settings");
        }

        if (ImGui.BeginPopup("fisher_intuition_settings"))
        {
            ImGui.TextColored(ImGuiColors.DalamudYellow, "捕鱼人之识");
            ImGui.Spacing();
            Utils.DrawUtil.Checkbox("Enable", ref cfg.UseCustomIntuitionHook, "当检测到捕鱼人之识时启用特殊提钩");
            ImGui.Separator();

            DrawSelectTugs(StrHookWeak, ref cfg.HookWeakIntuitionEnabled, ref cfg.HookTypeWeakIntuition);
            DrawSelectTugs(StrHookStrong, ref cfg.HookStrongIntuitionEnabled, ref cfg.HookTypeStrongIntuition);
            DrawSelectTugs(StrHookLegendary, ref cfg.HookLegendaryIntuitionEnabled, ref cfg.HookTypeLegendaryIntuition);

            ImGui.EndPopup();
        }
    }

    public void DrawAutoMooch(HookConfig cfg)
    {

        if (ImGui.Button("自动以小钓大"))
        {
            ImGui.OpenPopup("auto_mooch");
        }

        if (ImGui.BeginPopup("auto_mooch"))
        {
            ImGui.TextColored(ImGuiColors.DalamudYellow, "自动以小钓大");
            ImGui.Spacing();
            ImGui.Text("- If this is a Bait, all fish caught by this bait will be mooched");
            ImGui.Text("- If this is a Fish/Mooch (Ex: Harbor Herring), it'll be mooched when caught");
            ImGui.Text("If this option is disabled, it will NOT be mooched even if Auto Mooch is also enabled in the general tab");
            if (Utils.DrawUtil.Checkbox("自动以小钓大", ref cfg.UseAutoMooch, "This option takes priority over the Auto Cast Line"))
            {
                if (!cfg.UseAutoMooch)
                    cfg.UseAutoMooch2 = false;
            }

            if (cfg.UseAutoMooch)
            {
                ImGui.Indent();
                ImGui.Checkbox("使用以小钓大II", ref cfg.UseAutoMooch2);
                ImGui.Unindent();
            }
            ImGui.EndPopup();
        }
    }

    public void DrawSurfaceSlapIdenticalCast(HookConfig cfg)
    {

        if (ImGui.Button("拍击水面和专一垂钓"))
        {
            ImGui.OpenPopup("surface_slap_identical_cast");
        }

        if (ImGui.BeginPopup("surface_slap_identical_cast"))
        {
            ImGui.TextColored(ImGuiColors.DalamudYellow, "拍击水面和专一垂钓");
            ImGui.Spacing();
            if (DrawUtil.Checkbox("使用拍击水面", ref cfg.UseSurfaceSlap, "覆盖专一垂钓"))
            {
                cfg.UseIdenticalCast = false;
            }

            if (DrawUtil.Checkbox("使用专一垂钓", ref cfg.UseIdenticalCast, "覆盖拍击水面"))
            {
                cfg.UseSurfaceSlap = false;
            }

            ImGui.EndPopup();
        }
    }

    public void DrawStopAfter(HookConfig cfg)
    {

        if (ImGui.Button("停止垂钓当..."))
        {
            ImGui.OpenPopup(str_id: "stop_after");
        }

        if (ImGui.BeginPopupContextWindow("stop_after"))
        {
            ImGui.TextColored(ImGuiColors.DalamudYellow, "停止垂钓");
            ImGui.Spacing();
            if (DrawUtil.Checkbox("钓到...", ref cfg.StopAfterCaught, "- If this config is a bait: Stops fishing after X amount of fish is caught\n- If this config is a fish: Stops fishing after it being caught X amount of times"))
            {

            }

            if (cfg.StopAfterCaught)
            {
                ImGui.Indent();
                ImGui.SetNextItemWidth(90 * ImGuiHelpers.GlobalScale);
                if (ImGui.InputInt("次", ref cfg.StopAfterCaughtLimit))
                {
                    if (cfg.StopAfterCaughtLimit < 1)
                        cfg.StopAfterCaughtLimit = 1;
                }

                ImGui.Unindent();
            }

            ImGui.EndPopup();
        }
    }
}
