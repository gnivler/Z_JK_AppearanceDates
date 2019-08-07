using System;
using System.Reflection;
using BattleTech.Save.Core;
using BattleTech.UI;
using Harmony;
using Newtonsoft.Json;

public class Core
{
    internal static Settings ModSettings;
    internal static HarmonyInstance harmony;

    public static void Init(string modDir, string settings)
    {
        Core.Log("Starting up");
        //Assembly.LoadFile(@"N:\SteamLibrary\steamapps\common\BATTLETECH\Mods\DevMod\0Harmony.dll");
        harmony = HarmonyInstance.Create("ca.gnivler.BattleTech.DevMod");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
        //if (ModSettings.enableDebug)
        //HarmonyInstance.DEBUG = true;
        // read settings
        try
        {
            ModSettings = JsonConvert.DeserializeObject<Settings>(settings);
            ModSettings.modDirectory = modDir;
        }
        catch (Exception)
        {
            ModSettings = new Settings();
        }

        //Log("PATCHING");   
        //var redMethod = AccessTools.Method(typeof(CombatHUDStatusStackItem), nameof(CombatHUDStatusStackItem.Init));
        //var redPatch = AccessTools.Method(typeof(Patches), nameof(ManualPatches.RedPatch.Postfix));
        //Log($"Patched? {harmony.GetPatchInfo(redMethod) != null}");
        //harmony.Patch(redMethod, null, new HarmonyMethod(redPatch));
        //Log($"Patched? {harmony.GetPatchInfo(redMethod) != null}");

        ManualPatches.Init();
        //Clear();
        //PrintObjectFields(ModSettings, "ModSettings");
    }

    internal static void Log(string input)
    {
        FileLog.Log($"[DevMod] {(string.IsNullOrEmpty(input) ? "EMPTY" : input)}");
    }

    // internal static void PrintObjectFields(object obj, string name)
    // {
    //     LogDebug($"[START {name}]");
    //
    //     var settingsFields = typeof(Settings)
    //         .GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
    //     foreach (var field in settingsFields)
    //     {
    //         if (field.GetValue(obj) is IEnumerable &&
    //             !(field.GetValue(obj) is string))
    //         {
    //             LogDebug(field.Name);
    //             foreach (var item in (IEnumerable) field.GetValue(obj))
    //             {
    //                 LogDebug("\t" + item);
    //             }
    //         }
    //         else
    //         {
    //             LogDebug($"{field.Name,-30}: {field.GetValue(obj)}");
    //         }
    //     }
    //
    //     LogDebug($"[END {name}]");
    // }
}

public class Settings
{
    public bool enableDebug = true;
    public string modDirectory = "";
}
