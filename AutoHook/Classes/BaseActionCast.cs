using AutoHook.Configurations;
using AutoHook.Utils;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoHook.Classes;
public abstract class BaseActionCast
{
    protected BaitConfig? _baitConfig = null;
    protected static readonly AutoCastsConfig AcConfig = Service.Configuration.AutoCastsCfg;

    protected BaseActionCast(string name, uint id, ActionType actionType = ActionType.Action)
    {
        Name = name;
        ID = id;
        Enabled = false;

        ActionType = actionType;

        if (actionType == ActionType.Action)
            GPThreshold = PlayerResources.CastActionCost(ID, ActionType);
    }

    public string Name { get; protected init; }

    public bool Enabled { get; set; }

    public uint ID { get; protected init; }

    public uint GPThreshold { get; set; }

    public bool GPThresholdAbove { get; set; } = true;

    public bool DoesCancelMooch { get; set; } = false;

    public ActionType ActionType { get; protected init; }

    public virtual void SetThreshold(uint newCost)
    {
        var actionCost = PlayerResources.CastActionCost(ID, ActionType);
        if (newCost < actionCost)
            GPThreshold = actionCost;
        else
            GPThreshold = newCost;
    }

    public bool IsAvailableToCast(BaitConfig? baitConfig)
    {
        this._baitConfig = baitConfig;

        if (!Enabled)
            return false;

        if (DoesCancelMooch && PlayerResources.IsMoochAvailable() && AcConfig.DontCancelMooch)
            return false;

        uint currentGp = PlayerResources.GetCurrentGp();

        bool hasGP;

        if (GPThresholdAbove)
            hasGP = currentGp >= GPThreshold;
        else
            hasGP = currentGp <= GPThreshold;

        bool isActive = PlayerResources.ActionAvailable(ID, ActionType);

        return hasGP && isActive && CastCondition();
    }

    public abstract bool CastCondition();

}
