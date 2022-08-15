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
    private HookConfig? CurrentSetting;
    private List<HookConfig> HookSettings = cfg.CustomBait;

    private static Configuration cfg = Service.Configuration;

    private readonly FishingParser Parser = new();
    private CatchSteps LastStep = 0;
    private FishingState LastState = FishingState.None;
    private Stopwatch Timer = new();

    public static string? LastCatch = null;
    public static string? CurrentBait = null;

    public HookingManager()
    {
        CurrentBait = GetCurrentBait();
    }

    public void Enable()
    {
        Parser.Enable();
        SubscribeToParser();
        Service.Framework.Update += OnFrameworkUpdate;
    }

    public void Disable()
    {
        UnSubscribeToParser();
        Parser.Disable();
        Service.Framework.Update -= OnFrameworkUpdate;
    }

    public void Dispose()
    {
        Disable();
        Parser.Dispose();
    }

    public static string GetCurrentBait()
    {
        var baitId = Service.CurrentBait.Current;
        string baitName = MultiString.ParseSeStringLumina(Service.DataManager.GetExcelSheet<Item>()!.GetRow(baitId)?.Name);
        return baitName;
    }

    private void SubscribeToParser()
    {
        Parser.CaughtFish += OnCatch;
        Parser.BeganFishing += OnBeganFishing;
        Parser.BeganMooching += OnBeganMooch;
    }

    private void UnSubscribeToParser()
    {
        Parser.CaughtFish -= OnCatch;
        Parser.BeganFishing -= OnBeganFishing;
        Parser.BeganMooching -= OnBeganMooch;
    }

    // The current config is updates two times: When we began fishing (to get the config based on the mooch/bait) and when we hooked the fish (in case the user updated their configs).
    private void UpdateCurrentSetting()
    {
        ResetAFKTimer();

        if (CurrentSetting == null)
            CurrentSetting = HookSettings.FirstOrDefault(mooch => mooch.BaitName.Equals(CurrentBait));

        if (CurrentSetting == null)
        {
            HookConfig defaultConfig;

            if (LastStep == CatchSteps.BeganMooching)
                defaultConfig = cfg.DefaultMoochConfig;
            else
                defaultConfig = cfg.DefaultCastConfig;

            if (defaultConfig.Enabled)
                CurrentSetting = defaultConfig;
        }

        else if (!CurrentSetting.Enabled)
            CurrentSetting = null;

        if (CurrentSetting == null)
            PluginLog.Debug("No config found. Not hooking");
        else
            PluginLog.Debug($"Config found. Hooking with {CurrentSetting.BaitName} Config");
    }

    private void OnBeganFishing()
    {
        CurrentBait = GetCurrentBait();
        Timer.Reset();
        Timer.Start();
        LastStep = CatchSteps.BeganFishing;
        UpdateCurrentSetting();
    }

    private void OnBeganMooch()
    {
        CurrentBait = new string(LastCatch);
        Timer.Reset();
        Timer.Start();
        LastCatch = null;
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
        
        // Check if should stop with the current bait/mooch
        if (CurrentSetting != null && CurrentSetting.StopAfterCaught) {
            int total = FishCounter.Add(CurrentSetting.BaitName);

            PluginLog.Debug($"{CurrentSetting.BaitName} caught. Total: {total} out of {CurrentSetting.StopAfterCaughtLimit}");

            if (total >= CurrentSetting.StopAfterCaughtLimit) {
                LastStep = CatchSteps.Quitting;
            }
        }

        // Check if should stop with another bait/mooch
        HookConfig? CustomMoochCfg = HookSettings.FirstOrDefault(mooch => mooch.BaitName.Equals(LastCatch));
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

    private void OnFishingStop()
    {
        LastStep = CatchSteps.None;
        if (Timer.IsRunning)
        {
            Timer.Stop();
    
        }

        CurrentBait = "-";
        FishCounter.Reset();
    }

    private void OnFrameworkUpdate(Framework _)
    {
        var state = Service.EventFramework.FishingState;

        if (!cfg.PluginEnabled || state == FishingState.None)
            return;

        if (state != FishingState.Quit && LastStep == CatchSteps.Quitting) {
            PlayerResources.CastActionDelayed(IDs.Actions.Quit);
            state = FishingState.Quit;
        }

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

    private unsafe void HookFish(BiteType bite)
    {
        if (CurrentSetting == null)
            return;

        // Check if the minimum time has passed
        if (!CheckMinTimeLimit())
            return;

        HookType? hook = null;

        if (PlayerResources.HasStatus(IDs.Status.IdenticalCast))
        {
            var IdenticalCast = HookSettings.FirstOrDefault(mooch => mooch.BaitName.Equals(LastCatch));
            if (IdenticalCast != null)
                hook = IdenticalCast.GetHookIgnoreEnable(bite);
            else
                hook = CurrentSetting.GetHook(bite);
        }
        else
            hook = CurrentSetting.GetHook(bite);

        if (hook == null || hook == HookType.None)
            return;

        if (PlayerResources.ActionAvailable((uint)hook)) // Check if Powerful/Precision is available
            PlayerResources.CastAction((uint)hook);
        else // If not, use normal hook
            PlayerResources.CastAction((uint)HookType.Normal);
    }

    private void UseAutoCasts()
    {
        AutoCast? cast = null;

        HookConfig? CustomMoochCfg = HookSettings.FirstOrDefault(mooch => mooch.BaitName.Equals(LastCatch));

        if (CustomMoochCfg != null)
            cast = cfg.AutoCastsCfg.GetNextAutoCast(CustomMoochCfg);
        else
            cast = cfg.AutoCastsCfg.GetNextAutoCast(CurrentSetting);

        if (cast != null)
        {
            PlayerResources.CastActionDelayed(cast.Id, cast.ActionType);
        }
    }

    private void QuitFishing() {
        PlayerResources.CastActionDelayed(IDs.Actions.Quit);
    }

    private bool CheckMinTimeLimit()
    {
        if (CurrentSetting == null)
            return true;

        double minTime = Math.Truncate(CurrentSetting.MinTimeDelay * 100) / 100;
        double timeElapsed = Math.Truncate((Timer.ElapsedMilliseconds / 1000.0) * 100) / 100;
        if (minTime > 0 && timeElapsed < minTime)
        {
            PluginLog.Debug($"Not enough time to hook. {timeElapsed} < {minTime}");
            LastStep = CatchSteps.TimeOut;
            return false;
        }

        return true;
    }

    private void CheckMaxTimeLimit()
    {
        if (CurrentSetting == null)
            return;

        double maxTime = Math.Truncate(CurrentSetting.MaxTimeDelay * 100) / 100;
        double currentTime = Math.Truncate((Timer.ElapsedMilliseconds / 1000.0) * 100) / 100;

        if (maxTime > 0 && currentTime > maxTime && LastStep != CatchSteps.TimeOut)
        {
            PluginLog.Verbose("Timeout. Hooking fish.");
            LastStep = CatchSteps.TimeOut;
            PlayerResources.CastAction(IDs.Actions.Hook);
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

    private static class FishCounter {
        static Dictionary<string, int> fishCount = new();

        public static int Add(string fishName) {
            if (!fishCount.ContainsKey(fishName))
                fishCount.Add(fishName, 0);
            fishCount[fishName]++;

            return GetCount(fishName);
        }

        public static int GetCount(string fishName) {
            if (!fishCount.ContainsKey(fishName))
                return 0;
            return fishCount[fishName];
        }

        public static void Reset() {

            fishCount = new();
        }
    }
}
