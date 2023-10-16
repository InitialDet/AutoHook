using System.Linq;
using AutoHook.Utils;
using Dalamud.Game;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoHook.SeFunctions;

public sealed class CurrentBait : SeAddressBase
{
    public CurrentBait(ISigScanner sigScanner)
        : base(sigScanner, "3B 05 ?? ?? ?? ?? 75 ?? C6 43")
    {
        Service.GameInteropProvider.InitializeFromAttributes(this);
    }

    public unsafe uint Current
        => *(uint*)Address;
    
    private delegate byte ExecuteCommandDelegate(int id, int unk1, uint baitId, int unk2, int unk3);
    
    [Signature("E8 ?? ?? ?? ?? 8D 43 0A")]
    private readonly ExecuteCommandDelegate _executeCommand = null!;
    
    public enum ChangeBaitReturn
    {
        Success,
        AlreadyEquipped,
        NotInInventory,
        InvalidBait,
        UnknownError,
    }
    
    public static unsafe int HasItem(uint itemId)
        => InventoryManager.Instance()->GetInventoryItemCount(itemId);
    
    public ChangeBaitReturn ChangeBait(uint baitId) {
        if (baitId == Current)
            return ChangeBaitReturn.AlreadyEquipped;
        
        if (baitId == 0 || PlayerResources.Baits.All(b => b.Id != baitId))
            return ChangeBaitReturn.InvalidBait;
        
        if (HasItem(baitId) <= 0)
            return ChangeBaitReturn.NotInInventory;
        
        
        return _executeCommand(701, 4, baitId, 0, 0) == 1 ? ChangeBaitReturn.Success : ChangeBaitReturn.UnknownError;
    }
    
    public ChangeBaitReturn ChangeBait(string baitName) {
        var result = PlayerResources.Baits.FirstOrDefault(b => b.Name == baitName);

        if (result == null)
            return ChangeBaitReturn.InvalidBait;
        
        return ChangeBait((uint)result.Id);
    }
}
