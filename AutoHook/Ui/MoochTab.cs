using System;
using System.Collections.Generic;
using AutoHook.Configurations;
using AutoHook.FishTimer;
using AutoHook.Ui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Logging;
using ImGuiNET;

namespace AutoHook
{
    class MoochTab : TabConfig
    {
        public override bool Enabled => true;
        public override string TabName => "Bait/Mooch";

        public MoochTab()
        { }

        public override void Draw()
        {
            ImGui.BeginGroup();

            for (int idx = 0; idx < Service.Configuration.CustomBaitMooch.Count; idx++)
            {
                var bait = Service.Configuration.CustomBaitMooch[idx];
                ImGui.PushID($"id###{idx}");
                if (ImGui.CollapsingHeader($"{bait.BaitName}###{idx}"))
                {
                    ImGui.Indent();

                    DrawEnabledButtonCustomBait(bait);
                    DrawInputDoubleMinTime(bait);
                    DrawInputDoubleMaxTime(bait);
                    DrawHookCheckboxes(bait);
                    DrawCheckBoxDoubleTripleHook(bait);
                    DrawInputTextName(bait);

                    ImGui.SameLine();
                    DrawDeleteBaitButton(bait);

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
                var setting = new HookSettings("EditMe");
                if (!Service.Configuration.CustomBaitMooch.Contains(setting))
                    Service.Configuration.CustomBaitMooch.Add(setting);

                Service.Configuration.Save();
            }

            ImGui.SameLine();
            ImGui.Text($"New bait/mooch ({Service.Configuration.CustomBaitMooch.Count})");
            ImGui.SameLine();
            ImGuiComponents.HelpMarker("Make sure to edit the bait/mooch name correctly like ingame (Ex: Versatile Lure)");

            if (ImGui.Button("Add Current Bait/Mooch"))
            {
                var setting = new HookSettings(HookingManager.CurrentBait ?? "-");

                if (!Service.Configuration.CustomBaitMooch.Contains(setting))
                    Service.Configuration.CustomBaitMooch.Add(setting);

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
}