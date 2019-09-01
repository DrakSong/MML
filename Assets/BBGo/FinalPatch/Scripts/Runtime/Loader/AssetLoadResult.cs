using UnityEngine;

namespace BBGo.FinalPatch
{
    public class AssetLoadResult<T> where T : Object
    {
        public T Asset { get; set; }
        public AssetBundle Bundle { get; set; }
    }
}