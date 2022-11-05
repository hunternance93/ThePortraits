
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;


public class JournalEditorUtils: Editor
{
    // Used to convert old component-based journal entries to Scriptable Objects. Here for reference.
    
    
    
    
    // [MenuItem("Atama/Convert Manor Journals")]
    // [ExecuteInEditMode]
    // static void ConvertManorJournals()
    // {
    //     JournalInteractable[] journals = GameObject.FindObjectsOfType<JournalInteractable>();
    //
    //     var index = 1;
    //     
    //     foreach (JournalInteractable journal in journals)
    //     {
    //         JournalEntry newEntry = ScriptableObject.CreateInstance<JournalEntry>();
    //         newEntry.entryName = journal.GetName();
    //         newEntry.entryText = journal.GetText();
    //         newEntry.area = GameArea.InsideManor;
    //         newEntry.areaOrder = index;
    //         index++;
    //
    //         var assetPath = $"Assets/Journals/InsideManor/{newEntry.areaOrder}-{newEntry.entryName}.asset";
    //         Debug.Log("Creating new journal entry: " + assetPath);
    //         // Create new JournalEntryAsset in the asset catalogue
    //         AssetDatabase.CreateAsset(newEntry, assetPath);
    //         AssetDatabase.SaveAssets();
    //         
    //         Undo.RecordObject(journal, "Setting journal entry");
    //         journal.SetJournalEntry(newEntry);
    //         EditorUtility.SetDirty(journal);
    //     }
    // }
    //
    // [MenuItem("Atama/Convert Journals")]
    // [ExecuteInEditMode]
    // static void ConvertJournals()
    // {
    //     Scene scene = EditorSceneManager.GetActiveScene();
    //     Dictionary<GameArea, int> areaToIndex = new Dictionary<GameArea, int>();
    //     areaToIndex.Add(GameArea.Alleyway, 1);
    //     areaToIndex.Add(GameArea.Courtyard, 1);
    //     areaToIndex.Add(GameArea.OutsideManor, 1);
    //     areaToIndex.Add(GameArea.Residential, 1);
    //     
    //     JournalInteractable[] journals = GameObject.FindObjectsOfType<JournalInteractable>();
    //     foreach (JournalInteractable journal in journals)
    //     {
    //         Debug.Log("Converting journal: " + journal.GetName());
    //         // Find parent area
    //         GameObject possibleArea = journal.gameObject;
    //         while (!possibleArea.name.Contains("Area"))
    //         {
    //             possibleArea = possibleArea.transform.parent.gameObject;
    //         }
    //
    //         GameArea area = GameArea.Alleyway;
    //         
    //         switch (possibleArea.name)
    //         {
    //             case "AlleywayArea":
    //                 area = GameArea.Alleyway;
    //                 break;
    //             case "CourtyardArea":
    //                 area = GameArea.Courtyard;
    //                 break;
    //             case "ResidentialArea":
    //                 area = GameArea.Residential;
    //                 break;
    //             case "ManorArea":
    //                 area = GameArea.OutsideManor;
    //                 break;
    //         }
    //         // Create new JournalEntry in the asset catalogue
    //         JournalEntry newEntry = ScriptableObject.CreateInstance<JournalEntry>();
    //         newEntry.entryName = journal.GetName();
    //         newEntry.entryText = journal.GetText();
    //         newEntry.area = area;
    //         newEntry.areaOrder = areaToIndex[area];
    //         areaToIndex[area]++;
    //
    //         var assetPath = $"Assets/Journals/{area.ToString()}/{newEntry.areaOrder}-{newEntry.entryName}.asset";
    //         Debug.Log("Creating new journal entry: " + assetPath);
    //         // Create new JournalEntryAsset in the asset catalogue
    //         AssetDatabase.CreateAsset(newEntry, assetPath);
    //         AssetDatabase.SaveAssets();
    //
    //         Undo.RecordObject(journal, "Setting journal entry");
    //         journal.SetJournalEntry(newEntry);
    //         EditorUtility.SetDirty(journal);
    //     }
    //
    //     EditorSceneManager.SaveScene(scene);
    // }
}
