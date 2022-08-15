using System;
using Dalamud;
using Dalamud.Data;
using Lumina.Text;

namespace AutoHook.Utils;

public readonly struct MultiString
{
    public static string ParseSeStringLumina(SeString? luminaString)
        => luminaString == null ? string.Empty : Dalamud.Game.Text.SeStringHandling.SeString.Parse(luminaString.RawData).TextValue;

    public readonly string English;
    public readonly string German;
    public readonly string French;
    public readonly string Japanese;
    public readonly string ChineseSimplified;

    public string this[ClientLanguage lang]
        => Name(lang);

    public override string ToString()
        => Name(ClientLanguage.English);

    public string ToWholeString()
        => $"{English}|{German}|{French}|{Japanese}|{ChineseSimplified}";

    public MultiString(string en, string de, string fr, string jp,string zh)
    {
        English = en;
        German = de;
        French = fr;
        Japanese = jp;
        ChineseSimplified = zh;
    }


    public static MultiString FromPlaceName(DataManager gameData, uint id)
    {
        var en = ParseSeStringLumina(gameData.GetExcelSheet<Lumina.Excel.GeneratedSheets.PlaceName>(ClientLanguage.English)!.GetRow(id)?.Name);
        var de = ParseSeStringLumina(gameData.GetExcelSheet<Lumina.Excel.GeneratedSheets.PlaceName>(ClientLanguage.German)!.GetRow(id)?.Name);
        var fr = ParseSeStringLumina(gameData.GetExcelSheet<Lumina.Excel.GeneratedSheets.PlaceName>(ClientLanguage.French)!.GetRow(id)?.Name);
        var jp = ParseSeStringLumina(gameData.GetExcelSheet<Lumina.Excel.GeneratedSheets.PlaceName>(ClientLanguage.Japanese)!.GetRow(id)?.Name);
        var zh = ParseSeStringLumina(gameData.GetExcelSheet<Lumina.Excel.GeneratedSheets.PlaceName>(ClientLanguage.ChineseSimplified)!.GetRow(id)?.Name);
        return new MultiString(en, de, fr, jp, zh);
    }

    public static MultiString FromItem(DataManager gameData, uint id)
    {
        var en = ParseSeStringLumina(gameData.GetExcelSheet<Lumina.Excel.GeneratedSheets.Item>(ClientLanguage.English)!.GetRow(id)?.Name);
        var de = ParseSeStringLumina(gameData.GetExcelSheet<Lumina.Excel.GeneratedSheets.Item>(ClientLanguage.German)!.GetRow(id)?.Name);
        var fr = ParseSeStringLumina(gameData.GetExcelSheet<Lumina.Excel.GeneratedSheets.Item>(ClientLanguage.French)!.GetRow(id)?.Name);
        var jp = ParseSeStringLumina(gameData.GetExcelSheet<Lumina.Excel.GeneratedSheets.Item>(ClientLanguage.Japanese)!.GetRow(id)?.Name);
        var zh = ParseSeStringLumina(gameData.GetExcelSheet<Lumina.Excel.GeneratedSheets.Item>(ClientLanguage.ChineseSimplified)!.GetRow(id)?.Name);
        return new MultiString(en, de, fr, jp, zh);
    }

    private string Name(ClientLanguage lang)
        => lang switch
        {
            ClientLanguage.English => English,
            ClientLanguage.German => German,
            ClientLanguage.Japanese => Japanese,
            ClientLanguage.French => French,
            ClientLanguage.ChineseSimplified => ChineseSimplified,
            _ => throw new ArgumentException(),
        };

    public static readonly MultiString Empty = new(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
}
