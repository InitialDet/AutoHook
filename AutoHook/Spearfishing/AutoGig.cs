using AutoHook.Data;
using AutoHook.Utils;
using Dalamud.Hooking;
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
using System.Text;
using System.Threading.Tasks;

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


    List<SpearfishSpeed> speedTypes = Enum.GetValues(typeof(SpearfishSpeed)).Cast<SpearfishSpeed>().ToList();
    List<SpearfishSize> sizeTypes = Enum.GetValues(typeof(SpearfishSize)).Cast<SpearfishSize>().ToList();

    public AutoGig() : base("SpearfishingHelper", WindowFlags, true)
    {
        Service.WindowSystem.AddWindow(this);
        IsOpen = true;
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

    public void DrawSettings()
    {
        if (ImGui.Checkbox("Enable AutoGig ", ref Service.Configuration.AutoGigEnabled))
        {
            Service.Configuration.Save();
        }

        ImGui.SameLine();

        if (ImGui.Checkbox("Use Nature's Bounty ", ref Service.Configuration.AutoGigNaturesBountyEnabled))
        {
            Service.Configuration.Save();
        }
        ImGui.SameLine();

        ShowKofi();

        ImGui.SetNextItemWidth(130);
        if (ImGui.BeginCombo("Size", Service.Configuration.currentSize.ToName()))
        {

            foreach (SpearfishSize size in sizeTypes.Where(size =>
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
            foreach (SpearfishSpeed speed in speedTypes.Where(speed =>
                        ImGui.Selectable(speed.ToName(), speed == Service.Configuration.currentSpeed)))
            {
                Service.Configuration.currentSpeed = speed;
            }
            ImGui.EndCombo();
        }
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

    private unsafe void DrawFishOverlay()
    {
        
        _addon = (SpearfishWindow*)Service.GameGui.GetAddonByName("SpearFishing", 1);

        bool _isOpen = _addon != null && _addon->Base.WindowNode != null;

        if (!_isOpen)
            return;

        _uiScale = _addon->Base.Scale;
        _uiPos = new Vector2(_addon->Base.X, _addon->Base.Y);
        _uiSize = new Vector2(_addon->Base.WindowNode->AtkResNode.Width * _uiScale, _addon->Base.WindowNode->AtkResNode.Height * _uiScale);

        ImGui.SetNextWindowPos(new Vector2(_addon->Base.X + 5, _addon->Base.Y - 65));
        if (ImGui.Begin("gig", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar))
        {
            DrawSettings();
        }

        if (Service.Configuration.AutoGigEnabled)
        {
            if (!PlayerResources.HasStatus(IDs.Status.NaturesBounty) && Service.Configuration.AutoGigNaturesBountyEnabled)
                PlayerResources.CastActionDelayed(IDs.Actions.NaturesBounty);

            GigFish(_addon->Fish1, _addon->Fish1Node);
            GigFish(_addon->Fish2, _addon->Fish2Node);
            GigFish(_addon->Fish3, _addon->Fish3Node);
        }
    }

    private unsafe void GigFish(SpearfishWindow.Info info, AtkResNode* node)
    {
        if (!info.Available)
            return;

        var currentSize = Service.Configuration.currentSize;
        var currentSpeed = Service.Configuration.currentSpeed;
        var gig = (info.Size == currentSize || currentSize == SpearfishSize.All) &&
                  (info.Speed == currentSpeed || currentSpeed == SpearfishSpeed.All);

        if (!gig)
            return;

        var fixedx = (_uiSize.X / 2);
        float newx = 0;

        if (node->GetScaleX() == -1)
            newx = (node->X * _uiScale) - (node->Width / 2);
        else
            newx = (node->X * _uiScale) + (node->Width / 2);

        if (newx <= fixedx + 25 && newx >= fixedx - 25)
        {
            PlayerResources.CastActionNoDelay(IDs.Actions.Gig);
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct ActionEffectHeader
    {
        [FieldOffset(0x0)] public long TargetObjectId;
        [FieldOffset(0x8)] public uint ActionId;
        [FieldOffset(0x14)] public uint UnkObjectId;
        [FieldOffset(0x18)] public ushort Sequence;
        [FieldOffset(0x1A)] public ushort Unk_1A;
    }


}