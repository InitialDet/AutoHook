using System;
using AutoHook.Resources.Localization;
using AutoHook.Utils;
using Lumina.Excel.GeneratedSheets;
using FishRow = Lumina.Excel.GeneratedSheets.FishParameter;
using ItemRow = Lumina.Excel.GeneratedSheets.Item;

namespace AutoHook.Classes;

public class BaitFishClass : IComparable<BaitFishClass>
{
    public const uint FishingTackleRow = 30;
    
    public string Name; 

    public int Id;

    public BaitFishClass(Item data)
    {
        Id = (int)data.RowId;
        if (data.RowId != 0)
            Name = MultiString.ParseSeStringLumina(data.Name);
        else
            Name = ""; 
    }
    
    public BaitFishClass(FishRow fishRow)
    {
        var itemData = Service.DataManager.GetExcelSheet<ItemRow>()?.GetRow((uint)fishRow.Item) ?? new Item();
        
        Id = (int)itemData.RowId;
        Name = MultiString.ParseSeStringLumina(Service.DataManager.GetExcelSheet<Item>()!.GetRow(itemData.RowId)?.Name);
    }
    
    public BaitFishClass(string name, int id)
    {
        Name = name;
        Id = id;
    }
    
    public BaitFishClass()
    {
        Name = UIStrings.EditMe;
        Id = -1;
    }

    public int CompareTo(BaitFishClass? other)
        => Id.CompareTo(other?.Id ?? 0);
}
