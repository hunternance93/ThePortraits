using System.Linq;
using UnityEditor;

public class SteamworksEditor : Editor
{
    [MenuItem("Atama/Steamworks/Enable")]
    static void TurnSteamOn()
    {
        string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        if (!defineSymbols.Contains("STEAMWORKS_ATAMA"))
        {
            defineSymbols += ";STEAMWORKS_ATAMA";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defineSymbols);
        }
    }
    
    [MenuItem("Atama/Steamworks/Disable")]
    static void TurnSteamOff()
    {
        string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        if (defineSymbols.Contains("STEAMWORKS_ATAMA"))
        {
            var defineSymbolsList = defineSymbols.Split(';').ToList();
            defineSymbolsList.Remove("STEAMWORKS_ATAMA");
            defineSymbols = string.Join(";", defineSymbolsList.ToArray());
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defineSymbols);
        }
    }
}
