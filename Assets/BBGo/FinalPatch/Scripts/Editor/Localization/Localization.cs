using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

namespace BBGo.FinalPatch
{
    public static class Localization
    {
        public readonly static string[] LOCALIZATION_LABEL = new string[]
        {
            "English",
            "中文",
        };

        public static LocalizationType CurrentType { get; private set; }
        public static event Action OnLanguageChanged;

        private static Dictionary<string, string> s_localizationMap;

        public static void SetLocalizationType(LocalizationType type)
        {
            string cfgPath = $"{FinalPatchConst.LOCALIZATION_DIR}/{type.ToString().ToLower()}{FinalPatchConst.LOCALIZATION_EXT}";
            bool found = false;
            try
            {
                using (FileStream fs = File.Open(cfgPath, FileMode.Open, FileAccess.Read))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(LocalizationData));
                    LocalizationData data = serializer.Deserialize(fs) as LocalizationData;

                    if (s_localizationMap == null)
                    {
                        s_localizationMap = new Dictionary<string, string>();
                    }
                    s_localizationMap.Clear();
                    foreach (var item in data.items)
                    {
                        s_localizationMap.Add(item.Key, item.Value);
                    }
                    found = true;
                    CurrentType = type;
                    EditorPrefs.SetInt(FinalPatchConst.KEY_FINAL_PATCH_LOCALIZATION, (int)type);
                    OnLanguageChanged?.Invoke();
                }
            }
            catch { }

            if (!found)
            {
                // create localization file
                LocalizationData data = new LocalizationData()
                {
                    items = new List<LocalizationItem>()
                    {
                        new LocalizationItem()
                        {
                            Key="localization_key",
                            Value="localization_value",
                        }
                    }
                };

                string dir = Path.GetDirectoryName(cfgPath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                using (FileStream fs = new FileStream(cfgPath, FileMode.Create, FileAccess.Write))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(LocalizationData));
                    serializer.Serialize(fs, data);
                }

                Debug.LogFormat("Create Localization File:{0}", cfgPath);
                AssetDatabase.Refresh();
                SetLocalizationType(type);
            }
        }

        public static string GetString(string key)
        {
            if (s_localizationMap == null)
            {
                Localization.SetLocalizationType((LocalizationType)EditorPrefs.GetInt(FinalPatchConst.KEY_FINAL_PATCH_LOCALIZATION));
            }
            string value;
            if (!s_localizationMap.TryGetValue(key, out value))
            {
                Debug.LogError($"Get '{key}' failure in language '{CurrentType}'");
            }
            return value;
        }
    }
}