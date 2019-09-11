using System;
using System.Linq;
using System.Text.RegularExpressions;
using BattleTech;
using BattleTech.Data;
using BattleTech.UI;
using BattleTech.UI.Tooltips;
using Harmony;
using TMPro;
using UnityEngine;

// ReSharper disable InconsistentNaming

public class Patches
{
    internal static void Init()
    {
    }

    [HarmonyPatch(typeof(MechBayMechInfoWidget), "SetData")]
    [HarmonyPatch(new[] {typeof(SimGameState), typeof(MechBayPanel), typeof(DataManager), typeof(MechBayMechUnitElement), typeof(bool), typeof(bool)})]
    public static class MechBayMechInfoWidget_SetData_Patch
    {
        public static void Postfix(GameObject ___rootInfoObj, MechBayMechUnitElement mechElement, DataManager dataManager)
        {
            try
            {
                var label = ___rootInfoObj.GetComponentsInChildren<TextMeshProUGUI>().FirstOrDefault(x => x.name == "txt_mechType");
                var date = dataManager.MechDefs
                    .Where(x => x.Value.Description.Id == mechElement.MechDef.Description.Id)
                    .Select(x => x.Value.MinAppearanceDate).FirstOrDefault();
                // regex because newline
                var matches = Regex.Match(label.text, @"(.+-\s.+)\s(.+)");
                var mechString = matches.Groups[1];
                var mechWeight = matches.Groups[2];
                var labelText = $"{mechString} ({date.Value:MMM yyyy})\n{mechWeight}";
                label.SetText(labelText);
            }
            catch
            {
            }
        }
    }

    // large chassis display on right in mech bay
    [HarmonyPatch(typeof(MechBayChassisInfoWidget), "SetData")]
    public static class MechBayChassisInfoWidget_SetData_Patch
    {
        public static void Postfix(MechBayChassisInfoWidget __instance, GameObject ___rootInfoObj, MechBayChassisUnitElement ___chassisElement)
        {
            try
            {
                var label = ___rootInfoObj.GetComponentsInChildren<TextMeshProUGUI>().FirstOrDefault(x => x.name == "txt_mechType");
                var dm = UnityGameInstance.BattleTechGame.DataManager;
                var date = dm.MechDefs
                    .Where(x => x.Value.Description.Id == ___chassisElement.ChassisDef.Description.Id.Replace("chassisdef", "mechdef"))
                    .Select(x => x.Value.MinAppearanceDate).FirstOrDefault();
                // regex because newline
                var matches = Regex.Match(label.text, @"(.+-\s.+)\s(.+)");
                var mechString = matches.Groups[1];
                var mechWeight = matches.Groups[2];
                var labelText = $"{mechString} ({date.Value:MMM yyyy})\n{mechWeight}";
                label.SetText(labelText);
            }
            catch // silent drop
            {
            }
        }
    }

    // chassis tooltip
    [HarmonyPatch(typeof(TooltipPrefab_Chassis), "SetData")]
    public static class TooltipPrefab_Chassis_SetData_Patch
    {
        public static void Postfix(TooltipPrefab_Chassis __instance, object data, TextMeshProUGUI ___variantNameText)
        {
            try
            {
                if (data is ChassisDef chassisDef)
                {
                    var dm = UnityGameInstance.BattleTechGame.DataManager;
                    var date = dm.MechDefs
                        .Where(x => x.Value.Description.Id == chassisDef.Description.Id.Replace("chassisdef", "mechdef"))
                        .Select(x => x.Value.MinAppearanceDate).FirstOrDefault();
                    var variantName = ___variantNameText.text.Substring(2, ___variantNameText.text.Length - 4);
                    // vanilla has ( ) with spaces
                    ___variantNameText.SetText($"( {variantName} ({date.Value.Year}) )");
                }
            }
            catch // silent drop
            {
            }
        }
    }

