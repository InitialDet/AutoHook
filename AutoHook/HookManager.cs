using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AutoHook.Classes;
using AutoHook.Configurations;
using AutoHook.Data;
using AutoHook.Enums;
using AutoHook.Resources.Localization;
using AutoHook.SeFunctions;
using AutoHook.Utils;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using Lumina.Excel.GeneratedSheets;

namespace AutoHook;

public class HookingManager : IDisposable
{
    private static readonly HookPresets Presets = Service.Configuration.HookPresets;

    private static double _lastTickMs = 200;

    private static double _debugValueLast = 3000;
    private readonly Stopwatch _recastTimer = new();

    private readonly Stopwatch _timer = new();

    private readonly Stopwatch _timerState = new();

    private Hook<UpdateCatchDelegate>? _catchHook;
    private Hook<UseActionDelegate>? _hookHook;

    private FishingState _lastState = FishingState.None;
    private CatchSteps _lastStep = 0;

    //create a dictionary to receive a PresetConfig and HookConfig
    private (HookConfig? hook, string presetName) _currentHook = new();

    public HookingManager()
    {
        CreateDalamudHooks();
        Enable();
    }

    public static BaitFishClass LastCatch { get; private set; } = new(@"-", -1);

    public static string CurrentBaitMooch { get; private set; } = @"-";

    public void Dispose()
    {
        Disable();

        _catchHook?.Dispose();
        _hookHook?.Dispose();
    }

    private unsafe void CreateDalamudHooks()
    {
        _catchHook = new UpdateFishCatch(Service.SigScanner).CreateHook(OnCatchUpdate);
        var hookPtr = (IntPtr)ActionManager.MemberFunctionPointers.UseAction;
        _hookHook = Service.GameInteropProvider.HookFromAddress<UseActionDelegate>(hookPtr, OnUseAction);
    }

    private void Enable()
    {
        Service.Framework.Update += OnFrameworkUpdate;
        _catchHook?.Enable();
        _hookHook?.Enable();
    }

    private void Disable()
    {
        Service.Framework.Update -= OnFrameworkUpdate;
        _hookHook?.Disable();
        _catchHook?.Disable();
    }

    private static string GetCurrentBait()
    {
        var baitId = Service.EquipedBait.Current;
        var baitName = MultiString.ParseSeStringLumina(Service.DataManager.GetExcelSheet<Item>()!.GetRow(baitId)?.Name);
        return baitName;
    }

    // The current config is updates two times: When we began fishing (to get the config based on the mooch/bait) and when we hooked the fish (in case the user updated their configs).
    private void UpdateCurrentPreset()
    {
        ResetAfkTimer();

        // check if SelectedPreset has hook for the current bait
        var customHook = _lastStep == CatchSteps.BeganMooching
            ? Presets.SelectedPreset?.GetMoochByName(CurrentBaitMooch)
            : Presets.SelectedPreset?.GetBaitByName(CurrentBaitMooch);

        var defaultHook = _lastStep == CatchSteps.BeganMooching
            ? Presets.DefaultPreset.ListOfMooch.FirstOrDefault()
            : Presets.DefaultPreset.ListOfBaits.FirstOrDefault();

        _currentHook.hook = customHook?.Enabled ?? false ? customHook
            : defaultHook?.Enabled ?? false ? defaultHook
            : null;

        _currentHook.presetName = customHook?.Enabled ?? false ? Presets.SelectedPreset?.PresetName ?? @"Custom"
            : defaultHook?.Enabled ?? false ? Presets.DefaultPreset.PresetName
            : @"";
        
        Service.Status = _currentHook.hook == null
            ? @"-"
            : @$"Preset: {_currentHook.presetName} | Hook Config: {_currentHook.hook.BaitFish.Name}";
        
        Service.PrintDebug(_currentHook.hook == null
            ? @$"[HookManager] No config found. Not hooking"
            : @$"[HookManager] Config Found: {_currentHook.hook.BaitFish.Name}, Preset: {_currentHook.presetName}");
    }


    private AutoCastsConfig GetAutoCastCfg()
    {
        return Presets.SelectedPreset?.AutoCastsCfg.EnableAll ?? false
            ? Presets.SelectedPreset.AutoCastsCfg
            : Presets.DefaultPreset.AutoCastsCfg;
    }

    private (FishConfig, string)? GetFishConfig()
    {
        if (LastCatch == null)
            return null;

        var fish = Presets.SelectedPreset?.GetFishAndPresetById(LastCatch.Id) ??
                   Presets.DefaultPreset.GetFishAndPresetById(LastCatch.Id);

        return fish;
    }

