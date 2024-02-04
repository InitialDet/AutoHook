using System;
using System.Collections.Generic;
using System.Numerics;
using AutoHook.Resources.Localization;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using ImGuiNET;

namespace AutoHook.Utils;

public static class DrawUtil
{
    public static void NumericDisplay(string label, int value)
    {
        ImGui.Text(label);
        ImGui.SameLine();
        ImGui.Text($"{value}");
    }

    public static void NumericDisplay(string label, string formattedString)
    {
        ImGui.Text(label);
        ImGui.SameLine();
        ImGui.Text(formattedString);
    }

    public static void NumericDisplay(string label, int value, Vector4 color)
    {
        ImGui.Text(label);
        ImGui.SameLine();
        ImGui.TextColored(color, $"{value}");
    }

    public static bool EditNumberField(string label, ref int refValue, string helpText = "")
    {
        return EditNumberField(label, 30, ref refValue, helpText);
    }

    public static bool EditNumberField(string label, float fieldWidth, ref int refValue, string helpText = "")
    {
        TextV(label);

        ImGui.SameLine();

        ImGui.PushItemWidth(fieldWidth * ImGuiHelpers.GlobalScale);
        var clicked = ImGui.InputInt($"##{label}###", ref refValue, 0, 0);
        ImGui.PopItemWidth();

        if (helpText != string.Empty)
        {
            ImGuiComponents.HelpMarker(helpText);
        }

        return clicked;
    }

    public static void TextV(string s)
    {
        var cur = ImGui.GetCursorPos();
        ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0);
        ImGui.Button("");
        ImGui.PopStyleVar();
        ImGui.SameLine();
        ImGui.SetCursorPos(cur);
        ImGui.TextUnformatted(s);
    }

    public static bool Checkbox(string label, ref bool refValue, string helpText = "", bool hoverHelpText = false)
    {
        var clicked = ImGui.Checkbox($"{label}", ref refValue);

        if (helpText != string.Empty)
        {
            if (hoverHelpText)
            {
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip(helpText);
            }
            else
                ImGuiComponents.HelpMarker(helpText);
        }

        return clicked;
    }

    public static void DrawWordWrappedString(string message)
    {
        var words = message.Split(' ');

        var windowWidth = ImGui.GetContentRegionAvail().X;
        var cumulativeSize = 0.0f;
        var padding = 2.0f;

        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(2.0f, 0.0f));

        foreach (var word in words)
        {
            var wordWidth = ImGui.CalcTextSize(word).X;

            if (cumulativeSize == 0)
            {
                ImGui.Text(word);
                cumulativeSize += wordWidth + padding;
            }
            else if ((cumulativeSize + wordWidth) < windowWidth)
            {
                ImGui.SameLine();
                ImGui.Text(word);
                cumulativeSize += wordWidth + padding;
            }
            else if ((cumulativeSize + wordWidth) >= windowWidth)
            {
                ImGui.Text(word);
                cumulativeSize = wordWidth + padding;
            }
        }

        ImGui.PopStyleVar();
    }

    private static string _filterText = "";
    
    public static void DrawComboSelector<T>(
        List<T> itemList,
        Func<T, string> getItemName,
        string selectedItem,
        Action<T> onSelect)
    {
        
        ImGui.SetNextItemWidth(200 * ImGuiHelpers.GlobalScale);
        
        if (ImGui.BeginCombo("###search", selectedItem))
        {
            string clearText = "";
            ImGui.SetNextItemWidth(190 * ImGuiHelpers.GlobalScale);
            if (ImGui.InputTextWithHint("", UIStrings.Search_Hint, ref clearText, 100))
            {
                _filterText = new string(clearText);
            }
            
            ImGui.Separator();

            if (ImGui.BeginChild("ComboSelector", new Vector2(0, 100 * ImGuiHelpers.GlobalScale), false))
            {
                
                foreach (var item in itemList)
                {
                    var itemName = getItemName(item);
                    var filterTextLower = _filterText.ToLower();

                    if (_filterText.Length != 0 && !itemName.ToLower().Contains(filterTextLower))
                        continue;

                    if (ImGui.Selectable(itemName, false))
                    {
                        onSelect(item);
                        _filterText = "";
                        clearText = "";
                        ImGui.CloseCurrentPopup();
                        Service.Save();
                    }
                }
                
                ImGui.EndChild();
            }
            ImGui.EndCombo();
        }
    }
    
    public static void DrawCheckboxTree(string treeName, ref bool enable, Action action, string helpText = "")
    {
        ImGui.Checkbox("", ref enable);
        
        if (helpText != string.Empty)
        {
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip(helpText);
        }
        
        ImGui.SameLine();
        if (ImGui.TreeNodeEx(treeName, ImGuiTreeNodeFlags.FramePadding))
        {
            ImGui.Spacing();
            ImGui.Indent();
            action();
            ImGui.Unindent();
            ImGui.Separator();
            ImGui.TreePop();
        }
    }

    public static void SpacingSeparator()
    {
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();
    }
}