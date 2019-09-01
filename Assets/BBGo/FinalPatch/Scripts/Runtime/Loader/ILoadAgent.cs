using System.Threading.Tasks;
using UnityEngine;

namespace BBGo.FinalPatch
{
    public interface ILoadAgent
    {
        AssetCache GetAssetCache(string assetName);
        Task<AssetLoadResult<T>> LoadAssetAsync<T>(string assetName) where T : Object;
        Task UnloadSceneAsync(string sceneName);
    }
}