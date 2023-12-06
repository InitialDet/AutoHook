using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using AutoHook.Configurations;
using AutoHook.Resources.Localization;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;

namespace AutoHook.Ui;

public class TabCustomPresets : BaseTab
{
    public override bool Enabled => true;
    public override string TabName => UIStrings.TabNameCustomPresets;

    private PresetConfig? _tempImport = null;

    private HookPresets _hookPresets = Service.Configuration.HookPresets;

    private SubTabBaitMooch _subTabBaitMooch = new();
    private SubTabAutoCast _subTabAutoCast = new();
    private SubTabFish _subTabFish = new();
    private SubTabExtra _subTabExtra = new();
    
    private bool _showDescription;

    public override void DrawHeader()
    {
        ImGui.Spacing();


        if (ImGui.TreeNodeEx(UIStrings.Tab_Description,
                ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.FramePadding))
        {
            _showDescription = true;
            ImGui.TreePop();
        }
        else
            _showDescription = false;

        ImGui.Spacing();

        if (_showDescription)
        {
            ImGui.TextWrapped(UIStrings.TabPresets_DrawHeader_NewTabDescription);

            ImGui.Spacing();
        }

        DrawPresetSelection();

        ImGui.SameLine();

        DrawImportExport();

        ImGui.SameLine();

        DrawDeletePreset();

        ImGui.Spacing();
    }

