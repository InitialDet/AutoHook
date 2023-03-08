using System;
using System.Diagnostics;
using System.Linq;
using AutoHook.Configurations;
using AutoHook.Utils;
using Dalamud.Game;
using Dalamud.Logging;
using Parser;
using Item = Lumina.Excel.GeneratedSheets.Item;
using AutoHook.Enums;
using AutoHook.Data;
using System.Collections.Generic;

namespace AutoHook.FishTimer;

public class HookingManager : IDisposable
{
    private BaitConfig? _selectedPreset;

    private static readonly Configuration cfg = Service.Configuration;

    private readonly FishingParser Parser = new();
    private CatchSteps LastStep = 0;
    private FishingState LastState = FishingState.None;
    private Stopwatch Timer = new();

    public static string? LastCatch = null;
    public static string? CurrentBait = null;

    public HookingManager()
    {
        Enable();
    }

    public void Enable()
    {  
        SubscribeToParser();
        Service.Framework.Update += OnFrameworkUpdate;
    }

    private void SubscribeToParser()
    {
        Parser.Enable();
        Parser.CaughtFish += OnCatch;
        Parser.BeganFishing += OnBeganFishing;
        Parser.BeganMooching += OnBeganMooch;
    }

    public void Dispose()
    {
        Disable();
        Parser.Dispose();
    }

    public void Disable()
    {
        UnSubscribeToParser();
        Service.Framework.Update -= OnFrameworkUpdate;
    }
    private void UnSubscribeToParser()
    {
        Parser.Disable();
        Parser.CaughtFish -= OnCatch;
        Parser.BeganFishing -= OnBeganFishing;
        Parser.BeganMooching -= OnBeganMooch;
    }

    public static string GetCurrentBait()
    {
        var baitId = Service.CurrentBait.Current;
        string baitName = MultiString.ParseSeStringLumina(Service.DataManager.GetExcelSheet<Item>()!.GetRow(baitId)?.Name);
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

            if (LastStep == CatchSteps.BeganMooching)
                defaultConfig = cfg.DefaultMoochConfig;
            else
                defaultConfig = cfg.DefaultCastConfig;

            if (defaultConfig.Enabled)
                _selectedPreset = defaultConfig;
        }

        else if (!_selectedPreset.Enabled)
            _selectedPreset = null;

