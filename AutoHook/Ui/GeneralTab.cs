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
        ImGui.TextWrapped("The new Auto Cast/Mooch is a experimental feature and can be a little confusing at first. I'll be trying to find a more simple and intuitive solution later\nPlease report any issues you encounter.");
        ImGui.Text("Discord: Det#8574");

        ImGui.Checkbox("Global Auto Cast", ref Service.Configuration.UseAutoCast);
        ImGuiComponents.HelpMarker("Cast will be used after a fish is hooked");
        if(ImGui.Checkbox("Global Auto Mooch", ref Service.Configuration.UseAutoMooch)) {
            if (!Service.Configuration.UseAutoMooch)
                Service.Configuration.UseAutoMooch2 = false;
        }
        ImGuiComponents.HelpMarker("All fish will be mooched if available. This option have priority over Auto Cast\nIf you want to Auto Mooch only a especific fish and ignore others, disable this option and add the fish you want in the bait/mooch tab");

        if (Service.Configuration.UseAutoMooch)
        {
            ImGui.Indent();
            ImGui.Checkbox("Use Mooch II", ref Service.Configuration.UseAutoMooch2);
            ImGui.Unindent();
        }
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
