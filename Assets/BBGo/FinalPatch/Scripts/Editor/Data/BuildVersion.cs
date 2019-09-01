using System;
using UnityEditor;
using UnityEngine;

namespace BBGo.FinalPatch
{
    [Serializable]
    public class BuildVersion
    {
        public string name;
        public string description;
        public BuildTarget buildTarget;
        public int version;
        public bool foldout = true;
        [HideInInspector]
        public bool isEdit;
    }
}