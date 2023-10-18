using System.Collections.Generic;
using AutoHook.Data;
using AutoHook.Resources.Localization;
using AutoHook.Utils;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoHook.Classes.AutoCasts;

public class AutoCordial : BaseActionCast
{
    private const uint CordialHiRecovery = 400;
    private const uint CordialRecovery = 300;
    private const uint CordialWateredRecovery = 150;

    public bool InvertCordialPriority;

    public AutoCordial() : base(UIStrings.Cordial, IDs.Item.Cordial, ActionType.Item)
    {
        GpThreshold = 1;
    }
    
    public override string GetName()
        => Name = UIStrings.Cordial;
    
    public override bool CastCondition()
    {
        var cordialList = new List<(uint, uint)>
        {
            (IDs.Item.HiCordial, CordialHiRecovery),
            (IDs.Item.Cordial, CordialRecovery),
            (IDs.Item.WateredCordial, CordialWateredRecovery)
        };
        

        if (InvertCordialPriority)
            cordialList.Reverse();
        
        foreach (var (id, recovery) in cordialList)
        {
            if (!PlayerResources.HaveCordialInInventory(id, out bool isHq))
                continue;
            
            var cordialRecovery = recovery;

            if (isHq)
                cordialRecovery += 50; // yep hardcoded (thumbsup emoji)
            
            Id = id;

            var notOvercaped = PlayerResources.GetCurrentGp() + cordialRecovery < PlayerResources.GetMaxGp();
            return notOvercaped;
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
        if (DrawUtil.Checkbox(UIStrings.AutoCastCordialPriority, ref InvertCordialPriority))
        {
            Service.Save();
        }
    };
}