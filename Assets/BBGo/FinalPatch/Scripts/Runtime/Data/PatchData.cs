using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Networking;

namespace BBGo.FinalPatch
{
    [Serializable]
    public class PatchData
    {
        [XmlAttribute]
        public string Build { get; set; }
        [XmlAttribute]
        public string Hash { get; set; }
        [XmlArray("Bundles")]
        [XmlArrayItem("Bundle")]
        public List<BundleData> Bundles { get; set; }

        public static async Task<PatchData> DownloadFromUrl(string url)
        {
            UnityWebRequest request = await FinalPatchUtility.WebRequestGet(url);
            if (request == null)
                return null;

            try
            {
                PatchData patchData = null;
                using (MemoryStream ms = new MemoryStream(request.downloadHandler.data))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(PatchData));
                    patchData = serializer.Deserialize(ms) as PatchData;
                }
                return patchData;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }

        public static PatchData LoadAtPath(string path)
        {
            PatchData patchData = null;
            try
            {
                using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(PatchData));
                    patchData = serializer.Deserialize(fs) as PatchData;
                }
            }
            catch { }
            return patchData;
        }

        public void SaveToStreamingAssets()
        {
            Save(FinalPatchUtility.GetStreamingAssetsPatchDataPath());
        }

        public void SaveToPersistent()
        {
            Save(FinalPatchUtility.GetPersistentPatchDataPath());
        }

        public void Save(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(PatchData));
                serializer.Serialize(fs, this);
            }
        }
    }
}