    private void OnFrameworkUpdate(IFramework _)
    {
        var currentState = Service.EventFramework.FishingState;

        if (!Service.Configuration.PluginEnabled || currentState == FishingState.None)
            return;

        if (currentState != FishingState.Quit && _lastStep == CatchSteps.Quitting)
        {
            if (PlayerResources.IsCastAvailable())
            {
                PlayerResources.CastActionDelayed(IDs.Actions.Quit, ActionType.Action, @"Quit");
                currentState = FishingState.Quit;
            }
        }

        //CheckState();

        if (_lastStep != CatchSteps.Quitting && currentState == FishingState.PoleReady)
            UseAutoCasts();

        if (currentState == FishingState.Waiting2)
            CheckMaxTimeLimit();

        if (_lastState == currentState)
            return;

        _lastState = currentState;

        switch (currentState)
        {
            case FishingState.PullPoleIn: // If a hook is manually used before a bite, dont use auto cast
                if (_lastStep is CatchSteps.BeganFishing or CatchSteps.BeganMooching)
                    _lastStep = CatchSteps.None;
                break;
            case FishingState.Bite:
                if (_lastStep != CatchSteps.FishBit) OnBite();
                break;
            case FishingState.Quit:
                OnFishingStop();
                break;
        }
    }

    private void UseAutoCasts()
    {
        // if _lastStep is FishBit but currentState is FishingState.PoleReady, it case means that the fish was hooked, but it escaped.
        if (_lastStep is CatchSteps.None or CatchSteps.BeganFishing or CatchSteps.BeganMooching)
            return;

        if (!_recastTimer.IsRunning)
            _recastTimer.Start();

        // only try to auto cast every 500ms
        if (!(_recastTimer.ElapsedMilliseconds > _lastTickMs + 500))
            return;

        _lastTickMs = _recastTimer.ElapsedMilliseconds;

        if (!PlayerResources.IsCastAvailable())
            return;

        var fishCfg = GetFishConfig();
        var acCfg = GetAutoCastCfg();

        var cast = FishCaughtActions(fishCfg) ?? GetNextAutoCast(acCfg);

        if (cast != null)
            PlayerResources.CastActionDelayed(cast.Id, cast.ActionType, cast.Name);
        else
            CastLineOrMooch(fishCfg?.Item1, acCfg);
    }

    private BaseActionCast? FishCaughtActions((FishConfig, string)? fishAndPreset)
    {
        var fishCfg = fishAndPreset?.Item1;

        if (fishCfg == null || !fishCfg.Enabled) return null;

        if (!PlayerResources.IsCastAvailable()) return null;

        if (fishCfg.IdenticalCast.IsAvailableToCast())
            return fishCfg.IdenticalCast;

        if (fishCfg.SurfaceSlap.IsAvailableToCast())
            return fishCfg.SurfaceSlap;

        var count = FishingCounter.GetCount($"{fishCfg.Fish.Name} {fishAndPreset?.Item2}");
        if (fishCfg.SwapBait && fishCfg.BaitToSwap.Id != Service.EquipedBait.Current)
        {
            Service.PrintDebug($"{fishCfg.Fish.Name} {fishAndPreset?.Item2} {count}/{fishCfg.SwapBaitCount}");
            if (count >= fishCfg.SwapBaitCount)
            {
                var result = Service.EquipedBait.ChangeBait((uint)fishCfg.BaitToSwap.Id);

                if (result == CurrentBait.ChangeBaitReturn.Success)
                {
                    Service.PrintChat(@$"Swapping bait to {fishCfg.BaitToSwap.Name}");
                    Service.Save();
                }
            }
        }

        if (fishCfg.SwapPresets && fishCfg.PresetToSwap != Presets.SelectedPreset?.PresetName)
        {
            Service.PrintDebug($"{fishCfg.Fish.Name} {fishAndPreset?.Item2} {count}/{fishCfg.SwapPresetCount}");
            
            if (count >= fishCfg.SwapPresetCount)
            {
                var preset = Presets.CustomPresets.FirstOrDefault(preset => preset.PresetName == fishCfg.PresetToSwap);

                if (preset != null)
                {
                    Presets.SelectedPreset = preset;
                    Service.PrintChat(@$"Swapping current preset to {fishCfg.PresetToSwap}");
                    Service.Save();
                }
            }
        }

        return null;
    }

    private BaseActionCast? GetNextAutoCast(AutoCastsConfig acCfg)
    {
        if (!acCfg.EnableAll)
            return null;

        var autoActions = acCfg.GetAutoActions();

        foreach (var action in autoActions.Where(action => action.IsAvailableToCast()))
        {
            return action;
        }

        return null;
    }

