using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BBGo.FinalPatch
{
    public class FinalPatchEditorData : ScriptableObject
    {
        public int activeTab;
        public List<BuildVersion> builds;
        public bool extractDuplicatedDependency = true;
        public string buildCallbackType;
        public string assetPath = "Assets/Bundles";

        public static FinalPatchEditorData Load()
        {
            FinalPatchEditorData data = Resources.Load<FinalPatchEditorData>("FinalPatchEditorData");
            if (data == null)
            {
#if UNITY_EDITOR
                data = CreateInstance<FinalPatchEditorData>();
                string dir = Path.GetDirectoryName(FinalPatchConst.FINAL_PATCH_EDITOR_DATA_PATH);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                UnityEditor.AssetDatabase.CreateAsset(data, FinalPatchConst.FINAL_PATCH_EDITOR_DATA_PATH);
                UnityEditor.AssetDatabase.Refresh();
#endif
            }
            return data;
        }
    }
}