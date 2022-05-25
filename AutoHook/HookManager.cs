using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AutoHook.Configurations;
using AutoHook.SeFunctions;
using AutoHook.Utils;
using Dalamud.Game;
using Dalamud.Logging;
using GatherBuddy.Parser;
using Action = Lumina.Excel.GeneratedSheets.Action;
using Item = Lumina.Excel.GeneratedSheets.Item;

namespace AutoHook.FishTimer;

public enum BiteType : byte
{
    Unknown = 0,
    Weak = 36,
    Strong = 37,
    Legendary = 38,
    None = 255,
}

[Flags]
internal enum CatchSteps
{
    None = 0x00,
    BeganFishing = 0x01,
    IdentifiedSpot = 0x02,
    FishBit = 0x04,
    FishCaught = 0x08,
    Mooch = 0x10,
    FishReeled = 0x20,
}

// all credits to Otter (goat discord) for his gatherbuddy plugin 
public class HookingManager : IDisposable
{
    const uint idNormalHook = 296;         //Action
    const uint idDoubleHook = 269;         //Action
    const uint idTripleHook = 27523;         //Action

    const uint idPrecision = 4179;   //Action
    const uint idPowerful = 4103;    //Action
    const uint idPatienceBuff = 850; //Status
    const uint idInefficientHook = 850; //Status764

    public string NormalHook { get; set; }
    public string DoubleHook { get; set; }
    public string TripleHook { get; set; }

    public string PrecisionHook { get; set; }
    public string PowerfulHook { get; set; }

    private bool timeOut = false;


    public HookSettings? CurrentSetting;
    private List<HookSettings> HookSettings;

    public readonly FishingParser Parser = new();
    internal CatchSteps Step = 0;
    internal FishingState LastState = FishingState.None;
    internal Stopwatch Timer = new();

    public static string? LastCatch = null;
    public static string? CurrentBait = null;

    public HookingManager()
    {
        NormalHook = MultiString.ParseSeStringLumina(Service.DataManager.GetExcelSheet<Action>()!.GetRow(idNormalHook)?.Name);
        DoubleHook = MultiString.ParseSeStringLumina(Service.DataManager.GetExcelSheet<Action>()!.GetRow(idDoubleHook)?.Name);
        TripleHook = MultiString.ParseSeStringLumina(Service.DataManager.GetExcelSheet<Action>()!.GetRow(idTripleHook)?.Name);

        PrecisionHook = MultiString.ParseSeStringLumina(Service.DataManager.GetExcelSheet<Action>()!.GetRow(idPrecision)?.Name);
        PowerfulHook = MultiString.ParseSeStringLumina(Service.DataManager.GetExcelSheet<Action>()!.GetRow(idPowerful)?.Name);

        HookSettings = Service.Configuration.CustomBaitMooch;
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
        try
        {
            var baitId = Service.CurrentBait.Current;
            string baitName = MultiString.ParseSeStringLumina(Service.DataManager.GetExcelSheet<Item>()!.GetRow(baitId)?.Name);
            return baitName;
        }
        catch (Exception e)
        {
            PluginLog.Error(e, "Failed to get current bait");
            return "-";
        }
    }

    private void Reset()
    {
        Timer.Reset();
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
        UpdateCurrentSetting();

        Reset();
        timeOut = false;
        Timer.Start();
        Step = CatchSteps.BeganFishing;
    }

    private void OnBeganMooch()
    {
        CurrentBait = new string(LastCatch);
        UpdateCurrentSetting();

        Reset();
        Timer.Start();
        timeOut = false;

        LastCatch = null;
        Step = CatchSteps.Mooch;
    }

    private void HookFish(BiteType bite)
    {
        try
        {
            if (CurrentSetting == null)
                return;

            if (GetHook(bite, out string hookName))
            {
                if (hookName == null || hookName.Trim().Length == 0)
                    return;

                PluginLog.Debug($"Hooking fish with {hookName}.");
                Service.CommandManager.Execute($"/ac \"{hookName}\"");
            }
            else
                PluginLog.Debug("No hook available.");
        }
        catch (Exception e)
        {
            PluginLog.Error(e.ToString());
        }
    }

