using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AutoHook.Configurations;
using AutoHook.Utils;
using Dalamud.Game;
using Dalamud.Logging;
using GatherBuddy.Parser;
using Item = Lumina.Excel.GeneratedSheets.Item;
using FFXIVClientStructs.FFXIV.Client.Game;
using AutoHook.Enums;
using AutoHook.Data;

namespace AutoHook.FishTimer;

// all credits to Otter (goat discord) for his gatherbuddy plugin 
public class HookingManager : IDisposable
{
    public HookConfig? CurrentSetting;
    private List<HookConfig> HookSettings = Service.Configuration.CustomBait;

    public readonly FishingParser Parser = new();
    internal CatchSteps Step = 0;
    internal FishingState LastState = FishingState.None;
    internal Stopwatch Timer = new();

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
        Step = CatchSteps.BeganFishing;
        UpdateCurrentSetting();
    }

    private void OnBeganMooch()
    {
        CurrentBait = new string(LastCatch);
        Timer.Reset();
        Timer.Start();
        LastCatch = null;
        Step = CatchSteps.BeganMooching;
        UpdateCurrentSetting();
    }

    // The current config is updates two times: When we began fishing (to get the config based on the mooch/bait) and when we hooked the fish (in case the user updated their configs).
    private void UpdateCurrentSetting()
    {
        CurrentSetting = HookSettings.FirstOrDefault(mooch => mooch.BaitName.Equals(CurrentBait));

        if (CurrentSetting == null)
        {
            HookConfig defaultConfig;

            if (Step == CatchSteps.BeganMooching)
                defaultConfig = Service.Configuration.DefaultMoochConfig;
            else
                defaultConfig = Service.Configuration.DefaultCastConfig;

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
        Step = CatchSteps.FishBit;
        Timer.Stop();

        HookFish(Service.TugType?.Bite ?? BiteType.Unknown);
    }

    private unsafe void HookFish(BiteType bite)
    {
        UpdateCurrentSetting();
        if (CurrentSetting == null)
            return;

        // Check if the minimum time has passed
        if (!CheckValidMinTime(Math.Truncate(CurrentSetting.MinTimeDelay * 100) / 100))
        {
            Step = CatchSteps.TimeOut;
            return;
        }

        HookType hook = CurrentSetting.GetHook(bite);

        if (hook == HookType.None)
        {
            Step = CatchSteps.TimeOut;
            return;
        }
        
        CastAction((uint)hook);
    }

    private bool CheckValidMinTime(double minTime)
    {
        double timeElapsed = Math.Truncate((Timer.ElapsedMilliseconds / 1000.0) * 100) / 100;
        if (minTime > 0 && timeElapsed < minTime)
        {
            PluginLog.Debug($"Not enough time to hook. {timeElapsed} < {minTime}");
            return false;
        }

        return true;
    }

    private void OnFishingStop()
    {
        if (Timer.IsRunning)
        {
            Timer.Stop();
            return;
        }

        //Step = CatchSteps.None;
    }

    private void OnCatch(string fishName, uint fishId)
    {
        LastCatch = fishName;
        CurrentBait = GetCurrentBait();

        Step = CatchSteps.FishCaught;
    }

    // jesus christ if someone can figure out a better way to do this, please elp me i'm tired of this i hate programming im goin to morb
    private unsafe void AutoCastMooch()
    {
        if (ActionAvailable(IDs.idCast))
        {
            // First, check if theres a specific config for the fish that was just hooked
            var HasMoochConfig = HookSettings.FirstOrDefault(mooch => mooch.BaitName.Equals(LastCatch));
            if (HasMoochConfig != null)
            { 
                if (ActionAvailable(IDs.idMooch) && HasMoochConfig.GetUseAutoMooch())
                    CastAction(IDs.idMooch);
                else if (ActionAvailable(IDs.idMooch2) && HasMoochConfig.GetUseAutoMooch2())
                    CastAction(IDs.idMooch2);
                else if (Service.Configuration.UseAutoCast)
                    CastAction(IDs.idCast);
                return; // if we have a config for the fish, we dont need to check the rest of the configs
            }

            // This is the behavior for when the config is default or a bait (not a fish)
            if (ActionAvailable(IDs.idMooch) && CurrentSetting != null && CurrentSetting.GetUseAutoMooch())
                CastAction(IDs.idMooch);
            else if (ActionAvailable(IDs.idMooch2) && CurrentSetting != null && CurrentSetting.GetUseAutoMooch2())
                CastAction(IDs.idMooch2);
            else if (ActionAvailable(IDs.idMooch) && Service.Configuration.UseAutoMooch)
                CastAction(IDs.idMooch);
            else if (ActionAvailable(IDs.idMooch2) && Service.Configuration.UseAutoMooch2)
                CastAction(IDs.idMooch2);
            else if (Service.Configuration.UseAutoCast)
                CastAction(IDs.idCast);

        }
    }

    private unsafe void CastAction(uint id)
    {
        ActionManager.Instance()->UseAction(ActionType.Spell, id);
    }

    private unsafe bool ActionAvailable(uint id)
    {
        // status 0 == available to cast? not sure but it seems to be
        // Also make sure its the skill is not on cooldown (maily for mooch2)
        return ActionManager.Instance()->GetActionStatus(ActionType.Spell, id) == 0 && !ActionManager.Instance()->IsRecastTimerActive(ActionType.Spell, id);
    }

    private void OnFrameworkUpdate(Framework _)
    {
        var state = Service.EventFramework.FishingState;

        if (!Service.Configuration.AutoHookEnabled || state == FishingState.None)
        {
            return;
        }

        if (state == FishingState.PoleReady && (Step == CatchSteps.FishCaught || Step == CatchSteps.FishReeled || Step == CatchSteps.TimeOut))
        {
            AutoCastMooch();
        }

        //CheckState();

        if (state == FishingState.Waiting2)
        {
            CheckTimeout();
        }

        if (LastState == state)
            return;

        LastState = state;

        switch (state)
        {
            case FishingState.Reeling:
                Step = CatchSteps.FishReeled;
                break;
            case FishingState.PullPoleIn:
                //Step = CatchSteps.FishReeled;
                break;
            case FishingState.Bite:
                if (Step != CatchSteps.FishBit) OnBite();
                break;

            case FishingState.Quit:
                OnFishingStop();
                break;
        }
    }

    private static double debugValueLast = 1000;
    internal Stopwatch Timerrr = new();
    private void CheckState()
    {
        if (!Timerrr.IsRunning)
            Timerrr.Start();

        if (Timerrr.ElapsedMilliseconds > debugValueLast + 500)
        {
            debugValueLast = Timerrr.ElapsedMilliseconds;
            PluginLog.Debug($"Fishing State: {Service.EventFramework.FishingState}, Step: {Step}");
        }
    }

    private void CheckTimeout()
    {
        if (CurrentSetting == null)
            return;

        double maxTime = Math.Truncate(CurrentSetting.MaxTimeDelay * 100) / 100;
        double currentTime = Math.Truncate((Timer.ElapsedMilliseconds / 1000.0) * 100) / 100;

        if (maxTime > 0 && currentTime > maxTime && Step != CatchSteps.TimeOut)
        {
            PluginLog.Debug("Time out. Hooking fish.");
            CastAction(IDs.idNormalHook);
            Step = CatchSteps.TimeOut;
        }
    }
}
