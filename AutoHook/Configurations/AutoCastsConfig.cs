using AutoHook.Data;
using AutoHook.Utils;
using Dalamud.Logging;

namespace AutoHook.Configurations;

public class AutoCastsConfig
{
    public bool EnableAll = false;
    public bool EnableAutoCast = false;
    public bool EnableMooch = false;
    public bool EnableMooch2 = false;

    public bool EnablePatience = false;
    public bool EnableMakeshiftPatience = false;
    public uint SelectedPatienceID = IDs.Actions.Patience2; // Default to Patience2

    public bool EnableThaliaksFavor = false;
    public int ThaliaksFavorStacks = 3;

    public bool EnableMakeshiftBait = false;
    public int MakeshiftBaitStacks = 5;

    public bool EnablePrizeCatch = false;

    public bool EnableChum = false;
    public bool EnableFishEyes = false;

    public uint GetNextAutoCast(HookConfig? hookConfig)
    {
        if (!EnableAll)
            return 0;

        if (!PlayerResources.ActionAvailable(IDs.Actions.Cast))
            return 0;

        if (UseThaliaksFavor())
            return IDs.Actions.ThaliaksFavor;

        if (UseMakeshiftBait())
            return IDs.Actions.MakeshiftBait;

        if (UsesChum())
            return IDs.Actions.Chum;

        if (UsesFishEyes())
            return IDs.Actions.FishEyes;

        bool useAutoMooch = false;
        bool useAutoMooch2 = false;

        if (hookConfig == null || hookConfig?.BaitName == "DefaultCast" || hookConfig?.BaitName == "DefaultMooch")
        {
            useAutoMooch = EnableMooch;
            useAutoMooch2 = EnableMooch2;
        }
        else
        {
            useAutoMooch = hookConfig?.UseAutoMooch ?? false;
            useAutoMooch2 = hookConfig?.UseAutoMooch2 ?? false;
        }

        if (useAutoMooch)
        {
            if (PlayerResources.ActionAvailable(IDs.Actions.Mooch))
                return IDs.Actions.Mooch;
            else if (useAutoMooch2 && PlayerResources.ActionAvailable(IDs.Actions.Mooch2))
                return IDs.Actions.Mooch2;
        }

        if (UsePrizeCatch())
            return IDs.Actions.PrizeCatch;

        if (UsePatience()) // This cant be used if a mooch is available or it'll cancel it
            return SelectedPatienceID;

        if (EnableAutoCast)
            return IDs.Actions.Cast;

        return 0;
    }

    private bool UsePatience()
    {
        if (EnablePatience)
        {
            if (!PlayerResources.HasStatus(IDs.Status.AnglersFortune))
            {

                if (PlayerResources.HasStatus(IDs.Status.PrizeCatch))
                    return false;

                if (PlayerResources.HasStatus(IDs.Status.MakeshiftBait) && !EnableMakeshiftPatience)
                    return false;

                if (PlayerResources.ActionAvailable(SelectedPatienceID))
                {
                    if (SelectedPatienceID == IDs.Actions.Patience)
                        return PlayerResources.GetCurrentGP() >= (200 + 20);
                    if (SelectedPatienceID == IDs.Actions.Patience2)
                        return PlayerResources.GetCurrentGP() >= (560 + 20);
                }
            }
        }

        return false;
    }

    private uint ThaliaksFavorRecover = 150; // This might change in the future.

    private bool UseThaliaksFavor()
    {
        if (!EnableThaliaksFavor)
            return false;
        bool available = PlayerResources.ActionAvailable(IDs.Actions.ThaliaksFavor);
        bool hasStacks = PlayerResources.HasAnglersArtStacks(ThaliaksFavorStacks);
        bool notOvercaped = (PlayerResources.GetCurrentGP() + ThaliaksFavorRecover) < PlayerResources.GetMaxGP();

        return available && hasStacks && notOvercaped; // dont use if its going to overcap gp
    }

    private bool UseMakeshiftBait()
    {
        if (!EnableMakeshiftBait)
            return false;

        if (PlayerResources.HasStatus(IDs.Status.MakeshiftBait))
            return false;

        if (PlayerResources.HasStatus(IDs.Status.PrizeCatch))
            return false;

        if (PlayerResources.HasStatus(IDs.Status.AnglersFortune))
            return false;

        
        bool available = PlayerResources.ActionAvailable(IDs.Actions.MakeshiftBait);
        bool hasStacks = PlayerResources.HasAnglersArtStacks(MakeshiftBaitStacks);

        return hasStacks && available;
    }

    private bool IsMoochAvailable()
    {
        return PlayerResources.ActionAvailable(IDs.Actions.Mooch) || PlayerResources.ActionAvailable(IDs.Actions.Mooch2);
    }

    private bool UsePrizeCatch()
    {
        return EnablePrizeCatch && PlayerResources.ActionAvailable(IDs.Actions.PrizeCatch);
    }

    private bool UsesFishEyes()
    {
        return EnableFishEyes && PlayerResources.ActionAvailable(IDs.Actions.FishEyes);
    }

    private bool UsesChum()
    {
        return EnableChum && PlayerResources.ActionAvailable(IDs.Actions.Chum);
    }
}