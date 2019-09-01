using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace BBGo.FinalPatch
{
    public class ClientData
    {
        [XmlAttribute]
        public bool IsObsolete { get; set; }
        public List<string> Bundles { get; } = new List<string>();

        [XmlIgnore]
        private Dictionary<string, string> m_pathMap;

        public static ClientData GetOrCreate(bool allowObsolete)
        {
            ClientData clientData = ClientData.LoadAtPath(FinalPatchUtility.GetPersistentClientDataPath());
            if (clientData != null && (allowObsolete || !clientData.IsObsolete))
                return clientData;

            return new ClientData();
        }

        public static ClientData LoadAtPath(string path)
        {
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ClientData));
                    return serializer.Deserialize(fs) as ClientData;
                }
            }
            catch
            {
                return null;
            }
        }

        public string GetAssetBundlePath(string bundleName)
        {
            if (m_pathMap == null)
            {
                m_pathMap = new Dictionary<string, string>();
            }

            string path;
            if (!m_pathMap.TryGetValue(bundleName, out path))
            {
                bool containsBundle = Bundles.Contains(bundleName);
                string rootDir = containsBundle ? Application.persistentDataPath : Application.streamingAssetsPath;
                path = $"{rootDir}/{FinalPatchConst.ASSET_BUNDLE_SUBDIRECTORY_NAME}/{bundleName}";
                m_pathMap.Add(bundleName, path);
            }
            return path;
        }

        internal void UpdateBundle(string name)
        {
            if (!Bundles.Contains(name))
            {
                Bundles.Add(name);
            }
        }

        internal void DeleteBundle(string name)
        {
            Bundles.Remove(name);
        }

        public void SaveToPersistent()
        {
            Save(FinalPatchUtility.GetPersistentClientDataPath());
        }

        private void Save(string path)
        {
            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ClientData));
                serializer.Serialize(fs, this);
            }
        }
    }
}