    public override void Draw()
    {
        if (_hookPresets.SelectedPreset == null)
            return;

        if (ImGui.BeginTabBar(@"TabBarsPreset", ImGuiTabBarFlags.NoTooltip))
        {
            if (ImGui.BeginTabItem(UIStrings.Bait))
            {
                _subTabBaitMooch.IsMooch = false;
                _subTabBaitMooch.DrawHookTab(_hookPresets.SelectedPreset);
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem(UIStrings.Mooch))
            {
                _subTabBaitMooch.IsMooch = true;
                _subTabBaitMooch.DrawHookTab(_hookPresets.SelectedPreset);
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem(UIStrings.Fish))
            {
                _subTabFish.DrawFishTab(_hookPresets.SelectedPreset);
                ImGui.EndTabItem();
            }
            
            if (ImGui.BeginTabItem(UIStrings.Extra))
            {
                _subTabExtra.DrawExtraTab(_hookPresets.SelectedPreset.ExtraCfg);
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem(UIStrings.Auto_Casts))
            {
                _subTabAutoCast.DrawAutoCastTab(_hookPresets.SelectedPreset.AutoCastsCfg);
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
    }

    private void DrawDeletePreset()
    {
        ImGui.PushFont(UiBuilder.IconFont);
        if (ImGui.Button($"{FontAwesomeIcon.Trash.ToIconChar()}", new Vector2(ImGui.GetFrameHeight(), 0)) &&
            ImGui.GetIO().KeyShift)
        {
            if (_hookPresets.SelectedPreset != null)
            {
                _hookPresets.CustomPresets.Remove(_hookPresets.SelectedPreset);
                _hookPresets.SelectedPreset = null;
            }

            Service.Save();
        }

        ImGui.PopFont();

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(UIStrings.HoldShiftToDelete);
    }

    private void DrawEditPresetName()
    {
        if (_hookPresets.SelectedPreset == null)
            return;

        if (ImGui.BeginPopupContextItem("PresetName###name"))
        {
            string name = _hookPresets.SelectedPreset.PresetName;
            ImGui.Text(UIStrings.TabPresets_DrawHeader_EditPresetNamePressEnterToConfirm);

            if (ImGui.InputText(UIStrings.PresetName, ref name, 64,
                    ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
            {
                if (_hookPresets.SelectedPreset != null &&
                    _hookPresets.CustomPresets.All(preset => preset.PresetName != name))
                {
                    _hookPresets.SelectedPreset.RenamePreset(name);
                    Service.Save();
                }
            }

            if (ImGui.Button(UIStrings.Close))
            {
                ImGui.CloseCurrentPopup();
                Service.Save();
            }
            

            ImGui.EndPopup();
        }
    }

    private void DrawPresetSelection()
    {
        ImGui.SetNextItemWidth(230);
        if (ImGui.BeginCombo("", _hookPresets.SelectedPreset?.PresetName ?? UIStrings.None))
        {
            foreach (var preset in _hookPresets.CustomPresets)
            {
                if (ImGui.Selectable(preset.PresetName, preset.PresetName == _hookPresets.SelectedPreset?.PresetName))
                {
                    _hookPresets.SelectedPreset = preset;
                }
            }

            ImGui.EndCombo();
        }

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(UIStrings.RightClickToRename);

        DrawEditPresetName();

        ImGui.SameLine();
        ImGui.PushFont(UiBuilder.IconFont);

        var buttonSize = ImGui.CalcTextSize(FontAwesomeIcon.Plus.ToIconString()) + ImGui.GetStyle().FramePadding * 2;
        if (ImGui.Button(FontAwesomeIcon.Plus.ToIconString(), buttonSize))
        {
            try
            {
                PresetConfig preset = new(@$"{UIStrings.NewPreset} {_hookPresets.CustomPresets.Count + 1}");
                _hookPresets.AddPreset(preset);
                _hookPresets.SelectedPreset = preset;
                Service.Save();
            }
            catch (Exception e)
            {
                Service.PluginLog.Error(e.ToString());
            }
        }

        ImGui.PopFont();
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(UIStrings.AddNewPreset);
    }

    private void DrawImportExport()
    {
        ImGui.PushFont(UiBuilder.IconFont);

        var buttonSize = ImGui.CalcTextSize(FontAwesomeIcon.SignOutAlt.ToIconString()) +
                         ImGui.GetStyle().FramePadding * 2;

        if (ImGui.Button(FontAwesomeIcon.SignOutAlt.ToIconString(), buttonSize))
        {
            try
            {
                ImGui.SetClipboardText(Configuration.ExportActionStack(_hookPresets.SelectedPreset!));

                _alertMessage = UIStrings.PresetExportedToTheClipboard;
                _alertTimer.Start();
            }
            catch (Exception e)
            {
                Service.PrintDebug(e.Message);
                _alertMessage = e.Message;
                _alertTimer.Start();
            }
        }

        ImGui.PopFont();

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(UIStrings.ExportPresetToClipboard);

        ImGui.SameLine();

        ImGui.PushFont(UiBuilder.IconFont);

        if (ImGui.Button(FontAwesomeIcon.SignInAlt.ToIconString(), buttonSize))
        {
            try
            {
                _tempImport = Configuration.ImportActionStack(ImGui.GetClipboardText());

                if (_tempImport != null)
                {
                    ImGui.OpenPopup("import_new_preset");
                }
            }
            catch (Exception e)
            {
                Service.PrintDebug($"[TabCustomPresets] {e.Message}");
                _alertMessage = e.Message;
                _alertTimer.Start();
            }
        }

        ImGui.PopFont();

        if (_tempImport != null)
        {
            if (ImGui.BeginPopup("import_new_preset"))
            {
                string name = _tempImport.PresetName;

                if (_tempImport.PresetName.StartsWith("[Old Version]"))
                    ImGui.TextColored(ImGuiColors.ParsedOrange, UIStrings.Old_Preset_Warning);
                else
                    ImGui.TextWrapped(UIStrings.ImportThisPreset);

                if (ImGui.InputText(UIStrings.PresetName, ref name, 64, ImGuiInputTextFlags.AutoSelectAll))
                {
                    _tempImport.RenamePreset(name);
                }

                if (ImGui.Button(UIStrings.Import))
                {
                    if (_hookPresets.CustomPresets.Any(preset => preset.PresetName == name))
                    {
                        _alertMessage = UIStrings.PresetAlreadyExist;
                        _alertTimer.Start();
                    }
                    else
                    {
                        _hookPresets.AddPreset(_tempImport);
                        _hookPresets.SelectedPreset = _tempImport;
                        _tempImport = null;
                        Service.Save();
                    }
                }

                ImGui.SameLine();

                if (ImGui.Button(UIStrings.DrawImportExport_Cancel))
                {
                    _tempImport = null;
                    ImGui.CloseCurrentPopup();
                }

                TimedWarning();

                ImGui.EndPopup();
            }
        }

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(UIStrings.ImportStackFromClipboard);

        TimedWarning();
    }

    private static readonly double _timelimit = 5000;
    private readonly Stopwatch _alertTimer = new();
    private string _alertMessage = "-";

    private void TimedWarning()
    {
        if (_alertTimer.IsRunning)
        {
            ImGui.TextColored(ImGuiColors.DalamudYellow, _alertMessage);

            if (_alertTimer.ElapsedMilliseconds > _timelimit)
            {
                _alertTimer.Reset();
            }
        }
    }
}