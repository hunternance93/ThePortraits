using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Newtonsoft.Json;
using UnityEngine;

namespace Assets.Scripts.Common
{
    /* Singleton Class to store data that should be stored in a save file that would be considered "secure" and un-editable by the end user.
     * For example: brightness settings, preferred language, etc. need not be stored here.
     * But data such as current player location, items in inventory, etc. should be stored here, as this has some extra security to the file it saves.
     * 
     * NOTE: This is not actually "secure" and should more be considered "obfuscated". To get true security here would be a lot more complex than this project. 
     *      The intent is to prevent a large majority of save-file editing. Expect that some data may be easily read by determined players, 
     *      so don't store passwords or spoilers in here.
     * 
     * TODO: consider expanding this to multiple save files?
     */
    public class SecureSaveFile : ISaveFile
    {

        private static object fileLock = new object();

        // Specifies to the inherited class that we want to save to disk after every setting change
        public override bool saveAfterChangeSetting
        {
            get
            {
                return false;
            }
        }

        /* Writes the current dictionary to Disk.
         * We first write everything to a json, then we have that converted to a byte-array that is XOR'd with a "private key"
         */
        public override void SaveToDisk(bool hardcoreMode = false)
        {
            string saveString = JsonConvert.SerializeObject(settings);

#if !DEVELOPMENT_BUILD
            byte[] saveFileBytes = Encoding.UTF8.GetBytes(saveString);

            // XOR the data with our key from our constants.
            // Using int here as our counter creates a 2.1GB limit of the data, but we should honestly never go above that
            for(int i = 0; i < saveFileBytes.Length; i++)
            {
                saveFileBytes[i] ^= SaveFileConstants.SecureSaveFileOneXORKey[i % SaveFileConstants.SecureSaveFileOneXORKey.Length];
            }
            saveString = Convert.ToBase64String(saveFileBytes);
#endif
            string saveFileFullPath = UnityEngine.Application.persistentDataPath + '/';
            if (hardcoreMode)
            {
                saveFileFullPath += SaveFileConstants.SecureSaveFileHardcord;
            }
            else
            {
                saveFileFullPath += SaveFileConstants.SecureSaveFileOneName;
            }
            if (!Directory.Exists(UnityEngine.Application.persistentDataPath))
            {
                Directory.CreateDirectory(UnityEngine.Application.persistentDataPath);
            }

            lock (fileLock)
            {

                if (!File.Exists(saveFileFullPath))
                {
                    File.Create(saveFileFullPath).Dispose();
                }

                File.WriteAllText(saveFileFullPath, saveString);
            }
        }


        // On first creation of the instance, we should look in the "insecure" save file for the expected save data we are loading, if there's nothing there, assume save file 1.
        // Loads the specified save file from Disk.
        public override void LoadFromDisk(string saveFileName = SaveFileConstants.SecureSaveFileOneName)
        {
            string saveFileFullPath = UnityEngine.Application.persistentDataPath + '/' + saveFileName;
            if (!Directory.Exists(UnityEngine.Application.persistentDataPath))
            {
                Directory.CreateDirectory(UnityEngine.Application.persistentDataPath);
            }

            string rawSaveData = String.Empty;

            lock (fileLock)
            {
                if (!File.Exists(saveFileFullPath))
                {
                    if (!Application.isEditor && saveFileName == SaveFileConstants.SecureSaveFileOneName)
                    {
                        throw new FileNotFoundException("Specified save file does not exist");
                    }
                    else 
                    {
                        Debug.LogWarning("Save File not found");
                        return;
                    }
                }

                rawSaveData = File.ReadAllText(saveFileFullPath);
            }
#if !DEVELOPMENT_BUILD
            try
            {
                byte[] saveFileBytes = Convert.FromBase64String(rawSaveData);
                for (int i = 0; i < saveFileBytes.Length; i++)
                {
                    saveFileBytes[i] ^= SaveFileConstants.SecureSaveFileOneXORKey[i % SaveFileConstants.SecureSaveFileOneXORKey.Length];
                }
                rawSaveData = Encoding.UTF8.GetString(saveFileBytes);
            }
            catch(FormatException formatException)
            {
                Debug.LogWarning("We were unable to decrypt the save file. This means it's either corrupted or an un-encrypted save file: " + formatException.ToString());
            }
#endif
            try
            {
                settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(rawSaveData);
            }
            catch(JsonReaderException jsonException)
            {
                Debug.LogWarning("We were unable to load the data. This means it's either still encoded or invalid json. Will try to decrypt. Exception details: " + jsonException.ToString());


                byte[] saveFileBytes = Convert.FromBase64String(rawSaveData);
                for (int i = 0; i < saveFileBytes.Length; i++)
                {
                    saveFileBytes[i] ^= SaveFileConstants.SecureSaveFileOneXORKey[i % SaveFileConstants.SecureSaveFileOneXORKey.Length];
                }
                rawSaveData = Encoding.UTF8.GetString(saveFileBytes);
                settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(rawSaveData);
            }
        }

        public override bool DoesFileExistOnDisk(bool hardcoreMode = false)
        {
            string saveFile = hardcoreMode
                ? SaveFileConstants.SecureSaveFileHardcord
                : SaveFileConstants.SecureSaveFileOneName;
            string saveFileFullPath = UnityEngine.Application.persistentDataPath + '/' + saveFile;
            if (!Directory.Exists(UnityEngine.Application.persistentDataPath))
            {
                Directory.CreateDirectory(UnityEngine.Application.persistentDataPath);
            }
            return File.Exists(saveFileFullPath);
        }

        public override bool DeleteFileFromDisk(bool hardcoreMode = false)
        {
            string saveFile = hardcoreMode
                ? SaveFileConstants.SecureSaveFileHardcord
                : SaveFileConstants.SecureSaveFileOneName;
            string saveFileFullPath = UnityEngine.Application.persistentDataPath + '/' + saveFile;
            if (DoesFileExistOnDisk(hardcoreMode))
            {
                lock (fileLock)
                {
                    File.Delete(saveFileFullPath);
                }
                return true;
            }
            else
            {
                return false;
            }
        }


        // Singleton setup
        private static readonly Lazy<SecureSaveFile> lazy = new Lazy<SecureSaveFile>(() => new SecureSaveFile());
        public static SecureSaveFile Instance
        {
            get
            {
                return lazy.Value;
            }
        }
    }
}
