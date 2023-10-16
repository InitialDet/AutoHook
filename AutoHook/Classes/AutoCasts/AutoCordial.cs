using System.Collections.Generic;
using AutoHook.Data;
using AutoHook.Resources.Localization;
using AutoHook.Utils;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoHook.Classes.AutoCasts;

public class AutoCordial : BaseActionCast
{
    private const uint CordialHiRecovery = 400;
    private const uint CordialHqRecovery = 350;
    private const uint CordialRecovery = 300;
    private const uint CordialHqWateredRecovery = 200;
    private const uint CordialWateredRecovery = 150;

    private bool _invertCordialPriority;
    
    public AutoCordial() : base(UIStrings.Cordial, IDs.Item.Cordial, ActionType.Item)
    {
        GpThreshold = 1;
    }

    public override bool CastCondition()
    {
        var cordialList = new List<(uint, bool, uint)>
        {
            (IDs.Item.HiCordial, false, CordialHiRecovery),
            (IDs.Item.Cordial, true, CordialHqRecovery), //Hq
            (IDs.Item.Cordial, false, CordialRecovery),
            (IDs.Item.WateredCordial, true, CordialHqWateredRecovery), // Hq
            (IDs.Item.WateredCordial, false, CordialWateredRecovery)
        };
        
        if (_invertCordialPriority)
            cordialList.Reverse();
        
        foreach (var (id, hq, recovery) in cordialList)
        {
            if (!PlayerResources.HaveItemInInventory(id, hq))
                continue;
            
            Id = id;
            
            var notOvercaped = PlayerResources.GetCurrentGp() + recovery < PlayerResources.GetMaxGp();
            return notOvercaped && PlayerResources.IsPotOffCooldown();
        }
        return false;
    }

    public override void SetThreshold(int newCost)
    {
        if (newCost <= 1)
            GpThreshold = 1;
        else
            GpThreshold = newCost;
    }
    
    protected override DrawOptionsDelegate DrawOptions => () =>
    {
        if (DrawUtil.Checkbox(UIStrings.AutoCastCordialPriority, ref _invertCordialPriority))
        {
            Service.Save();
        }
    };
}