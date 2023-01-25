using AutoHook.Data;
using AutoHook.Utils;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Component.GUI;
using GatherBuddy.Enums;
using GatherBuddy.SeFunctions;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace AutoHook.Spearfishing;
internal class AutoGig : Window, IDisposable
{
    private static unsafe ActionManager* _actionManager = ActionManager.Instance();

    private const ImGuiWindowFlags WindowFlags = ImGuiWindowFlags.NoDecoration
      | ImGuiWindowFlags.NoInputs
      | ImGuiWindowFlags.AlwaysAutoResize
      | ImGuiWindowFlags.NoFocusOnAppearing
      | ImGuiWindowFlags.NoNavFocus
      | ImGuiWindowFlags.NoBackground;

    private float _uiScale = 1;
    private Vector2 _uiPos = Vector2.Zero;
    private Vector2 _uiSize = Vector2.Zero;
    private unsafe SpearfishWindow* _addon = null;

    private readonly List<SpearfishSize> _sizeTypes = Enum.GetValues(typeof(SpearfishSize)).Cast<SpearfishSize>().ToList();
    private readonly List<SpearfishSpeed> _speedTypes = Enum.GetValues(typeof(SpearfishSpeed)).Cast<SpearfishSpeed>().ToList();

    private string currentKey = "zero";

    public AutoGig() : base("SpearfishingHelper", WindowFlags, true)
    {
        Service.WindowSystem.AddWindow(this);
        IsOpen = true;

        currentKey = Service.Configuration.currentSize.ToName() + Service.Configuration.currentSpeed.ToName();
    }

