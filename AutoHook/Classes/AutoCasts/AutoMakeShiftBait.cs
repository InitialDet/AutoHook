using System;
using AutoHook.Data;
using AutoHook.Resources.Localization;
using AutoHook.Utils;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoHook.Classes.AutoCasts;

public class AutoMakeShiftBait : BaseActionCast
{
    public int MakeshiftBaitStacks = 5;
    private bool _onlyUseWithIntuition;

    public AutoMakeShiftBait() : base(UIStrings.MakeShift_Bait, IDs.Actions.MakeshiftBait, ActionType.Action)
    {
        HelpText = UIStrings.TabAutoCasts_DrawMakeShiftBait_HelpText;
    }

    public override string GetName()
        => Name = UIStrings.MakeShift_Bait;

    public override bool CastCondition()
    {
        if (!Enabled)
            return false;

        if (PlayerResources.HasStatus(IDs.Status.MakeshiftBait))
            return false;

        if (PlayerResources.HasStatus(IDs.Status.PrizeCatch))
            return false;

        if (PlayerResources.HasStatus(IDs.Status.AnglersFortune))
            return false;

        if (!PlayerResources.HasStatus(IDs.Status.FishersIntuition) && _onlyUseWithIntuition)
            return false;


        bool available = PlayerResources.ActionTypeAvailable(IDs.Actions.MakeshiftBait);
        bool hasStacks = PlayerResources.HasAnglersArtStacks(MakeshiftBaitStacks);

        return hasStacks && available;
    }

    protected override DrawOptionsDelegate DrawOptions => () =>
    {
        var stack = MakeshiftBaitStacks;
        if (DrawUtil.EditNumberField(UIStrings.TabAutoCasts_When_Stack_Equals, ref stack))
        {
            // value has to be between 5 and 10
            MakeshiftBaitStacks = Math.Max(5, Math.Min(stack, 10));
            Service.Save();
        }

        if (DrawUtil.Checkbox(UIStrings.OnlyUseWhenFisherSIntutionIsActive, ref _onlyUseWithIntuition))
        {
            Service.Save();
        }
    };
}