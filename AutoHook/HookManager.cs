using System;
using System.Diagnostics;
using System.Linq;
using AutoHook.Configurations;
using AutoHook.Utils;
using Dalamud.Game;
using Dalamud.Logging;
using GatherBuddy.Parser;
using Item = Lumina.Excel.GeneratedSheets.Item;
using AutoHook.Enums;
using AutoHook.Data;
using System.Collections.Generic;

namespace AutoHook.FishTimer;

// all credits to Otter (goat discord) for his gatherbuddy plugin 
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

    // The current config is updates two times: When we began fishing (to get the config based on the mooch/bait) and when we hooked the fish (in case the user updated their configs).
    private void UpdateCurrentSetting()
    {
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

    private void OnBite()
    {
        UpdateCurrentSetting();
        LastStep = CatchSteps.FishBit;
        Timer.Stop();

        HookFish(Service.TugType?.Bite ?? BiteType.Unknown);
    }

    private unsafe void HookFish(BiteType bite)
    { 
        if (CurrentSetting == null)
            return;

        // Check if the minimum time has passed
        if (!CheckMinTimeLimit())
            return;

        HookType hook = CurrentSetting.GetHook(bite);

        if (hook == HookType.None)
            return;

        if (PlayerResources.ActionAvailable((uint)hook)) // Check if Powerful/Precision is available
            PlayerResources.CastAction((uint)hook);
        else // If not, use normal hook
            PlayerResources.CastAction((uint)HookType.Normal);
    }

    private void OnCatch(string fishName, uint fishId)
    {
        LastCatch = fishName;
        CurrentBait = GetCurrentBait();

        LastStep = CatchSteps.FishCaught;
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

    private void OnFishingStop()
    {
        if (Timer.IsRunning)
        {
            Timer.Stop();
            return;
        }

        CurrentBait = "-";
    }

    private void UseAutoCasts()
    {
        AutoCast? cast = null;

        HookConfig? CustomMoochCfg = HookSettings.FirstOrDefault(mooch => mooch.BaitName.Equals(LastCatch));
        
        if (CustomMoochCfg != null)
            cast = cfg.AutoCastsCfg.GetNextAutoCast(CustomMoochCfg);
        else
            cast = cfg.AutoCastsCfg.GetNextAutoCast(CurrentSetting);

        if (cast != null) {
            PlayerResources.CastActionDelayed(cast.Id, cast.ActionType);
        }     
    }

    private void OnFrameworkUpdate(Framework _)
    {
        var state = Service.EventFramework.FishingState;

        if (!cfg.PluginEnabled || state == FishingState.None)
            return;

        // FishBit in this case means that the fish was hooked, but it escaped. I might need to find a way to check if the fish was caught or not.
        if (state == FishingState.PoleReady && (LastStep == CatchSteps.FishBit || LastStep == CatchSteps.FishCaught || LastStep == CatchSteps.TimeOut))
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

    private static double debugValueLast = 1000;
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
}
