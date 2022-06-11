using Dalamud.Interface.Components;
using ImGuiNET;

namespace AutoHook.Ui;

internal class GeneralTab : TabConfig
{
    public override bool Enabled => true;
    public override string TabName => "General";

    public override void DrawHeader()
    {
        ImGui.Text("General settings");
        ImGui.Separator();
        ImGui.TextWrapped("Please report any issues you encounter.");
        ImGui.Text("Discord: Det#8574");    
    }

    public override void Draw()
    {
        DrawDefaultCast();
        ImGui.Separator();
        DrawDefaultMooch();
    }

    public void DrawDefaultCast()
    {
        ImGui.PushID($"{TabName}-DefaultCast");
        ImGui.Checkbox("Use Default Cast", ref Service.Configuration.DefaultCastConfig.Enabled);
        ImGuiComponents.HelpMarker("This is the default hooking behavior if no specific Bait Config is found.");

        ImGui.Indent();
        
        DrawInputDoubleMinTime(Service.Configuration.DefaultCastConfig);
        DrawInputDoubleMaxTime(Service.Configuration.DefaultCastConfig);
        DrawHookCheckboxes(Service.Configuration.DefaultCastConfig);
        DrawCheckBoxDoubleTripleHook(Service.Configuration.DefaultCastConfig);
        DrawPatienceConfig(Service.Configuration.DefaultCastConfig);

        ImGui.Unindent();

        ImGui.PopID();
    }

    public void DrawDefaultMooch()
    {
        ImGui.PushID($"{TabName}-DefaultMooch");
        ImGui.Checkbox("Use Default Mooch", ref Service.Configuration.DefaultMoochConfig.Enabled);
        ImGuiComponents.HelpMarker("This is the default hooking behavior if no specific Mooch Config is found.");

        ImGui.Indent();
        
        DrawInputDoubleMinTime(Service.Configuration.DefaultMoochConfig);
        DrawInputDoubleMaxTime(Service.Configuration.DefaultMoochConfig);
        DrawHookCheckboxes(Service.Configuration.DefaultMoochConfig);
        DrawCheckBoxDoubleTripleHook(Service.Configuration.DefaultMoochConfig);
        DrawPatienceConfig(Service.Configuration.DefaultMoochConfig);
    
        ImGui.Unindent();

        ImGui.PopID();
    }
}
