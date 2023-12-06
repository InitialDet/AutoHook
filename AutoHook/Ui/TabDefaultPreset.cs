using AutoHook.Resources.Localization;
using ImGuiNET;

namespace AutoHook.Ui;

internal class TabDefaultPreset : BaseTab
{
    public override bool Enabled => true;
    public override string TabName => UIStrings.TabName_Default_Preset;

    private SubTabBaitMooch _subTabBaitMooch = new();
    private SubTabAutoCast _subTabAutoCast = new();
    private SubTabFish _subTabFish = new();
    private SubTabExtra _subTabExtra = new();

    private bool _showDescription = true;

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
            ImGui.TextWrapped(
                UIStrings.TabDefaultPreset_Description);
        }


        ImGui.Spacing();
    }

    public override void Draw()
    {
        ImGui.PushID("TabBarsDefault");
        if (ImGui.BeginTabBar(@"TabBarsDefault", ImGuiTabBarFlags.NoTooltip))
        {
            var preset = Service.Configuration.HookPresets.DefaultPreset;
            if (ImGui.BeginTabItem(UIStrings.Bait))
            {
                ImGui.PushID("TabDefaultCast");
                _subTabBaitMooch.IsMooch = false;
                _subTabBaitMooch.IsDefault = true;
                _subTabBaitMooch.DrawHookTab(preset);
                ImGui.PopID();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem($"{UIStrings.Mooch}"))
            {
                ImGui.PushID("TabDefaultMooch");
                _subTabBaitMooch.IsMooch = true;
                _subTabBaitMooch.IsDefault = true;
                _subTabBaitMooch.DrawHookTab(preset);
                ImGui.PopID();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem(UIStrings.Fish))
            {
                _subTabFish.DrawFishTab(preset);
                ImGui.EndTabItem();
            }
            
            if (ImGui.BeginTabItem(UIStrings.Extra))
            {
                _subTabExtra.IsDefaultPreset = true;
                _subTabExtra.DrawExtraTab(preset.ExtraCfg);
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem($"{UIStrings.Auto_Casts}"))
            {
                ImGui.PushID("TabDefaultAutoCast");
                _subTabAutoCast.IsDefaultPreset = true;
                _subTabAutoCast.DrawAutoCastTab(preset.AutoCastsCfg);
                ImGui.PopID();
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }

        ImGui.PopID();
    }
}