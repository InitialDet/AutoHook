using System;
using AutoHook.Data;
using AutoHook.Resources.Localization;
using AutoHook.Utils;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoHook.Classes.AutoCasts;

public class AutoThaliaksFavor : BaseActionCast
{
    public int ThaliaksFavorStacks = 3;
    public int ThaliaksFavorRecover = 150;

    public AutoThaliaksFavor() : base(UIStrings.Thaliaks_Favor, IDs.Actions.ThaliaksFavor, ActionType.Action)
    {
        HelpText = UIStrings.TabAutoCasts_DrawThaliaksFavor_HelpText;
    }
    
    public override string GetName()
        => Name = UIStrings.Thaliaks_Favor;

    public override bool CastCondition()
    {
        bool hasStacks = PlayerResources.HasAnglersArtStacks(ThaliaksFavorStacks);

        bool notOvercaped = (PlayerResources.GetCurrentGp() + ThaliaksFavorRecover) < PlayerResources.GetMaxGp();

        return hasStacks && notOvercaped; // dont use if its going to overcap gp
    }
    
    protected override DrawOptionsDelegate DrawOptions => () =>
    {
        var stack = ThaliaksFavorStacks;
        if (DrawUtil.EditNumberField(UIStrings.TabAutoCasts_DrawExtraOptionsThaliaksFavor_, ref stack))
        {
            // value has to be between 3 and 10
            ThaliaksFavorStacks = Math.Max(3, Math.Min(stack, 10));

            Service.Save();
        }
    };
}