        if (_selectedPreset == null)
            PluginLog.Debug("No config found. Not hooking");
        else
            PluginLog.Debug($"Preset Found: {cfg.CurrentPreset?.PresetName}, Bait: {_selectedPreset.BaitName}");
    }

    private static BaitConfig? GetPreset(string? baitName)
    {
        return cfg.CurrentPreset?.ListOfBaits.FirstOrDefault(mooch => mooch.BaitName.ToLower().Equals(baitName?.ToLower()));
    }

    private void OnBeganFishing()
    {
        if (LastStep == CatchSteps.BeganFishing && LastState != FishingState.PoleReady)
            return;

        CurrentBait = GetCurrentBait();
        Timer.Reset();
        Timer.Start();
        LastStep = CatchSteps.BeganFishing;
        UpdateCurrentSetting();
    }

    private void OnBeganMooch()
    {
        if (LastStep == CatchSteps.BeganMooching && LastState != FishingState.PoleReady)
            return;

        CurrentBait = new string(LastCatch);
        Timer.Reset();
        Timer.Start();
        //LastCatch = null;
        LastStep = CatchSteps.BeganMooching;
        UpdateCurrentSetting();
    }

    private void OnBite()
    {
        UpdateCurrentSetting();
        LastStep = CatchSteps.FishBit;
        Timer.Stop();

        HookFish(Service.TugType?.Bite ?? BiteType.Unknown);
    }

    private void OnCatch(string fishName, uint fishId)
    {
        LastCatch = fishName;
        CurrentBait = GetCurrentBait();

        LastStep = CatchSteps.FishCaught;

        // Check if should stop with the current bait/fish
        if (_selectedPreset != null && _selectedPreset.StopAfterCaught)
        {
            int total = FishCounter.Add(_selectedPreset.BaitName);

            PluginLog.Debug($"{_selectedPreset.BaitName} caught. Total: {total} out of {_selectedPreset.StopAfterCaughtLimit}");

            if (total >= _selectedPreset.StopAfterCaughtLimit)
            {
                LastStep = CatchSteps.Quitting;
            }
        }

        // Check if should stop with another bait/fish
        BaitConfig? CustomMoochCfg = GetPreset(LastCatch);
        if (CustomMoochCfg != null && CustomMoochCfg.StopAfterCaught)
        {
            int total = FishCounter.Add(CustomMoochCfg.BaitName);

            PluginLog.Debug($"{CustomMoochCfg.BaitName} caught. Total: {total} out of {CustomMoochCfg.StopAfterCaughtLimit}");

            if (total >= CustomMoochCfg.StopAfterCaughtLimit)
            {
                LastStep = CatchSteps.Quitting;
            }
        }
    }


    private void OnFrameworkUpdate(Framework _)
    {
        var state = Service.EventFramework.FishingState;

        if (!cfg.PluginEnabled || state == FishingState.None)
            return;

        if (state != FishingState.Quit && LastStep == CatchSteps.Quitting)
        {
            PlayerResources.CastActionDelayed(IDs.Actions.Quit);
            state = FishingState.Quit;
        }

        //CheckState();

        // FishBit in this case means that the fish was hooked, but it escaped. I might need to find a way to check if the fish was caught or not.
        if (LastStep != CatchSteps.Quitting && state == FishingState.PoleReady && (LastStep == CatchSteps.FishBit || LastStep == CatchSteps.FishCaught || LastStep == CatchSteps.TimeOut))
        {
            UseAutoCasts();
        }

        //CheckState();
        if (state == FishingState.Waiting2)
            CheckMaxTimeLimit();

        if (LastState == state)
            return;

        LastState = state;

        switch (state)
        {
            case FishingState.PullPoleIn: // If a hook is manually used before a bite, dont use auto cast
                if (LastStep == CatchSteps.BeganFishing || LastStep == CatchSteps.BeganFishing) LastStep = CatchSteps.None;
                break;
            case FishingState.Bite:
                if (LastStep != CatchSteps.FishBit) OnBite();
                break;
            case FishingState.Quit:
                OnFishingStop();
                break;
        }
    }
    private void OnFishingStop()
    {
        LastStep = CatchSteps.None;
        if (Timer.IsRunning)
        {
            Timer.Stop();
        }

        CurrentBait = "-";

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

        HookType? hook = null;

        if (PlayerResources.HasStatus(IDs.Status.IdenticalCast))
        {
            var IdenticalCast = GetPreset(LastCatch);
            if (IdenticalCast != null)
                hook = IdenticalCast.GetHookIgnoreEnable(bite);
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

    private static double LastTickMS = 200;
    private Stopwatch RecastTimer = new();
    private void UseAutoCasts()
    {
        if (!RecastTimer.IsRunning)
            RecastTimer.Start();

        if (RecastTimer.ElapsedMilliseconds > LastTickMS + 200)
        {
            LastTickMS = RecastTimer.ElapsedMilliseconds;

            AutoCast? cast = null;

            BaitConfig? CustomMoochCfg = GetPreset(LastCatch);

            if (CustomMoochCfg != null)
                cast = cfg.AutoCastsCfg.GetNextAutoCast(CustomMoochCfg);
            else
                cast = cfg.AutoCastsCfg.GetNextAutoCast(_selectedPreset);

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

        if(_selectedPreset.UseChumTimer && PlayerResources.HasStatus(IDs.Status.Chum))
        {
            minTime = Math.Truncate(_selectedPreset.MinChumTimeDelay * 100) / 100;
        }
        else 
            minTime = Math.Truncate(_selectedPreset.MinTimeDelay * 100) / 100;

        double timeElapsed = Math.Truncate((Timer.ElapsedMilliseconds / 1000.0) * 100) / 100;
        if (minTime > 0 && timeElapsed < minTime)
        {
            LastStep = CatchSteps.TimeOut;
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
            PluginLog.Debug($"Using Chum timer");
            maxTime = Math.Truncate(_selectedPreset.MaxChumTimeDelay * 100) / 100;
        }
        else
            maxTime = Math.Truncate(_selectedPreset.MaxTimeDelay * 100) / 100;
        double currentTime = Math.Truncate((Timer.ElapsedMilliseconds / 1000.0) * 100) / 100;

        if (maxTime > 0 && currentTime > maxTime && LastStep != CatchSteps.TimeOut)
        {
            PluginLog.Debug("Timeout. Hooking fish.");
            LastStep = CatchSteps.TimeOut;
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

    private static double debugValueLast = 3000;
    private Stopwatch Timerrr = new();
    private void CheckState()
    {
        if (!Timerrr.IsRunning)
            Timerrr.Start();

        if (Timerrr.ElapsedMilliseconds > debugValueLast + 500)
        {
            debugValueLast = Timerrr.ElapsedMilliseconds;
            PluginLog.Debug($"Fishing State: {Service.EventFramework.FishingState}, LastStep: {LastStep}");
        }
    }

    private static class FishCounter
    {
        static Dictionary<string, int> fishCount = new();

        public static int Add(string fishName)
        {
            if (!fishCount.ContainsKey(fishName))
                fishCount.Add(fishName, 0);
            fishCount[fishName]++;

            return GetCount(fishName);
        }

        public static int GetCount(string fishName)
        {
            if (!fishCount.ContainsKey(fishName))
                return 0;
            return fishCount[fishName];
        }

        public static void Reset()
        {
            fishCount = new();
        }
    }
}
