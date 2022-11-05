using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.SceneManagement;
using System;

public class JournalEditor : Editor
{
    private static string path = "Assets\\JournalEntryText\\";

    [MenuItem("Atama/Generate Journal File For Scene")]
    static void WriteSceneFile()
    {
        try
        {
            JournalInteractable[] journalList = GameObject.FindObjectsOfType<JournalInteractable>();
            string temp;
            string output = "";
            foreach (JournalInteractable journal in journalList)
            {
                temp = "\"" + journal.GetName() + "\":\"" + journal.GetText() + "\"";
                temp = temp.Replace("\n", "\\n");
                if (!output.Contains(temp)) output += temp + "\n";
            }
            File.WriteAllText(path + EditorSceneManager.GetActiveScene().name + " Journal Entries.txt", output);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to write journal file: " + e.StackTrace);
        }
    }
}
