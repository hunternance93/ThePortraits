using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using UnityEngine;

namespace Assets.Scripts.Common
{

    public abstract class ISaveFile
    {
        public Dictionary<string, string> settings = new Dictionary<string,string>();

        // readonly setting
        public abstract bool saveAfterChangeSetting
        {
            get;
        }

        // Callback method after each settings change to decide if we should actually "save" or just keep it in memory for now
        public void SaveToFileAfterSettingChange()
        {
            if (saveAfterChangeSetting)
            {
                SaveToDisk();
            }
        }

        public abstract void SaveToDisk(bool hardcoreMode = false);

        public abstract void LoadFromDisk(string saveFileName);

        public abstract bool DoesFileExistOnDisk(bool hardcoreMode = false);

        public abstract bool DeleteFileFromDisk(bool hardcoreMode = false);

        public string this[string key]
        {
            get { return settings[key]; }
            set { settings[key] = value; SaveToFileAfterSettingChange(); }
        }

        public bool HasKey(string key)
        {
            return settings.ContainsKey(key);
        }

        public void DeleteAll()
        {
            settings = new Dictionary<string, string>();
        }

        public bool DeleteKey(string key)
        {
            return settings.Remove(key);
        }

        public float GetFloat(string key, float defaultValue)
        {
            string value;
            if (settings.TryGetValue(key, out value))
                return float.Parse(value);
            return defaultValue;

        }
        public float GetFloat(string key)
        {
            return float.Parse(settings[key]);
        }
        public bool TryGetFloat(string key, out float outValue)
        {
            outValue = default(float);
            string value;
            bool retResult = settings.TryGetValue(key, out value);
            if (retResult)
                outValue = float.Parse(value);
            return retResult;
        }

        public int GetInt(string key, int defaultValue)
        {
            string value;
            if (settings.TryGetValue(key, out value))
                return int.Parse(value);
            return defaultValue;

        }
        public int GetInt(string key)
        {
            return int.Parse(settings[key]);
        }
        public bool TryGetInt(string key, out int outValue)
        {
            outValue = default(int);
            string value;
            bool retResult = settings.TryGetValue(key, out value);
            if (retResult)
                outValue = int.Parse(value);
            return retResult;
        }

        public string GetString(string key, string defaultValue)
        {
            string value;
            if (settings.TryGetValue(key, out value))
                return value;
            return defaultValue;

        }
        public string GetString(string key)
        {
            return settings[key];
        }
        public bool TryGetString(string key, out string outValue)
        {
            outValue = default(string);
            string value;
            bool retResult = settings.TryGetValue(key, out value);
            if (retResult)
                outValue = value;
            return retResult;
        }

        public T GetObject<T>(string key, T defaultValue)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(settings[key]);
            }
            catch (KeyNotFoundException)
            {
                return defaultValue;
            }
        }
        public T GetObject<T>(string key)
        {
            return JsonConvert.DeserializeObject<T>(settings[key]);
        }
        public T TryGetObject<T>(string key)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(settings[key]);
            }
            catch (KeyNotFoundException)
            {
                return default(T);
            }
        }


        public void SetFloat(string key, float value)
        {
            settings[key] = value.ToString();
            SaveToFileAfterSettingChange();
        }
        public  void SetInt(string key, int value)
        {
            settings[key] = value.ToString();
            SaveToFileAfterSettingChange();
        }
        public void SetString(string key, string value)
        {
            settings[key] = value;
            SaveToFileAfterSettingChange();
        }
        public void SetObject<T>(string key, T toSave)
        {
            settings[key] = JsonConvert.SerializeObject(toSave);
            SaveToFileAfterSettingChange();
        }
    }
}
