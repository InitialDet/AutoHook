using AutoHook.Configurations;
using AutoHook.FishTimer;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using ImGuiNET;

namespace AutoHook.Ui;

class TabBaitFish : TabBaseConfig
{
    public override bool Enabled => true;
    public override string TabName => "Bait/Fish";

    public TabBaitFish()
    { }

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

        ImGui.Text($"Current bait/mooch:");
        ImGui.SameLine();
        ImGui.TextColored(ImGuiColors.HealerGreen, HookingManager.CurrentBait ?? "-");
    }

    public override void Draw()
    {
        ImGui.BeginGroup();

        for (int idx = 0; idx < Service.Configuration.CustomBait.Count; idx++)
        {
            var bait = Service.Configuration.CustomBait[idx];
            ImGui.PushID($"id###{idx}");
            if (ImGui.CollapsingHeader($"{bait.BaitName}###{idx}"))
            {
                DrawEnabledButtonCustomBait(bait);
                ImGui.Indent();
                ImGui.SameLine();
                DrawDeleteBaitButton(bait);
                DrawInputTextName(bait);
                DrawInputDoubleMinTime(bait);
                DrawInputDoubleMaxTime(bait);
                DrawHookCheckboxes(bait);
                ImGui.Spacing();

                /*
                DrawFishersIntuitionConfig(bait);
                ImGui.Spacing();
                DrawCheckBoxDoubleTripleHook(bait);
                ImGui.Spacing();
                DrawSurfaceSlapIdenticalCast(bait);
                ImGui.Spacing();
                DrawAutoMooch(bait);
                //DrawPatienceConfig(bait);
                //ImGui.Separator();
                */

                ImGuiTableFlags flags = ImGuiTableFlags.SizingFixedFit | /*ImGuiTableFlags.Resizable |*/ ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.NoHostExtendX | ImGuiTableFlags.ContextMenuInBody;

                if (ImGui.BeginTable("table2", 2, flags))
                {

                    // Collumn 1
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    DrawFishersIntuitionConfig(bait);
                    ImGui.TableNextColumn();
                    DrawCheckBoxDoubleTripleHook(bait);

                    // Collumn 2
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    DrawSurfaceSlapIdenticalCast(bait);
                    ImGui.TableNextColumn();
                    DrawAutoMooch(bait);

                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    DrawStopAfter(bait);

                    ImGui.EndTable();
                }

                ImGui.Unindent();
            }
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
            ImGui.PopID();
        }
        ImGui.EndGroup();
    }
}
