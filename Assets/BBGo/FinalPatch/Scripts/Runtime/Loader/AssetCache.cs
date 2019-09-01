using UnityEngine;

namespace BBGo.FinalPatch
{
    public class AssetCache
    {
        public string Name { get; internal set; }
        public Object Asset { get; internal set; }
        public AssetBundle AssetBundle { get; internal set; }
        public State AssetState { get; internal set; }
        public int ReferenceCount { get; internal set; }
        public AssetCache[] Dependencies { get; internal set; }

        public enum State
        {
            NotLoad,
            Loading,
            Loaded,
        }
    }
}