    private void CastLineOrMooch(FishConfig? fishConfig, AutoCastsConfig acCfg)
    {
        var blockMooch = fishConfig is { Enabled: true, NeverMooch: true };


        if (!blockMooch)
        {
            if (fishConfig is { Enabled: true } && fishConfig.Mooch.IsAvailableToCast())
            {
                PlayerResources.CastActionDelayed(fishConfig.Mooch.Id, fishConfig.Mooch.ActionType, UIStrings.Mooch);
                return;
            }

            if (acCfg is { EnableAll: true } && acCfg.CastMooch.IsAvailableToCast())
            {
                PlayerResources.CastActionDelayed(acCfg.CastMooch.Id, acCfg.CastMooch.ActionType, UIStrings.Mooch);
                return;
            }
        }

        if (acCfg is { EnableAll: true } && acCfg.CastLine.IsAvailableToCast())
        {
            PlayerResources.CastActionDelayed(IDs.Actions.Cast, ActionType.Action, UIStrings.Cast_Line);
            return;
        }
    }

    private void OnBeganFishing()
    {
        if (_lastStep == CatchSteps.BeganFishing && _lastState != FishingState.PoleReady)
            return;

        CurrentBaitMooch = GetCurrentBait();
        _timer.Reset();
        _timer.Start();
        _lastStep = CatchSteps.BeganFishing;
        UpdateCurrentPreset();
    }

    private void OnBeganMooch()
    {
        if (_lastStep == CatchSteps.BeganMooching && _lastState != FishingState.PoleReady)
            return;

        CurrentBaitMooch = new string(LastCatch.Name);
        _timer.Reset();
        _timer.Start();
        //LastCatch = null;
        _lastStep = CatchSteps.BeganMooching;
        UpdateCurrentPreset();
    }

    private void OnBite()
    {
        UpdateCurrentPreset();
        _lastStep = CatchSteps.FishBit;
        _timer.Stop();

        HookFish(Service.TugType?.Bite ?? BiteType.Unknown);
    }

    private void OnCatch(uint fishId)
    {
        LastCatch = PlayerResources.Fishes.FirstOrDefault(fish => fish.Id == fishId) ?? new BaitFishClass(@"-", -1);

        CurrentBaitMooch = GetCurrentBait();

        Service.PrintDebug(@$"[HookManager] Caught {LastCatch.Name} (id {fishId})");

        _lastStep = CatchSteps.FishCaught;

        // check if should stop fishing 
        var fishCfg = Presets.SelectedPreset?.GetFishAndPresetById(LastCatch.Id) ?? Presets.DefaultPreset.GetFishAndPresetById(LastCatch.Id);

        if (fishCfg != null)
        {
            var name = @$"{fishCfg.Value.Item1.Fish.Name} {fishCfg.Value.Item2}";
            var total = FishingCounter.Add(name);

            if (total >= fishCfg.Value.Item1.StopAfterCaughtLimit && fishCfg.Value.Item1.StopAfterCaught)
            {
                Service.PrintChat(string.Format(UIStrings.Caught_Limited_Reached_Chat_Message, 
                    @$"{fishCfg.Value.Item1.Fish.Name}: {fishCfg.Value.Item1.StopAfterCaughtLimit}"));
                _lastStep = fishCfg.Value.Item1.StopFishingStep;
                FishingCounter.Remove(name);
            }
        }

        if (_currentHook.hook?.StopAfterCaught ?? false)
        {
            var name = @$"{_currentHook.hook.BaitFish.Name} {_currentHook.presetName}";
            var total = FishingCounter.Add(name);
            if (total >= _currentHook.hook.StopAfterCaughtLimit && _currentHook.hook.StopAfterCaught)
            {
                Service.PrintChat(string.Format(UIStrings.Hooking_Limited_Reached_Chat_Message, 
                    @$"{_currentHook.hook.BaitFish.Name}: {_currentHook.hook.StopAfterCaughtLimit}"));
                
                _lastStep = _currentHook.hook.StopFishingStep;
                
                FishingCounter.Remove(name);
            }
        }
    }

    private void OnFishingStop()
    {
        _lastStep = CatchSteps.None;

        if (_timer.IsRunning)
            _timer.Stop();

        if (_recastTimer.IsRunning)
            _recastTimer.Stop();

        if (_timerState.IsRunning)
            _timerState.Stop();

        CurrentBaitMooch = @"-";
        Service.Status = "-";

        FishingCounter.Reset();

        PlayerResources.CastActionNoDelay(IDs.Actions.Quit);
        PlayerResources.DelayNextCast(0);
        
        
    }

    private void HookFish(BiteType bite)
    {
        if (_currentHook.hook == null)
            return;

        // Check if the minimum time has passed
        if (!CheckMinTimeLimit())
            return;

        var hook = _currentHook.hook.GetHook(bite);

        if (hook is null or HookType.None)
            return;

        if (PlayerResources.ActionTypeAvailable((uint)hook)) // Check if Powerful/Precision is available
        {
            Service.PrintDebug(@$"[HookManager] Using {hook.ToString()} hook.");
            PlayerResources.CastActionDelayed((uint)hook, ActionType.Action, @$"{hook.ToString()}");
        }
        else
        {
            Service.PrintDebug(@"[HookManager] Powerful/Precision not available. Using normal hook.");
            PlayerResources.CastActionDelayed((uint)HookType.Normal, ActionType.Action,
                @$"{HookType.Normal.ToString()}");
        }
    }

