using System.Diagnostics;
using AutoHook.Resources.Localization;
using AutoHook.Utils;
using Dalamud.Interface.Utility;
using ImGuiNET;

namespace AutoHook.Ui;

public class TabConfigGuides : BaseTab
{
    public override string TabName { get; } = UIStrings.TabName_Config_Guides;
    public override bool Enabled { get; } = true;
    
    private bool _showDescription;
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
                "Localization options were added, but currently only English is available. If you want to help with the translation, please visit the link below");
        
            ImGui.Spacing();
        
            if (ImGui.Button(UIStrings.TabGeneral_DrawHeader_Localization_Help))
            {
                Process.Start(new ProcessStartInfo
                    { FileName = "https://crowdin.com/project/autohook-plugin-localization", UseShellExecute = true });
            }

            ImGui.Spacing();
        
            ImGui.TextWrapped(
                "This page will be updated more in the future.");
        }
       
        
        ImGui.Spacing();
    }

    public override void Draw()
    {
        if (ImGui.BeginTabBar(@"TabBarsConfig", ImGuiTabBarFlags.NoTooltip))
        {
            if (ImGui.BeginTabItem("Configs"))
            {
                ImGui.Spacing();
                DrawConfigs();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Guides"))
            {
                ImGui.Spacing();
                DrawGuides();
                ImGui.EndTabItem();
            }
            
            ImGui.EndTabBar();
        }
    }
    
    private void DrawConfigs()
    {
        
        if (DrawUtil.Checkbox(UIStrings.Show_Debug_Console, ref Service.Configuration.ShowDebugConsole))
        {
            Service.Save();
        }
        
        if (DrawUtil.Checkbox(UIStrings.Show_Chat_Logs, ref Service.Configuration.ShowChatLogs, UIStrings.Show_Chat_Logs_HelpText))
        {
            Service.Save();
        }
        
        ImGui.TextWrapped(UIStrings.Delay_Between_Casts); 
       
        ImGui.SetNextItemWidth(45 * ImGuiHelpers.GlobalScale);
        if (ImGui.InputInt(UIStrings.DrawConfigs_Min_, ref Service.Configuration.DelayBetweenCastsMin, 0))
        {
            if (Service.Configuration.DelayBetweenCastsMin < 1)
                Service.Configuration.DelayBetweenCastsMin = 1;

            Service.Save();
        }
        ImGui.SameLine();
        ImGui.SetNextItemWidth(45 * ImGuiHelpers.GlobalScale);
        if (ImGui.InputInt(UIStrings.DrawConfigs_Max_, ref Service.Configuration.DelayBetweenCastsMax,0))
        {
            if (Service.Configuration.DelayBetweenCastsMax < 1)
                Service.Configuration.DelayBetweenCastsMax = 1;

            Service.Save();
        }
        
        ImGui.Spacing();
        
        if (DrawUtil.Checkbox(UIStrings.Show_Current_Status_Header, ref Service.Configuration.ShowStatusHeader))
        {
            Service.Save();
        }
    }

    private void DrawGuides()
    {
        if (ImGui.Button(UIStrings.TabAutoCasts_DrawHeader_Guide_Collectables))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/InitialDet/AutoHook/blob/main/AcceptCollectable.md",
                UseShellExecute = true
            });
        }
    }
}