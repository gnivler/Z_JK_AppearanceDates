using System;
using System.Reflection;
using Harmony;
using Newtonsoft.Json;

public class Core
{
    internal static Settings ModSettings;
    internal static HarmonyInstance harmony;

    public static void Init(string modDir, string settings)
    {
        Log("Starting up");
        harmony = HarmonyInstance.Create("ca.gnivler.BattleTech.Z_JK_AppearanceDates");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
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

        Patches.Init();

        //File.WriteAllText(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName + "settings.json", JSON.ToNiceJSON(ModSettings, new JSONParameters()));
    }

    internal static void Log(object input)
    {
        //FileLog.Log($"[Z_JK_AppearanceDates] {(string.IsNullOrEmpty(input.ToString()) ? "EMPTY" : input)}");
    }
}

public class Settings
{
    public bool enableDebug = true;
    public string modDirectory = "";
}