    public static void ShowKofi()
    {
        string buttonText = "Support on Ko-fi";
        ImGui.PushStyleColor(ImGuiCol.Button, 0xFF000000 | 0x005E5BFF);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0xDD000000 | 0x005E5BFF);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0xAA000000 | 0x005E5BFF);

        if (ImGui.Button(buttonText))
        {
            Process.Start(new ProcessStartInfo { FileName = "https://ko-fi.com/initialdet", UseShellExecute = true });
        }

        ImGui.PopStyleColor(3);
    }

    public void Dispose()
    {
        Service.WindowSystem.RemoveWindow(this);
        Service.Configuration.Save();
    }

    public override void Draw()
    {
        if (!Service.Configuration.AutoGigHideOverlay)
            DrawFishOverlay();
    }

    public unsafe void DrawSettings()
    {
        currentKey = currentKey = Service.Configuration.currentSize.ToName() + Service.Configuration.currentSpeed.ToName();
        if (ImGui.Checkbox("Enable AutoGig ", ref Service.Configuration.AutoGigEnabled))
        {
            Service.Configuration.Save();
        }

        ImGui.SameLine();

        try
        {
            if (Service.Configuration.GigSpacing != null)
            {
                int hitbox;

                if (!Service.Configuration.GigSpacing.ContainsKey(currentKey))
                    Service.Configuration.GigSpacing.Add(currentKey, 30);

                hitbox = Service.Configuration.GigSpacing[currentKey];
                ImGui.SetNextItemWidth(90);
                if (ImGui.InputInt("Hitbox ", ref hitbox))
                {
                    if (hitbox > 300)
                        hitbox = 300;

                    if (hitbox < 0)
                        hitbox = 0;

                    Service.Configuration.GigSpacing[currentKey] = hitbox;
                }
                ImGui.SameLine();
                if (ImGui.Checkbox("Use Nature's Bounty ", ref Service.Configuration.AutoGigNaturesBountyEnabled))
                {
                    Service.Configuration.Save();
                }
            }
        }
        catch (Exception ex)
        {
            Service.Configuration.GigSpacing[currentKey] = 25;
            PluginLog.Debug(ex.Message);
        }

        ImGui.SameLine();

        ShowKofi();

        ImGui.SetNextItemWidth(130);
        if (ImGui.BeginCombo("Size", Service.Configuration.currentSize.ToName()))
        {

            foreach (SpearfishSize size in _sizeTypes.Where(size =>
                        ImGui.Selectable(size.ToName(), size == Service.Configuration.currentSize)))
            {
                Service.Configuration.currentSize = size;
            }
            ImGui.EndCombo();
        }

        ImGui.SameLine();

        ImGui.SetNextItemWidth(130);
        if (ImGui.BeginCombo("Speed", Service.Configuration.currentSpeed.ToName()))
        {
            foreach (SpearfishSpeed speed in _speedTypes.Where(speed =>
                        ImGui.Selectable(speed.ToName(), speed == Service.Configuration.currentSpeed)))
            {
                Service.Configuration.currentSpeed = speed;
            }
            ImGui.EndCombo();
        }
    }

    private unsafe void DrawFishOverlay()
    {
        _addon = (SpearfishWindow*)Service.GameGui.GetAddonByName("SpearFishing", 1);

        bool _isOpen = _addon != null && _addon->Base.WindowNode != null;

        if (!_isOpen)
            return;

        ImGui.SetNextWindowPos(new Vector2(_addon->Base.X + 5, _addon->Base.Y - 65));
        if (ImGui.Begin("gig", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar))
        {
            DrawSettings();
            ImGui.End();
        }


        if (Service.Configuration.AutoGigEnabled)
        {
            /*if (!PlayerResources.HasStatus(IDs.Status.NaturesBounty) && Service.Configuration.AutoGigNaturesBountyEnabled)
                PlayerResources.CastActionDelayed(IDs.Actions.NaturesBounty);*/

            GigFish(_addon->Fish1, _addon->Fish1Node);
            GigFish(_addon->Fish2, _addon->Fish2Node);
            GigFish(_addon->Fish3, _addon->Fish3Node);
        }
    }

    private unsafe void GigFish(SpearfishWindow.Info info, AtkResNode* node)
    {
        var drawList = ImGui.GetWindowDrawList();

        DrawGigHitbox(drawList);

        if (!info.Available)
            return;

        var currentSize = Service.Configuration.currentSize;
        var currentSpeed = Service.Configuration.currentSpeed;
        var gigFish = (info.Size == currentSize || currentSize == SpearfishSize.All) &&
                  (info.Speed == currentSpeed || currentSpeed == SpearfishSpeed.All);

        if (!gigFish)
            return;

        if (!PlayerResources.HasStatus(IDs.Status.NaturesBounty) && Service.Configuration.AutoGigNaturesBountyEnabled)
            PlayerResources.CastActionDelayed(IDs.Actions.NaturesBounty);

        var centerX = (_uiSize.X / 2);

        float fishHitbox = 0;

        // Im so tired of trying to figure this out someone help
        if (node->GetScaleX() == -1)
            fishHitbox = (node->X - (node->Width / 100 * 43)) * _uiScale;
        else
            fishHitbox = (node->X + (node->Width / 100 * 55)) * _uiScale;

        int hitBox = Service.Configuration.GigSpacing[currentKey];

        fishHitbox = (int)fishHitbox;
        DrawFishHitbox(drawList, fishHitbox);

        if (fishHitbox >= (centerX - hitBox) - 3 && fishHitbox <= (centerX + hitBox) + 3)
        {

            PlayerResources.CastActionNoDelay(IDs.Actions.Gig);

            if (node->GetScaleX() == -1)
                PluginLog.Debug($"FishHitbox L = {fishHitbox}, GigHitbox = {centerX - hitBox}");
            else
                PluginLog.Debug($"FishHitbox R= {fishHitbox}, GigHitbox = {centerX + hitBox}");

        }
    }

    private unsafe void DrawGigHitbox(ImDrawListPtr drawList)
    {
        if (!Service.Configuration.AutoGigDrawGigHitbox) 
            return;

        float startX = _uiSize.X / 2;
        float centerY = _addon->FishLines->Y * _uiScale;
        float endY = _addon->FishLines->Height * _uiScale;
 
        int space = Service.Configuration.GigSpacing[currentKey];    

        //Hitbox left
        var lineStart = _uiPos + new Vector2(startX - space, centerY);
        var lineEnd = lineStart + new Vector2(0, endY);
        drawList.AddLine(lineStart, lineEnd, 0xFF0000C0, 1 * ImGuiHelpers.GlobalScale);

        //Hitbox right
        lineStart = _uiPos + new Vector2(startX + space, centerY);
        lineEnd = lineStart + new Vector2(0, endY);
        drawList.AddLine(lineStart, lineEnd, 0xFF0000C0, 1 * ImGuiHelpers.GlobalScale);
    }

    private unsafe void DrawFishHitbox(ImDrawListPtr drawList, float fishHitbox)
    {
        if (!Service.Configuration.AutoGigDrawFishHitbox)
            return;

        var lineStart = _uiPos + new Vector2(fishHitbox, _addon->FishLines->Y * _uiScale);
        var lineEnd = lineStart + new Vector2(0, _addon->FishLines->Height * _uiScale);
        drawList.AddLine(lineStart, lineEnd, 0xFF20B020, 1 * ImGuiHelpers.GlobalScale);
    }

    public override unsafe bool DrawConditions()
    {
        _addon = (SpearfishWindow*)Service.GameGui.GetAddonByName("SpearFishing", 1);

        bool _isOpen = _addon != null && _addon->Base.WindowNode != null;

        if (!_isOpen)
            return false;

        return true;
    }

    public override unsafe void PreDraw()
    {
        _uiScale = _addon->Base.Scale;
        _uiPos = new Vector2(_addon->Base.X, _addon->Base.Y);
        _uiSize = new Vector2(_addon->Base.WindowNode->AtkResNode.Width * _uiScale,
            _addon->Base.WindowNode->AtkResNode.Height * _uiScale);

        Position = _uiPos;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = _uiSize,
            MaximumSize = Vector2.One * 10000,
        };
    }
}