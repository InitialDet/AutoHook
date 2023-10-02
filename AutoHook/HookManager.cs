using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AutoHook.Configurations;
using AutoHook.Data;
using AutoHook.Enums;
using AutoHook.Resources.Localization;
using AutoHook.Utils;
using Dalamud.Game;
using Dalamud.Logging;
using Parser;
using Item = Lumina.Excel.GeneratedSheets.Item;

namespace AutoHook;

public class HookingManager : IDisposable
{
    private BaitConfig? _selectedPreset;

    private static readonly Configuration Cfg = Service.Configuration;

    private readonly FishingParser _parser = new();
    private CatchSteps _lastStep = 0;
    private FishingState _lastState = FishingState.None;
    private readonly Stopwatch _timer = new();

    public static string? LastCatch { get; private set; } = @"-";
        
    public static string? CurrentBait { get; private set; } = @"-";

    public HookingManager()
    {
        Enable();
    }

    private void Enable()
    {
        SubscribeToParser();
        Service.Framework.Update += OnFrameworkUpdate;
    }

    private void SubscribeToParser()
    {
        _parser.Enable();
        _parser.CaughtFish += OnCatch;
        _parser.BeganFishing += OnBeganFishing;
        _parser.BeganMooching += OnBeganMooch;
    }

    public void Dispose()
    {
        Disable();
        _parser.Dispose();
    }

    private void Disable()
    {
        UnSubscribeToParser();
        Service.Framework.Update -= OnFrameworkUpdate;
    }

    private void UnSubscribeToParser()
    {
        _parser.Disable();
        _parser.CaughtFish -= OnCatch;
        _parser.BeganFishing -= OnBeganFishing;
        _parser.BeganMooching -= OnBeganMooch;
    }

    private static string GetCurrentBait()
    {
        var baitId = Service.CurrentBait.Current;
        var baitName = MultiString.ParseSeStringLumina(Service.DataManager.GetExcelSheet<Item>()!.GetRow(baitId)?.Name);
        return baitName;
    }

    // The current config is updates two times: When we began fishing (to get the config based on the mooch/bait) and when we hooked the fish (in case the user updated their configs).
    private void UpdateCurrentSetting()
    {
        ResetAFKTimer();

        _selectedPreset = GetPreset(CurrentBait);

        if (_selectedPreset == null)
        {
            BaitConfig defaultConfig;

            if (_lastStep == CatchSteps.BeganMooching)
                defaultConfig = Cfg.DefaultMoochConfig;
            else
                defaultConfig = Cfg.DefaultCastConfig;

            if (defaultConfig.Enabled)
                _selectedPreset = defaultConfig;
        }

        else if (!_selectedPreset.Enabled)
            _selectedPreset = null;

        if (_selectedPreset == null)
            PluginLog.Debug(@"No config found. Not hooking");
        else
            PluginLog.Debug($"Preset Found: {Cfg.CurrentPreset?.PresetName}, Bait: {_selectedPreset.BaitName}");
    }

    private static BaitConfig? GetPreset(string? baitName)
    {
        return Cfg.CurrentPreset?.ListOfBaits.FirstOrDefault(mooch =>
            mooch.BaitName.ToLower().Equals(baitName?.ToLower()));
    }

    private void OnBeganFishing()
    {
        if (_lastStep == CatchSteps.BeganFishing && _lastState != FishingState.PoleReady)
            return;

        CurrentBait = GetCurrentBait();
        _timer.Reset();
        _timer.Start();
        _lastStep = CatchSteps.BeganFishing;
        UpdateCurrentSetting();
    }

    private void OnBeganMooch()
    {
        if (_lastStep == CatchSteps.BeganMooching && _lastState != FishingState.PoleReady)
            return;

        CurrentBait = new string(LastCatch);
        _timer.Reset();
        _timer.Start();
        //LastCatch = null;
        _lastStep = CatchSteps.BeganMooching;
        UpdateCurrentSetting();
    }