    private bool CheckMinTimeLimit()
    {
        if (_currentHook.hook == null)
            return true;

        double minTime;

        if (_currentHook.hook.UseChumTimer && PlayerResources.HasStatus(IDs.Status.Chum))
            minTime = Math.Truncate(_currentHook.hook.MinChumTimeDelay * 100) / 100;
        else
            minTime = Math.Truncate(_currentHook.hook.MinTimeDelay * 100) / 100;

        var timeElapsed = Math.Truncate(_timer.ElapsedMilliseconds / 1000.0 * 100) / 100;

        if (!(minTime > 0) || !(timeElapsed < minTime))
            return true;

        _lastStep = CatchSteps.TimeOut;

        return false;
    }

    private void CheckMaxTimeLimit()
    {
        if (_currentHook.hook == null)
            return;

        double maxTime;

        if (_currentHook.hook.UseChumTimer && PlayerResources.HasStatus(IDs.Status.Chum))
            maxTime = Math.Truncate(_currentHook.hook.MaxChumTimeDelay * 100) / 100;
        else
            maxTime = Math.Truncate(_currentHook.hook.MaxTimeDelay * 100) / 100;

        var currentTime = Math.Truncate(_timer.ElapsedMilliseconds / 1000.0 * 100) / 100;

        if (!(maxTime > 0) || !(currentTime > maxTime) || _lastStep == CatchSteps.TimeOut)
            return;

        Service.PrintDebug(@"[HookManager] Timeout. Hooking fish.");
        _lastStep = CatchSteps.TimeOut;
        PlayerResources.CastActionDelayed(IDs.Actions.Hook, ActionType.Action, @"Hook");
    }

    private static void ResetAfkTimer()
    {
        if (!InputUtil.TryFindGameWindow(out var windowHandle)) return;

        // Virtual key for Right Winkey. Can't be used by FFXIV normally, and in tests did not seem to cause any
        // unusual interference.
        InputUtil.SendKeycode(windowHandle, 0x5C);
    }

    private void CheckState()
    {
        if (!_timerState.IsRunning)
            _timerState.Start();

        if (!(_timerState.ElapsedMilliseconds > _debugValueLast + 500))
            return;

        _debugValueLast = _timerState.ElapsedMilliseconds;
        Service.PrintDebug(
            @$"[HookManager] Fishing State: {Service.EventFramework.FishingState}, LastStep: {_lastStep}");
    }

    private delegate bool UseActionDelegate(IntPtr manager, ActionType actionType, uint actionId, GameObjectID targetId,
        uint a4, uint a5,
        uint a6, IntPtr a7);

    private bool OnUseAction(IntPtr manager, ActionType actionType, uint actionId, GameObjectID targetId, uint a4,
        uint a5, uint a6, IntPtr a7)
    {
        if (actionType == ActionType.Action && PlayerResources.ActionTypeAvailable(actionId))
            switch (actionId)
            {
                case IDs.Actions.Cast:
                    OnBeganFishing();
                    break;
                case IDs.Actions.Mooch:
                case IDs.Actions.Mooch2:
                    OnBeganMooch();
                    break;
            }

        return _hookHook!.Original(manager, actionType, actionId, targetId, a4, a5, a6, a7);
    }

    private void OnCatchUpdate(IntPtr module, uint fishId, bool large, ushort size, byte amount, byte level, byte unk7,
        byte unk8, byte unk9, byte unk10, byte unk11, byte unk12)
    {
        _catchHook!.Original(module, fishId, large, size, amount, level, unk7, unk8, unk9, unk10, unk11, unk12);

        // Check against collectibles.
        if (fishId > 500000)
            fishId -= 500000;

        OnCatch(fishId);
    }

    public static class FishingCounter
    {
        private static Dictionary<string, int> _fishCount = new();

        public static int Add(string fishName)
        {
            _fishCount.TryAdd(fishName, 0);
            _fishCount[fishName]++;

            foreach (var (key, value) in _fishCount)
            {
                Service.PrintDebug(@$"-----------[HookManager] {key}: {value}");
            }

            return GetCount(fishName);
        }

        public static int GetCount(string fishName)
        {
            return !_fishCount.ContainsKey(fishName) ? 0 : _fishCount[fishName];
        }
        
        public static void Remove(string fishName)
        {
            if (_fishCount.ContainsKey(fishName))
                _fishCount.Remove(fishName);
        }

        public static void Reset()
        {
            _fishCount = new Dictionary<string, int>();
        }
    }
}