using UnityEditor;
using UnityEngine;

namespace BBGo.FinalPatch
{
    public static class FinalPatchEditorUtility
    {
        public static void Save(this ScriptableObject so)
        {
            EditorUtility.SetDirty(so);
            AssetDatabase.SaveAssets();
        }
    }
}