    private void OnBite()
    {
        UpdateCurrentSetting();
        _lastStep = CatchSteps.FishBit;
        _timer.Stop();

        HookFish(Service.TugType?.Bite ?? BiteType.Unknown);
    }

    private void OnCatch(string fishName, uint fishId)
    {
        LastCatch = fishName;

        CurrentBait = GetCurrentBait();

        PluginLog.Debug($"{fishName} (id {fishId}) hooked");

        _lastStep = CatchSteps.FishCaught;

        // Check if should stop with the current bait/fish
        if (_selectedPreset != null && _selectedPreset.StopAfterCaught)
        {
            int total = FishCounter.Add(_selectedPreset.BaitName);

            PluginLog.Debug(
                $"{_selectedPreset.BaitName} caught. Total: {total} out of {_selectedPreset.StopAfterCaughtLimit}");

            if (total >= _selectedPreset.StopAfterCaughtLimit)
            {
                _lastStep = CatchSteps.Quitting;
            }
        }

        // Check if should stop with another bait/fish
        var customMoochCfg = GetPreset(LastCatch);
        if (customMoochCfg != null && customMoochCfg.StopAfterCaught)
        {
            int total = FishCounter.Add(customMoochCfg.BaitName);

            PluginLog.Debug(
                $"{customMoochCfg.BaitName} caught. Total: {total} out of {customMoochCfg.StopAfterCaughtLimit}");

            if (total >= customMoochCfg.StopAfterCaughtLimit)
            {
                _lastStep = CatchSteps.Quitting;
            }
        }
    }


