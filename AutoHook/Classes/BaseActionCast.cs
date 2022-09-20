using AutoHook.Configurations;
using AutoHook.Data;
using AutoHook.Utils;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoHook.Classes;
public abstract class BaseActionCast
{
    protected HookConfig? HookConfig = null;

    protected BaseActionCast(string name, uint id, ActionType actionType = ActionType.Spell)
    {
        Name = name;
        ID = id;
        Enabled = false;

        ActionType = actionType;

        if (actionType == ActionType.Spell)
            GPThreshold = PlayerResources.CastActionCost(ID, ActionType);
    }

    public string Name { get; protected init; }

    public bool Enabled { get; set; }

    public uint ID { get; protected init; }

    public uint GPThreshold { get; set; }

    public bool GPThresholdAbove { get; set; } = true;

    public bool DoesCancelMooch { get; set; } = false;

    public ActionType ActionType { get; protected init; }

    public virtual void SetThreshold(uint newcost)
    {
        var actionCost = PlayerResources.CastActionCost(ID, ActionType);
        if (newcost < actionCost)
            GPThreshold = actionCost;
        else
            GPThreshold = newcost;
    }

    public bool IsAvailableToCast(HookConfig? hookConfig)
    {
        this.HookConfig = hookConfig;

        if (!Enabled)
            return false;

        if (DoesCancelMooch && AutoCastsConfig.IsMoochAvailable && AutoCastsConfig.DontCancelMooch)
            return false;

        uint currentGp = PlayerResources.GetCurrentGP();

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
