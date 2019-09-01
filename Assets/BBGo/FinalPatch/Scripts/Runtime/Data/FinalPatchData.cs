using System.IO;
using UnityEngine;

namespace BBGo.FinalPatch
{
    public class FinalPatchData : ScriptableObject
    {
        public bool applyEditorMode = true;
        public ulong bundleBytesOffset;
        public string pathConverterType;
        public bool patchAutoPatch = true;
        public int patchRetryCount = 3;
        public int patchRetryInterval = 1;
        public int poolMaxCache = 5;
        public bool poolEnableAutoRelease = true;
        public float poolAutoReleaseDuration = 30;

        public static FinalPatchData Load()
        {
            FinalPatchData data = Resources.Load<FinalPatchData>("FinalPatchData");
            if (data == null)
            {
#if UNITY_EDITOR
                data = CreateInstance<FinalPatchData>();
                string dir = Path.GetDirectoryName(FinalPatchConst.FINAL_PATCH_DATA_PATH);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                UnityEditor.AssetDatabase.CreateAsset(data, FinalPatchConst.FINAL_PATCH_DATA_PATH);
                UnityEditor.AssetDatabase.Refresh();
#endif
            }
            return data;
        }
    }
}