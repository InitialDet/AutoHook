using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
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
        ImGui.Text(label);

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

    public static bool Checkbox(string label, ref bool refValue, string helpText = "")
    {
        var clicked = ImGui.Checkbox($"{label}", ref refValue);

        if (helpText != string.Empty)
        {
            ImGuiComponents.HelpMarker(helpText);
        }

        return clicked;
    }

    public static void CompleteIncomplete(bool complete)
    {
        ConditionalText(complete, "Complete", "Incomplete");
    }

    public static void ConditionalText(bool condition, string trueString, string falseString)
    {
        if (condition)
        {
            ImGui.TextColored(new Vector4(0, 255, 0, 0.8f), trueString);
        }
        else
        {
            ImGui.TextColored(new Vector4(185, 0, 0, 0.8f), falseString);
        }
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
}

