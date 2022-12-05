using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using Assets.Scripts.Common;

public class SaveGameManager : MonoBehaviour
{
    public bool EndingScene = false;

    private static bool useSecureSave = true;
    private static SecureSaveFile saveFile = SecureSaveFile.Instance;
    
    private void Start()
    {
    }

    public void SaveInventory()
    {
        if(useSecureSave)
        {
            SaveInventoryToFile();
            return;
        }
    }

    public void SaveInventoryToFile()
    {
        SecureSaveFile.Instance.SetObject<List<string>>(SaveFileConstants.SavedInventory, GameManager.instance.Player.Inventory);
        SecureSaveFile.Instance.SaveToDisk();
    }

    private void RetrieveInventory(List<string> inv)
    {
        if (inv == null)
        {
            inv = new List<string>();
        }

        if (GameManager.instance != null && GameManager.instance.Player != null) GameManager.instance.Player.Inventory = inv;
    }

    public void SaveJournal()
    {
        if (useSecureSave)
        {
            List<string> journalIds = GameManager.instance.Player.JournalEntries.Select(entry => entry.internalId).ToList();
            Debug.Log("Saving journal: " + journalIds);
            SecureSaveFile.Instance.SetObject<List<string>>(SaveFileConstants.Journal, journalIds);
            SecureSaveFile.Instance.SaveToDisk();
        }
    }
    
    private void RetrieveJournal(List<string> journalEntries)
    {
        if (GameManager.instance != null && GameManager.instance.Player != null && GameManager.instance.allJournalEntries != null)
        {
            GameManager.instance.Player.JournalEntries = new List<JournalEntry>();
        
            if (journalEntries == null) return;

            foreach (JournalEntry journal in GameManager.instance.allJournalEntries)
            {
                if (journalEntries.Contains(journal.internalId))
                {
                    Debug.Log("Loaded journal " + journal.entryName);
                    GameManager.instance.Player.JournalEntries.Add(journal); 
                }
            }
        }
    }

    private void SetPlayerOnSpawnPoint(string spawnPoint)
    {
        if (string.IsNullOrEmpty(spawnPoint)) return;

        try { GameObject.Find(spawnPoint).GetComponent<Checkpoint>().SpawnPlayer(); }
        catch (Exception e)
        {
            Debug.LogWarning(e.ToString() +  ", Failed to find spawnPoint " + spawnPoint + ". Note that this may be because of an error in configuration for Checkpoint, like an array missing an element.");
        }
    }

    public void SaveGame(Transform checkPoint)
    {
        Debug.Log("Saving game");
        if (GameManager.instance.CurrentGameMode == GameManager.GameMode.Hardcore) return;
        if (useSecureSave)
        {
            saveFile[SaveFileConstants.CurrentScene] = SceneManager.GetActiveScene().name;

            saveFile[SaveFileConstants.CurrentCheckpoint] = checkPoint.gameObject.name;
            SaveInventoryToFile();
            SaveJournal();
            saveFile.SaveToDisk();
            return;
        }
    }

    public void ReloadDataFromFile(bool hardcoreMode = false)
    {
        saveFile.LoadFromDisk(hardcoreMode ? SaveFileConstants.SecureSaveFileHardcord : SaveFileConstants.SecureSaveFileOneName);
    }


    //Callback after the scene is loaded to load the appropriate save data into the system where it's needed
    public void LoadAllData()
    {
        Debug.Log("LoadAllData");
        if (!EndingScene && saveFile.HasKey(SaveFileConstants.CurrentCheckpoint))
        {
            SetPlayerOnSpawnPoint(saveFile[SaveFileConstants.CurrentCheckpoint]);
        }
        RetrieveJournal(saveFile.TryGetObject<List<string>>(SaveFileConstants.Journal));
        RetrieveInventory(saveFile.TryGetObject<List<string>>(SaveFileConstants.SavedInventory));
    }

    public void SaveGameScene(string sceneName)
    {
        //if (GameManager.instance.CurrentGameMode == GameManager.GameMode.Hardcore) return;
        if (useSecureSave)
        {
            saveFile[SaveFileConstants.CurrentScene] = sceneName;

            saveFile.DeleteKey(SaveFileConstants.CurrentCheckpoint);
            SaveInventoryToFile();
            SaveJournal();
            saveFile.SaveToDisk(GameManager.instance.CurrentGameMode == GameManager.GameMode.Hardcore);
            return;
        }
    }

    public void LoadGame()
    {
        Debug.Log("LoadingGame");
        if (useSecureSave)
        {
            try
            {
                saveFile.LoadFromDisk(SaveFileConstants.SecureSaveFileOneName);

                string sceneName = saveFile[SaveFileConstants.CurrentScene];
                if (GameManager.instance.CurrentGameMode == GameManager.GameMode.Hardcore && sceneName != "Level1V2")
                {
                    Debug.LogWarning("Attempting to load non-Level1V2 scene for Hardcore. Something is wrong! Loading Level1V2 instead.");
                    LevelLoader.LoadLevel("Level1V2", false);
                    return;
                }
                LevelLoader.LoadLevel(sceneName, true);

            }
             catch (FileNotFoundException)
            {
                LevelLoader.LoadLevel("Level1V2", false);
                // We couldn't find a file, assume there is no save data
                return;
            }
            return;
        }
    }


    // Singleton setup
    private static SaveGameManager mInstance;
    public static SaveGameManager Instance
    {
        get
        {
            if (mInstance == null && Application.isPlaying)
            {
                GameObject go = new GameObject("SaveGameManager");
                mInstance = go.AddComponent<SaveGameManager>();
            }
            return mInstance;
        }
    }
}
