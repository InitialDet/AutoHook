using System;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using AutoHook;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using AutoHook.SeFunctions;
using AutoHook.Utils;
using Item = Lumina.Excel.GeneratedSheets.Item;

namespace GatherBuddy.Parser;

public partial class FishingParser : IDisposable
{
    private delegate bool UseActionDelegate(IntPtr manager, ActionType actionType, uint actionId, GameObjectID targetId, uint a4, uint a5,
        uint a6, IntPtr a7);

    public event Action? BeganFishing;
    public event Action? BeganMooching;
    public event Action<string, uint>? CaughtFish;

    private const XivChatType FishingMessage = (XivChatType)2243;

    private readonly Regexes _regexes = Regexes.FromLanguage(Service.Language);

    private readonly Hook<UpdateCatchDelegate>? _catchHook;

    public FishingParser()
    {
        _catchHook = new UpdateFishCatch(Service.SigScanner).CreateHook(OnCatchUpdate);
    }

    public void Enable()
    {
        _catchHook?.Enable();
        Service.Chat.ChatMessage += OnMessageDelegate;
    }

    public void Disable()
    {
        _catchHook?.Disable();
        Service.Chat.ChatMessage -= OnMessageDelegate;
    }

    public void Dispose()
    {
        Disable();
        _catchHook?.Dispose();
    }

    private void OnMessageDelegate(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        switch (type)
        {
            case FishingMessage:
                {
                    var text = message.TextValue;

                    var match = _regexes.Cast.Match(text);
                    if (match.Success)
                    {
                        BeganFishing?.Invoke();
                        return;
                    }

                    match = _regexes.Mooch.Match(text);
                    if (match.Success)
                    {
                        BeganMooching?.Invoke();
                        return;
                    }

                    break;
                }
        }
    }

    private void OnCatchUpdate(IntPtr module, uint fishId, bool large, ushort size, byte amount, byte level, byte unk7, byte unk8, byte unk9, byte unk10, byte unk11, byte unk12)
    {
        string fishName = MultiString.ParseSeStringLumina(Service.DataManager.GetExcelSheet<Item>()!.GetRow(fishId)?.Name);
        CaughtFish?.Invoke(fishName, fishId);
    }


}
