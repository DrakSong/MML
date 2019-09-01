using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BBGo.FinalPatch
{
    public class AssetDatabaseLoadAgent : ILoadAgent
    {
        public async Task<AssetLoadResult<T>> LoadAssetAsync<T>(string assetName) where T : Object
        {
#if UNITY_EDITOR
            await new WaitForUpdate();
            T asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetName);
            if (asset == null)
            {
                Debug.LogError($"load asset failure in Editor Mode,assetName:`{assetName}`");
                return null;
            }
            return new AssetLoadResult<T>() { Asset = asset, Bundle = null };
#else
            Debug.LogError($"load asset failure: Can't use EdtiorMode to load asset in non-editor environment");
            return null;
#endif
        }

        public AssetCache GetAssetCache(string assetPath)
        {
            AssetCache cache;
            if (!FinalLoader.AssetNameCaches.TryGetValue(assetPath, out cache))
            {
                cache = new AssetCache()
                {
                    Name = assetPath,
                    Dependencies = new AssetCache[0],
                };
                FinalLoader.AssetNameCaches.Add(assetPath, cache);
            }
            return cache;
        }

        public async Task UnloadSceneAsync(string sceneName)
        {
            AssetCache cache;
            if (!FinalLoader.AssetNameCaches.TryGetValue(sceneName, out cache))
                return;

            await SceneManager.UnloadSceneAsync(sceneName);
            FinalLoader.UnloadAsset(cache);
        }
    }
}