    private void OnFrameworkUpdate(Framework _)
    {
        var state = Service.EventFramework.FishingState;

        if (!Cfg.PluginEnabled || state == FishingState.None)
            return;

        if (state != FishingState.Quit && _lastStep == CatchSteps.Quitting)
        {
            PlayerResources.CastActionDelayed(IDs.Actions.Quit);
            state = FishingState.Quit;
        }

        //CheckState();

        // FishBit in this case means that the fish was hooked, but it escaped. I might need to find a way to check if the fish was caught or not.
        if (_lastStep != CatchSteps.Quitting && state == FishingState.PoleReady && (_lastStep == CatchSteps.FishBit ||
                _lastStep == CatchSteps.FishCaught || _lastStep == CatchSteps.TimeOut))
        {
            UseAutoCasts();
        }

        //CheckState();
        if (state == FishingState.Waiting2)
            CheckMaxTimeLimit();

        if (_lastState == state)
            return;

        _lastState = state;

        switch (state)
        {
            case FishingState.PullPoleIn: // If a hook is manually used before a bite, dont use auto cast
                if (_lastStep == CatchSteps.BeganFishing || _lastStep == CatchSteps.BeganFishing)
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

    private void OnFishingStop()
    {
        _lastStep = CatchSteps.None;
        if (_timer.IsRunning)
        {
            _timer.Stop();
        }

        CurrentBait = @"-";

        FishCounter.Reset();
        PlayerResources.ResetAutoCast();
    }

    private unsafe void HookFish(BiteType bite)
    {
        if (_selectedPreset == null)
            return;

        // Check if the minimum time has passed
        if (!CheckMinTimeLimit())
            return;

        HookType? hook;

        if (PlayerResources.HasStatus(IDs.Status.IdenticalCast))
        {
            var identicalCast = GetPreset(LastCatch);
            if (identicalCast != null)
                hook = identicalCast.GetHookIgnoreEnable(bite);
            else
                hook = _selectedPreset.GetHook(bite);
        }
        else
            hook = _selectedPreset.GetHook(bite);

        if (hook == null || hook == HookType.None)
            return;

        if (PlayerResources.ActionAvailable((uint)hook)) // Check if Powerful/Precision is available
            PlayerResources.CastActionDelayed((uint)hook);
        else // If not, use normal hook
            PlayerResources.CastActionDelayed((uint)HookType.Normal);
    }

    private static double _lastTickMs = 200;
    private Stopwatch _recastTimer = new();

    private void UseAutoCasts()
    {
        if (!_recastTimer.IsRunning)
            _recastTimer.Start();

        if (_recastTimer.ElapsedMilliseconds > _lastTickMs + 200)
        {
            _lastTickMs = _recastTimer.ElapsedMilliseconds;

            AutoCast? cast;

            var customMoochCfg = GetPreset(LastCatch);

            if (customMoochCfg != null)
                cast = Cfg.AutoCastsCfg.GetNextAutoCast(customMoochCfg);
            else
                cast = Cfg.AutoCastsCfg.GetNextAutoCast(_selectedPreset);

            if (cast != null)
            {
                PlayerResources.CastActionDelayed(cast.Id, cast.ActionType);
            }
        }
    }

    private void QuitFishing()
    {
        PlayerResources.CastActionDelayed(IDs.Actions.Quit);
    }

    private bool CheckMinTimeLimit()
    {
        if (_selectedPreset == null)
            return true;

        double minTime;

        if (_selectedPreset.UseChumTimer && PlayerResources.HasStatus(IDs.Status.Chum))
        {
            minTime = Math.Truncate(_selectedPreset.MinChumTimeDelay * 100) / 100;
        }
        else
            minTime = Math.Truncate(_selectedPreset.MinTimeDelay * 100) / 100;

        double timeElapsed = Math.Truncate((_timer.ElapsedMilliseconds / 1000.0) * 100) / 100;
        if (minTime > 0 && timeElapsed < minTime)
        {
            _lastStep = CatchSteps.TimeOut;
            return false;
        }

        return true;
    }

    private void CheckMaxTimeLimit()
    {
        if (_selectedPreset == null)
            return;

        double maxTime;

        if (_selectedPreset.UseChumTimer && PlayerResources.HasStatus(IDs.Status.Chum))
        {
            PluginLog.Debug(UIStrings.Using_Chum_Timer);
            maxTime = Math.Truncate(_selectedPreset.MaxChumTimeDelay * 100) / 100;
        }
        else
            maxTime = Math.Truncate(_selectedPreset.MaxTimeDelay * 100) / 100;

        double currentTime = Math.Truncate((_timer.ElapsedMilliseconds / 1000.0) * 100) / 100;

        if (maxTime > 0 && currentTime > maxTime && _lastStep != CatchSteps.TimeOut)
        {
            PluginLog.Debug("Timeout. Hooking fish.");
            _lastStep = CatchSteps.TimeOut;
            PlayerResources.CastActionDelayed(IDs.Actions.Hook);
        }
    }

    public static void ResetAFKTimer()
    {
        if (!InputUtil.TryFindGameWindow(out var windowHandle)) return;

        // Virtual key for Right Winkey. Can't be used by FFXIV normally, and in tests did not seem to cause any
        // unusual interference.
        InputUtil.SendKeycode(windowHandle, 0x5C);
    }

    private static double _debugValueLast = 3000;
    
    private readonly Stopwatch _timerState = new();

    private void CheckState()
    {
        if (!_timerState.IsRunning)
            _timerState.Start();

        if (_timerState.ElapsedMilliseconds > _debugValueLast + 500)
        {
            _debugValueLast = _timerState.ElapsedMilliseconds;
            PluginLog.Debug($"Fishing State: {Service.EventFramework.FishingState}, LastStep: {_lastStep}");
        }
    }

    private static class FishCounter
    {
        static Dictionary<string, int> _fishCount = new();

        public static int Add(string fishName)
        {
            if (!_fishCount.ContainsKey(fishName))
                _fishCount.Add(fishName, 0);
            _fishCount[fishName]++;

            return GetCount(fishName);
        }

        public static int GetCount(string fishName)
        {
            if (!_fishCount.ContainsKey(fishName))
                return 0;
            return _fishCount[fishName];
        }

        public static void Reset()
        {
            _fishCount = new();
        }
    }
    
    // Get current bait and check the last catch
    
}