    public bool GetHook(BiteType tug, out string hookName)
    {

        hookName = NormalHook;

        UpdateCurrentSetting();
        if (CurrentSetting == null) return false;

        double timeElapsed = Math.Truncate((Timer.ElapsedMilliseconds / 1000.0) * 100) / 100;
        double minTime = Math.Truncate(CurrentSetting.MinTimeDelay * 100) / 100;

        if (minTime > 0 && timeElapsed < minTime)
        {
            PluginLog.Debug($"Not enough time to hook. {timeElapsed} < {minTime}");
            return false;
        }

        PluginLog.Debug($"Current GP: {GetCurrentGP()}");

        bool hasPatience = GetPatienceBuff();

        // Cheking if we should use double/triple hook
        if ((CurrentSetting.UseDoubleHook || CurrentSetting.UseTripleHook))
        {
            // Do nothing if patience is up and the user choose not to use hook it
            if (hasPatience && !CurrentSetting.UseTripleDoubleHookPacience)
                PluginLog.Debug($"Patience buff detected. Not using double/triple hook");
            else
            {
                if (CurrentSetting.UseDoubleHook && GetCurrentGP() > 400)
                {
                    hookName = DoubleHook;
                    return true;
                }
                else if (CurrentSetting.UseTripleHook && GetCurrentGP() > 700)
                {
                    hookName = TripleHook;
                    return true;
                }
            }
        }

        bool hookWeak = CurrentSetting.HookWeak;
        bool hookStrong = CurrentSetting.HookStrong;
        bool hookLendary = CurrentSetting.HookLendary;

        switch (tug)
        {
            case BiteType.Weak:
                if (hasPatience) hookName = PrecisionHook;
                return CurrentSetting.HookWeak;
            case BiteType.Strong:
                if (hasPatience) hookName = PowerfulHook;
                return CurrentSetting.HookStrong;
            case BiteType.Legendary:
                if (hasPatience) hookName = PowerfulHook;
                return CurrentSetting.HookLendary;
            default:
                return true;
        }
    }

    private bool GetPatienceBuff()
    {
        if (Service.ClientState.LocalPlayer?.StatusList == null)
            return false;

        foreach (var buff in Service.ClientState.LocalPlayer.StatusList)
        {
            if (buff.StatusId == idPatienceBuff)
                return true;
        }

        return false;
    }

    private uint GetCurrentGP()
    {
        if (Service.ClientState.LocalPlayer?.CurrentGp == null)
            return 0;

        return Service.ClientState.LocalPlayer.CurrentGp;
    }



    // The current config is updates two times: When we began fishing (to get the config based on the mooch/bait) and when we hooked the fish (in case the user updated their configs).
    private void UpdateCurrentSetting()
    {
        CurrentSetting = HookSettings.FirstOrDefault(mooch => mooch.BaitName.Equals(CurrentBait));

        if (CurrentSetting == null)
        {
            HookSettings defaultSettings;

            if (Step.HasFlag(CatchSteps.Mooch))
                defaultSettings = Service.Configuration.DefaultMoochSettings;
            else
                defaultSettings = Service.Configuration.DefaultCastSettings;

            if (defaultSettings.Enabled)
                CurrentSetting = defaultSettings;
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
        try
        {
            Timer.Stop();

            HookFish(Service.TugType?.Bite ?? BiteType.Unknown);

            Step |= CatchSteps.FishBit;
        }
        catch (Exception e)
        {
            PluginLog.Error(e.ToString());
        }
    }

    private void OnFishingStop()
    {
        if (Timer.IsRunning)
        {
            Timer.Stop();
            return;
        }

        if (!Step.HasFlag(CatchSteps.BeganFishing))
            return;

        Step = CatchSteps.None;
    }

    private void OnCatch(string fishName, uint fishId)
    {
        LastCatch = fishName;
        CurrentBait = GetCurrentBait();
    }

    private void OnFrameworkUpdate(Framework _)
    {
        if (!Service.Configuration.AutoHookEnabled)
        {
            return;
        }

        var state = Service.EventFramework.FishingState;

        if (CurrentSetting == null)
            return;

        // im not smart enough to make it more effiecient
        if (!timeOut && state == FishingState.Waiting2)
        {

            double maxTime = Math.Truncate(CurrentSetting.MaxTimeDelay * 100) / 100;
            double time = Math.Truncate((Timer.ElapsedMilliseconds / 1000.0) * 100) / 100;

            if (maxTime > 0 && time > maxTime)
            {
                timeOut = true;
                PluginLog.Debug("Time out. Hooking fish.");
                OnBite();
            }
        }

        if (LastState == state)
            return;

        LastState = state;

        switch (state)
        {
            case FishingState.Bite:
                if (!timeOut) OnBite();
                break;
            case FishingState.Reeling:
                Step |= CatchSteps.FishReeled;
                break;
            case FishingState.PoleReady:
            case FishingState.Quit:
                OnFishingStop();
                break;
        }
    }
}
