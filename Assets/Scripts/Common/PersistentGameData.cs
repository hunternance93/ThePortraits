using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Newtonsoft.Json;


namespace Assets.Scripts.Common
{
    /* Singleton Class to store data that should persist across all game plays.
     * Examples may include: overall playtime, collectables, number of deaths, etc.
     */
    class PersistentGameData : ISaveFile
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
                saveFileBytes[i] ^= SaveFileConstants.PersistantGameDataXORKey[i % SaveFileConstants.PersistantGameDataXORKey.Length];
            }
            saveString = Convert.ToBase64String(saveFileBytes);
#endif

            string saveFileFullPath = UnityEngine.Application.persistentDataPath + '/' + SaveFileConstants.PersistantGameDataFileName;
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
        public override void LoadFromDisk(string saveFileName = SaveFileConstants.PersistantGameDataFileName)
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
                    throw new FileNotFoundException("Specified save file does not exist");
                }

                rawSaveData = File.ReadAllText(saveFileFullPath);
            }
#if !DEVELOPMENT_BUILD
            byte[] saveFileBytes = Convert.FromBase64String(rawSaveData);
            for (int i = 0; i < saveFileBytes.Length; i++)
            {
                saveFileBytes[i] ^= SaveFileConstants.PersistantGameDataXORKey[i % SaveFileConstants.PersistantGameDataXORKey.Length];
            }
            rawSaveData = Encoding.UTF8.GetString(saveFileBytes);
#endif

            settings = JsonConvert.DeserializeObject<Dictionary<string,string>>(rawSaveData);
        }

        public override bool DoesFileExistOnDisk(bool hardcoreMode = false)
        {
            string saveFileFullPath = UnityEngine.Application.persistentDataPath + '/' + SaveFileConstants.PersistantGameDataFileName;
            if (!Directory.Exists(UnityEngine.Application.persistentDataPath))
            {
                Directory.CreateDirectory(UnityEngine.Application.persistentDataPath);
            }
            return File.Exists(saveFileFullPath);
        }

        public override bool DeleteFileFromDisk(bool hardcoreMode = false)
        {
            string saveFileFullPath = UnityEngine.Application.persistentDataPath + '/' + SaveFileConstants.PersistantGameDataFileName;
            if (DoesFileExistOnDisk())
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
        private static readonly Lazy<PersistentGameData> lazy = new Lazy<PersistentGameData>(() => new PersistentGameData());
        public static PersistentGameData Instance
        {
            get
            {
                return lazy.Value;
            }
        }
    }
}