    // mech tooltip
    [HarmonyPatch(typeof(TooltipPrefab_Mech), "SetData")]
    public static class TooltipPrefab_Mech_SetData_Patch
    {
        public static void Postfix(TooltipPrefab_Mech __instance, object data, TextMeshProUGUI ___VariantField)
        {
            try
            {
                if (data is MechDef mechDef)
                {
                    var dm = UnityGameInstance.BattleTechGame.DataManager;
                    // not sure why this method gets a shitty MechDef but it does... MinAppearance is 0, minAppearance is 1
                    // so get a real mechdef using its name... this works and is necessary! (1.6.1)
                    var date = dm.MechDefs.Where(x => x.Value.Description.Id == mechDef.Description.Id)
                        .Select(x => x.Value.MinAppearanceDate).FirstOrDefault();
                    var variantName = ___VariantField.text.Substring(2, ___VariantField.text.Length - 4);
                    // vanilla has ( ) with spaces
                    ___VariantField.SetText($"( {variantName} ({date.Value.Year}) )");
                }
            }
            catch // silent drop
            {
            }
        }
    }

    private static void AddYearToVariant(InventoryItemElement theWidget)
    {
        try
        {
            var dm = UnityGameInstance.BattleTechGame.Simulation.DataManager;
            DateTime? date = new DateTime();
            foreach (var def in dm.MechDefs.Where(x => !x.Value.MechTags.Contains("BLACKLISTED")))
                if (def.Value.Description.UIName.Contains(theWidget.mechNameText.text))
                    date = def.Value.MinAppearanceDate;

            theWidget.mechNameText.SetText($"{theWidget.mechNameText.text} ({date.Value.Year})");
        }
        // ReSharper disable once EmptyGeneralCatchClause
        catch
        {
        }
    }

    // these elements are ... odd, so I used an odd approach..
    [HarmonyPatch(typeof(AAR_SalvageScreen), "CalculateAndAddAvailableSalvage")]
    public static class AAR_SalvageScreen_CalculateAndAddAvailableSalvage_Patch
    {
        public static void Postfix()
        {
            var dm = UnityGameInstance.BattleTechGame.DataManager;
            var elements = Resources.FindObjectsOfTypeAll(typeof(InventoryItemElement_NotListView)) as InventoryItemElement_NotListView[];

            foreach (var element in elements)
            {
                foreach (var textMeshProUgui in element.GetComponentsInChildren<TextMeshProUGUI>())
                {
                    if (textMeshProUgui.name != "label-txt_mech") continue;
                    DateTime? date = new DateTime();
                    foreach (var def in dm.MechDefs.Where(x => !x.Value.MechTags.Contains("BLACKLISTED")))
                    {
                        if (def.Value.Description.UIName.EndsWith(textMeshProUgui.text))
                            date = def.Value.MinAppearanceDate;
                    }

                    // some mechs have no date and will throw
                    try
                    {
                        textMeshProUgui.SetText($"{textMeshProUgui.text} ({date.Value.Year})");
                    }

                    // ReSharper disable once EmptyGeneralCatchClause
                    catch
                    {
                    }
                }
            }
        }
    }

    // shop inventory items
    [HarmonyPatch(typeof(InventoryDataObject_ShopMechPart), "SetupLook")]
    public static class InventoryDataObject_ShopMechPart_SetupLook_Patch
    {
        public static void Postfix(InventoryItemElement theWidget)
        {
            AddYearToVariant(theWidget);
        }
    }

    // shop inventory items
    [HarmonyPatch(typeof(InventoryDataObject_ShopFullMech), "SetupLook")]
    public static class InventoryDataObject_ShopFullMech_SetupLook_Patch
    {
        public static void Postfix(InventoryItemElement theWidget)
        {
            AddYearToVariant(theWidget);
        }
    }

    // mech bay inventory items
    [HarmonyPatch(typeof(InventoryDataObject_SalvageFullMech), "SetupLook")]
    public static class InventoryDataObject_SalvageFullMech_SetupLook_Patch
    {
        public static void Postfix(InventoryItemElement theWidget)
        {
            AddYearToVariant(theWidget);
        }
    }

    // mech bay inventory items
    [HarmonyPatch(typeof(InventoryDataObject_SalvageMechPart), "SetupLook")]
    public static class InventoryDataObject_SalvageMechPart_SetupLook_Patch
    {
        public static void Postfix(InventoryItemElement theWidget)
        {
            AddYearToVariant(theWidget);
        }
    }
}
