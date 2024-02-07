using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoHook.Classes;
using AutoHook.Classes.AutoCasts;
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

    private FishingState _lastState = FishingState.None;
    private FishingSteps _lastStep = 0;

    private HookConfig? _currentHook;

    //private FishConfig? _lastFishCatchCfg;
    private BaitFishClass? _lastCatch;

    private IntuitionStatus _intuitionStatus = IntuitionStatus.NotActive;
    private SpectralCurrentStatus _spectralCurrentStatus = SpectralCurrentStatus.NotActive;

    private delegate bool UseActionDelegate(IntPtr manager, ActionType actionType, uint actionId, GameObjectID targetId,
        uint a4, uint a5,
        uint a6, IntPtr a7);

    private Hook<UseActionDelegate>? _useActionHook;

    public HookingManager()
    {
        CreateDalamudHooks();
        Enable();
    }

    //public static BaitFishClass LastCatch { get; private set; } = new(@"-", -1);

    public static string CurrentBaitMooch { get; private set; } = @"-";

    public void Dispose()
    {
        Disable();

        _catchHook?.Dispose();
        _useActionHook?.Dispose();
    }

    private unsafe void CreateDalamudHooks()
    {
        _catchHook = new UpdateFishCatch(Service.SigScanner).CreateHook(OnCatchUpdate);
        var hookPtr = (IntPtr)ActionManager.MemberFunctionPointers.UseAction;
        _useActionHook = Service.GameInteropProvider.HookFromAddress<UseActionDelegate>(hookPtr, OnUseAction);
    }

    private void Enable()
    {
        Service.Framework.Update += OnFrameworkUpdate;
        _catchHook?.Enable();
        _useActionHook?.Enable();
    }

    private void Disable()
    {
        Service.Framework.Update -= OnFrameworkUpdate;
        _useActionHook?.Disable();
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
        var customHook = _lastStep == FishingSteps.BeganMooching
            ? Presets.SelectedPreset?.GetMoochByName(CurrentBaitMooch)
            : Presets.SelectedPreset?.GetBaitByName(CurrentBaitMooch);

        var defaultHook = _lastStep == FishingSteps.BeganMooching
            ? Presets.DefaultPreset.ListOfMooch.FirstOrDefault()
            : Presets.DefaultPreset.ListOfBaits.FirstOrDefault();

        _currentHook = customHook?.Enabled ?? false ? customHook
            : defaultHook?.Enabled ?? false ? defaultHook
            : null;

        var presetName = customHook?.Enabled ?? false ? Presets.SelectedPreset?.PresetName ?? @"Custom"
            : defaultHook?.Enabled ?? false ? Presets.DefaultPreset.PresetName
            : @"";

        var autoCastName = Presets.SelectedPreset?.AutoCastsCfg.EnableAll ?? false ? Presets.SelectedPreset.PresetName
            : Presets.DefaultPreset?.AutoCastsCfg.EnableAll ?? false ? Presets.DefaultPreset.PresetName
            : "-";

        var extraCfg = Presets.SelectedPreset?.ExtraCfg.Enabled ?? false ? Presets.SelectedPreset.PresetName
            : Presets.DefaultPreset?.ExtraCfg.Enabled ?? false ? Presets.DefaultPreset.PresetName
            : "-";

        Service.Status = _currentHook == null
            ? @"-"
            : @$"Hook Config: {_currentHook.BaitFish.Name}({presetName}) | AutoCast: {autoCastName} | Extra: {extraCfg}";

        Service.PrintDebug(_currentHook == null
            ? @$"[HookManager] No config found. Not hooking"
            : @$"[HookManager] Config Found: {_currentHook.BaitFish.Name}, Preset: {presetName}");
    }

    private AutoCastsConfig GetAutoCastCfg()
    {
        return Presets.SelectedPreset?.AutoCastsCfg.EnableAll ?? false
            ? Presets.SelectedPreset.AutoCastsCfg
            : Presets.DefaultPreset.AutoCastsCfg;
    }

    private ExtraConfig GetExtraCfg()
    {
        return Presets.SelectedPreset?.ExtraCfg.Enabled ?? false
            ? Presets.SelectedPreset.ExtraCfg
            : Presets.DefaultPreset.ExtraCfg;
    }

    private FishConfig? GetLastCatchConfig()
    {
        if (_lastCatch == null) 
            return null;

        return Presets.SelectedPreset?.GetFishById(_lastCatch.Id) ?? Presets.DefaultPreset.GetFishById(_lastCatch.Id);
    }

    private void OnFrameworkUpdate(IFramework _)
    {
        var currentState = Service.EventFramework.FishingState;

        if (!Service.Configuration.PluginEnabled || currentState == FishingState.None)
            return;

        if (currentState != FishingState.Quit && _lastStep == FishingSteps.Quitting)
        {
            if (PlayerResources.IsCastAvailable())
            {
                PlayerResources.CastActionDelayed(IDs.Actions.Quit, ActionType.Action, @"Quit");
                currentState = FishingState.Quit;
            }
        }

        //CheckState();

        if (_lastStep != FishingSteps.Quitting && currentState == FishingState.PoleReady &&
            _lastStep == FishingSteps.FishCaught)
            CheckStopCondition();

        if (_lastStep != FishingSteps.Quitting && currentState == FishingState.PoleReady)
            UseAutoCasts();

        if (currentState == FishingState.Waiting2)
            CheckMaxTimeLimit();

        if (_lastState == currentState)
            return;
        
        if (currentState == FishingState.PoleReady)
            Service.Status = "-";
        
        _lastState = currentState;

        switch (currentState)
        {
            case FishingState.PullPoleIn: // If a hook is manually used before a bite, dont use auto cast
                if (_lastStep is FishingSteps.BeganFishing or FishingSteps.BeganMooching)
                    _lastStep = FishingSteps.None;
                break;
            case FishingState.Bite:
                if (_lastStep != FishingSteps.FishBit) OnBite();
                break;
            case FishingState.Quit:
                OnFishingStop();
                break;
        }
    }

    private void UseAutoCasts()
    {
        // if _lastStep is FishBit but currentState is FishingState.PoleReady, it case means that the fish was hooked, but it escaped.
        if (_lastStep is FishingSteps.None or FishingSteps.BeganFishing or FishingSteps.BeganMooching)
            return;

        if (!_recastTimer.IsRunning)
            _recastTimer.Start();

        // only try to auto cast every 500ms
        if (!(_recastTimer.ElapsedMilliseconds > _lastTickMs + 500))
            return;

        _lastTickMs = _recastTimer.ElapsedMilliseconds;

        if (!PlayerResources.IsCastAvailable())
            return;

        CheckExtraActions();

        var acCfg = GetAutoCastCfg();

        var cast = GetFishCaughActions() ?? GetNextAutoCast(acCfg);

        if (cast != null)
            PlayerResources.CastActionDelayed(cast.Id, cast.ActionType, cast.Name);
        else
            CastLineMoochOrRelease(acCfg);
    }

    private BaseActionCast? GetFishCaughActions()
    {
        var lastFishCatchCfg = GetLastCatchConfig();

        if (lastFishCatchCfg == null || !lastFishCatchCfg.Enabled)
            return null;

        if (lastFishCatchCfg.IdenticalCast.IsAvailableToCast())
            return lastFishCatchCfg.IdenticalCast;

        if (lastFishCatchCfg.SurfaceSlap.IsAvailableToCast())
            return lastFishCatchCfg.SurfaceSlap;

        var count = FishingCounter.GetCount(lastFishCatchCfg.GetUniqueId());
        if (_lastStep != FishingSteps.BaitSwapped && lastFishCatchCfg.SwapBait)
        {
            if (count == lastFishCatchCfg.SwapBaitCount &&
                lastFishCatchCfg.BaitToSwap.Id != Service.EquipedBait.Current)
            {
                var result = Service.EquipedBait.ChangeBait(lastFishCatchCfg.BaitToSwap);

                _lastStep |= FishingSteps.BaitSwapped; // one try per catch

                if (result == CurrentBait.ChangeBaitReturn.Success)
                {
                    Service.PrintChat(@$"[Fish] Swapping bait to {lastFishCatchCfg.BaitToSwap.Name}");
                    Service.Save();
                }
            }
        }

        if (_lastStep != FishingSteps.PresetSwapped && lastFishCatchCfg.SwapPresets)
        {
            if (count == lastFishCatchCfg.SwapPresetCount &&
                lastFishCatchCfg.PresetToSwap != Presets.SelectedPreset?.PresetName)
            {
                var preset =
                    Presets.CustomPresets.FirstOrDefault(preset => preset.PresetName == lastFishCatchCfg.PresetToSwap);
                _lastStep |= FishingSteps.PresetSwapped; // one try per catch

                if (preset != null)
                {
                    Presets.SelectedPreset = preset;
                    Service.PrintChat(@$"[Fish] Swapping current preset to {lastFishCatchCfg.PresetToSwap}");
                    Service.Save();
                }
                else
                    Service.PrintChat(@$"Preset {lastFishCatchCfg.PresetToSwap} not found.");
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

    private void CheckExtraActions()
    {
        if (_intuitionStatus == IntuitionStatus.NotActive)
        {
            if (!PlayerResources.HasStatus(IDs.Status.FishersIntuition))
                return;

            _intuitionStatus = IntuitionStatus.Active; // only one try

            var extraCfg = GetExtraCfg();

            if (extraCfg.SwapPresetIntuitionGain)
            {
                var preset =
                    Presets.CustomPresets.FirstOrDefault(preset =>
                        preset.PresetName == extraCfg.PresetToSwapIntuitionGain);

                if (preset != null)
                {
                    Presets.SelectedPreset = preset;
                    Service.PrintChat(@$"[Extra] Swapping current preset to {extraCfg.PresetToSwapIntuitionGain}");
                    Service.Save();
                }
                else
                    Service.PrintChat(@$"Preset {extraCfg.PresetToSwapIntuitionGain} not found.");
            }

            if (extraCfg.SwapBaitIntuitionGain)
            {
                var result = Service.EquipedBait.ChangeBait(extraCfg.BaitToSwapIntuitionGain);

                if (result == CurrentBait.ChangeBaitReturn.Success)
                {
                    Service.PrintChat(@$"[Extra] Swapping bait to {extraCfg.BaitToSwapIntuitionGain.Name}");
                    _lastStep |= FishingSteps.BaitSwapped; // one try per catch
                    Service.Save();
                }
            }
        }

        if (_intuitionStatus == IntuitionStatus.Active)
        {
            if (PlayerResources.HasStatus(IDs.Status.FishersIntuition))
                return;

            _intuitionStatus = IntuitionStatus.NotActive; // only one try

            var extraCfg = GetExtraCfg();

            if (extraCfg.SwapPresetIntuitionLost)
            {
                var preset =
                    Presets.CustomPresets.FirstOrDefault(preset =>
                        preset.PresetName == extraCfg.PresetToSwapIntuitionLost);

                if (preset != null)
                {
                    // one try per catch
                    _lastStep |= FishingSteps.PresetSwapped;
                    Presets.SelectedPreset = preset;
                    Service.PrintChat(@$"[Extra] Swapping current preset to {extraCfg.PresetToSwapIntuitionLost}");
                    Service.Save();
                }
                else
                    Service.PrintChat(@$"Preset {extraCfg.PresetToSwapIntuitionLost} not found.");
            }

            if (extraCfg.SwapBaitIntuitionLost)
            {
                var result = Service.EquipedBait.ChangeBait(extraCfg.BaitToSwapIntuitionLost);

                // one try per catch
                _lastStep |= FishingSteps.BaitSwapped;
                if (result == CurrentBait.ChangeBaitReturn.Success)
                {
                    Service.PrintChat(@$"[Extra] Swapping bait to {extraCfg.BaitToSwapIntuitionLost.Name}");
                    Service.Save();
                }
            }
        }

        if (_spectralCurrentStatus == SpectralCurrentStatus.NotActive)
        {
            if (!PlayerResources.IsInActiveSpectralCurrent())
                return;

            _spectralCurrentStatus = SpectralCurrentStatus.Active; // only one try

            var extraCfg = GetExtraCfg();

            if (extraCfg.SwapPresetSpectralCurrentGain)
            {
                var preset =
                    Presets.CustomPresets.FirstOrDefault(preset =>
                        preset.PresetName == extraCfg.PresetToSwapSpectralCurrentGain);

                if (preset != null)
                {
                    Presets.SelectedPreset = preset;
                    Service.PrintChat(@$"[Extra] Swapping current preset to {extraCfg.PresetToSwapSpectralCurrentGain}");
                    Service.Save();
                }
                else
                    Service.PrintChat(@$"Preset {extraCfg.PresetToSwapSpectralCurrentGain} not found.");
            }

            if (extraCfg.SwapBaitSpectralCurrentGain)
            {
                var result = Service.EquipedBait.ChangeBait(extraCfg.BaitToSwapSpectralCurrentGain);

                if (result == CurrentBait.ChangeBaitReturn.Success)
                {
                    Service.PrintChat(@$"[Extra] Swapping bait to {extraCfg.BaitToSwapSpectralCurrentGain.Name}");
                    _lastStep |= FishingSteps.BaitSwapped; // one try per catch
                    Service.Save();
                }
            }
        }

        if (_spectralCurrentStatus == SpectralCurrentStatus.Active)
        {
            if (PlayerResources.IsInActiveSpectralCurrent())
                return;

            _spectralCurrentStatus = SpectralCurrentStatus.NotActive; // only one try

            var extraCfg = GetExtraCfg();

            if (extraCfg.SwapPresetSpectralCurrentLost)
            {
                var preset =
                    Presets.CustomPresets.FirstOrDefault(preset =>
                        preset.PresetName == extraCfg.PresetToSwapSpectralCurrentLost);

                if (preset != null)
                {
                    // one try per catch
                    _lastStep |= FishingSteps.PresetSwapped;
                    Presets.SelectedPreset = preset;
                    Service.PrintChat(@$"[Extra] Swapping current preset to {extraCfg.SwapPresetSpectralCurrentLost}");
                    Service.Save();
                }
                else
                    Service.PrintChat(@$"Preset {extraCfg.SwapPresetSpectralCurrentLost} not found.");
            }

            if (extraCfg.SwapBaitSpectralCurrentLost)
            {
                var result = Service.EquipedBait.ChangeBait(extraCfg.BaitToSwapSpectralCurrentLost);

                // one try per catch
                _lastStep |= FishingSteps.BaitSwapped;
                if (result == CurrentBait.ChangeBaitReturn.Success)
                {
                    Service.PrintChat(@$"[Extra] Swapping bait to {extraCfg.BaitToSwapSpectralCurrentLost.Name}");
                    Service.Save();
                }
            }
        }
    }

    private void CastLineMoochOrRelease(AutoCastsConfig acCfg)
    {
        var lastFishCatchCfg = GetLastCatchConfig();
        
        var blockMooch = lastFishCatchCfg is { Enabled: true, NeverMooch: true };

        if (!blockMooch)
        {
            if (lastFishCatchCfg is { Enabled: true } && lastFishCatchCfg.Mooch.IsAvailableToCast())
            {
                PlayerResources.CastActionDelayed(lastFishCatchCfg.Mooch.Id, lastFishCatchCfg.Mooch.ActionType,
                    UIStrings.Mooch);
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

    private void CheckStopCondition()
    {
        var lastFishCatchCfg = GetLastCatchConfig();

        if (lastFishCatchCfg?.StopAfterCaught ?? false)
        {
            var guid = lastFishCatchCfg.GetUniqueId();
            var total = FishingCounter.GetCount(guid);

            if (total >= lastFishCatchCfg.StopAfterCaughtLimit)
            {
                Service.PrintChat(string.Format(UIStrings.Caught_Limited_Reached_Chat_Message,
                    @$"{lastFishCatchCfg.Fish.Name}: {lastFishCatchCfg.StopAfterCaughtLimit}"));

                _lastStep = lastFishCatchCfg.StopFishingStep;
                FishingCounter.Remove(guid);
            }
        }

        if (_currentHook?.StopAfterCaught ?? false)
        {
            var guid = _currentHook.GetUniqueId();
            var total = FishingCounter.GetCount(guid);

            if (total >= _currentHook.StopAfterCaughtLimit)
            {
                Service.PrintChat(string.Format(UIStrings.Hooking_Limited_Reached_Chat_Message,
                    @$"{_currentHook.BaitFish.Name}: {_currentHook.StopAfterCaughtLimit}"));

                _lastStep = _currentHook.StopFishingStep;
                FishingCounter.Remove(guid);
            }
        }
    }

    private void OnBeganFishing()
    {
        if (_lastStep == FishingSteps.BeganFishing && _lastState != FishingState.PoleReady)
            return;

        CurrentBaitMooch = GetCurrentBait();
        _timer.Reset();
        _timer.Start();
        _lastStep = FishingSteps.BeganFishing;
        UpdateCurrentPreset();
    }

    private void OnBeganMooch()
    {
        if (_lastStep == FishingSteps.BeganMooching && _lastState != FishingState.PoleReady)
            return;

        CurrentBaitMooch = new string(Service.LastCatch.Name);
        _timer.Reset();
        _timer.Start();
        //LastCatch = null;
        _lastStep = FishingSteps.BeganMooching;
        UpdateCurrentPreset();
    }

    private void OnBite()
    {
        UpdateCurrentPreset();
        _lastStep = FishingSteps.FishBit;
        _timer.Stop();

        HookFish(Service.TugType?.Bite ?? BiteType.Unknown);
    }

    private async void HookFish(BiteType bite)
    {
        var delay = new Random().Next(Service.Configuration.DelayBetweenHookMin,
            Service.Configuration.DelayBetweenHookMax);
        await Task.Delay(delay);

        if (_currentHook == null)
            return;

        // Check if the minimum time has passed
        if (!CheckMinTimeLimit())
            return;

        var hook = _currentHook.GetHook(bite);

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

    private void OnCatch(uint fishId, uint amount)
    {
        _lastCatch = PlayerResources.Fishes.FirstOrDefault(fish => fish.Id == fishId) ?? new BaitFishClass(@"-", -1);
        var lastFishCatchCfg = GetLastCatchConfig();

        Service.LastCatch = _lastCatch;

        // Set the equipped bait back
        CurrentBaitMooch = GetCurrentBait();

        Service.PrintDebug(@$"[HookManager] Caught {_lastCatch.Name} (id {_lastCatch.Id})");

        _lastStep = FishingSteps.FishCaught;

        if (lastFishCatchCfg != null)
        {
            for (var i = 0; i < amount; i++)
            {
                FishingCounter.Add(lastFishCatchCfg.GetUniqueId());
            }
        }

        if (_currentHook != null)
            FishingCounter.Add(_currentHook.GetUniqueId());
    }

    private void OnFishingStop()
    {
        _lastStep = FishingSteps.None;

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

    private bool CheckMinTimeLimit()
    {
        if (_currentHook == null)
            return true;

        double minTime;

        if (_currentHook.UseChumTimer && PlayerResources.HasStatus(IDs.Status.Chum))
            minTime = Math.Truncate(_currentHook.MinChumTimeDelay * 100) / 100;
        else
            minTime = Math.Truncate(_currentHook.MinTimeDelay * 100) / 100;

        var timeElapsed = Math.Truncate(_timer.ElapsedMilliseconds / 1000.0 * 100) / 100;

        if (!(minTime > 0) || !(timeElapsed < minTime))
            return true;

        _lastStep = FishingSteps.TimeOut;

        return false;
    }

    private void CheckMaxTimeLimit()
    {
        if (_currentHook == null)
            return;

        double maxTime;

        if (_currentHook.UseChumTimer && PlayerResources.HasStatus(IDs.Status.Chum))
            maxTime = Math.Truncate(_currentHook.MaxChumTimeDelay * 100) / 100;
        else
            maxTime = Math.Truncate(_currentHook.MaxTimeDelay * 100) / 100;

        var currentTime = Math.Truncate(_timer.ElapsedMilliseconds / 1000.0 * 100) / 100;

        if (!(maxTime > 0) || !(currentTime > maxTime) || _lastStep == FishingSteps.TimeOut)
            return;

        Service.PrintDebug(@"[HookManager] Timeout. Hooking fish.");
        _lastStep = FishingSteps.TimeOut;
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

    private bool OnUseAction(IntPtr manager, ActionType actionType, uint actionId, GameObjectID targetId, uint a4,
        uint a5, uint a6, IntPtr a7)
    {
        try
        {
            if (actionType == ActionType.Action)
            {
                switch (actionId)
                {
                    case IDs.Actions.Cast:
                        if (PlayerResources.ActionTypeAvailable(actionId)) OnBeganFishing();
                        break;
                    case IDs.Actions.Mooch:
                    case IDs.Actions.Mooch2:
                        if (PlayerResources.ActionTypeAvailable(actionId)) OnBeganMooch();
                        break;
                }
            }
        }
        catch (Exception e)
        {
            Service.PrintDebug(@$"[HookManager] Error: {e.Message}");
        }

        return _useActionHook!.Original(manager, actionType, actionId, targetId, a4, a5, a6, a7);
    }

    private void OnCatchUpdate(IntPtr module, uint fishId, bool large, ushort size, byte amount, byte level, byte unk7,
        byte unk8, byte unk9, byte unk10, byte unk11, byte unk12)
    {
        _catchHook!.Original(module, fishId, large, size, amount, level, unk7, unk8, unk9, unk10, unk11, unk12);

        // Check against collectibles.
        if (fishId > 500000)
            fishId -= 500000;

        OnCatch(fishId, amount);
    }

    public static class FishingCounter
    {
        private static Dictionary<Guid, int> _fishCount = new();

        public static int Add(Guid guid)
        {
            _fishCount.TryAdd(guid, 0);
            _fishCount[guid]++;

            return GetCount(guid);
        }

        public static int GetCount(Guid fishName)
        {
            return !_fishCount.ContainsKey(fishName) ? 0 : _fishCount[fishName];
        }

        public static void Remove(Guid fishName)
        {
            if (_fishCount.ContainsKey(fishName))
                _fishCount.Remove(fishName);
        }

        public static void Reset()
        {
            _fishCount = new Dictionary<Guid, int>();
        }
    }
}