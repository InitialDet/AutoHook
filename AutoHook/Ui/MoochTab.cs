using AutoHook.Configurations;
using AutoHook.FishTimer;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using ImGuiNET;

namespace AutoHook.Ui;

class MoochTab : TabConfig
{
    public override bool Enabled => true;
    public override string TabName => "Bait/Mooch";

    public MoochTab()
    { }

    public override void Draw()
    {
        ImGui.BeginGroup();

        for (int idx = 0; idx < Service.Configuration.CustomBait.Count; idx++)
        {
            var bait = Service.Configuration.CustomBait[idx];
            ImGui.PushID($"id###{idx}");
            if (ImGui.CollapsingHeader($"{bait.BaitName}###{idx}"))
            {
                ImGui.Indent();
                DrawEnabledButtonCustomBait(bait);
                ImGui.SameLine();
                DrawDeleteBaitButton(bait);
                DrawInputTextName(bait);
                DrawInputDoubleMinTime(bait);
                DrawInputDoubleMaxTime(bait);
                DrawHookCheckboxes(bait);
                ImGui.Separator();
                DrawCheckBoxDoubleTripleHook(bait);
                ImGui.Separator();
                DrawPatienceConfig(bait);
                ImGui.Separator();
                DrawAutoMooch(bait);

                ImGui.Unindent();

            }
            ImGui.PopID();
        }
        ImGui.EndGroup();
    }

    public override void DrawHeader()
    {
        ImGui.TextWrapped("Here you can customize which hook to use based on the current bait or fish being mooched.\nIf a bait/mooch is not specified, the default behavior (General Tab) will be used instead.");
        if (ImGui.Button("Add"))
        {
            var setting = new HookConfig("EditMe");
            if (!Service.Configuration.CustomBait.Contains(setting))
                Service.Configuration.CustomBait.Add(setting);

            Service.Configuration.Save();
        }

        ImGui.SameLine();
        ImGui.Text($"New bait/mooch ({Service.Configuration.CustomBait.Count})");
        ImGui.SameLine();
        ImGuiComponents.HelpMarker("Make sure to edit the bait/mooch name correctly like ingame (Ex: Versatile Lure)");

        if (ImGui.Button("Add Current Bait/Mooch"))
        {
            var setting = new HookConfig(HookingManager.CurrentBait ?? "-");

            if (!Service.Configuration.CustomBait.Contains(setting))
                Service.Configuration.CustomBait.Add(setting);

            Service.Configuration.Save();
        }

        /*ImGui.SameLine();
        if (ImGui.Button("Reset"))
        {
            foreach (HookSettings mooch in Settings.CustomBaitMooch)
            {
                Settings.CustomBaitMooch.Remove(mooch);
            }
            Settings.CustomBaitMooch.Add(new HookSettings("Generic"));
            Settings.Save();
        }*/

        ImGui.Text($"Current bait/mooch:");
        ImGui.SameLine();
        ImGui.TextColored(ImGuiColors.HealerGreen, HookingManager.CurrentBait ?? "-");
    }
}
