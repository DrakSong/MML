using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BBGo.FinalPatch
{
    public static class FinalLoader
    {
        public static Dictionary<string, AssetCache> AssetNameCaches { get; } = new Dictionary<string, AssetCache>();
        public static Dictionary<Object, AssetCache> AssetObjectCaches { get; } = new Dictionary<Object, AssetCache>();

        private static bool s_initialized;
        private static FinalPatchData s_data;

        internal static void Init(FinalPatchData data)
        {
            if (s_initialized)
                return;

            s_initialized = true;
            s_data = data;
        }

        internal static void Reset()
        {
            List<Object> allAssets = new List<Object>();
            foreach (var asset in AssetObjectCaches.Keys)
            {
                allAssets.Add(asset);
            }
            foreach (var asset in allAssets)
            {
                UnloadAsset(asset);
            }
            AssetBundleLoadAgent.Reset();
        }

        private static ILoadAgent CreateAgent()
        {
            if (Application.isEditor && s_data.applyEditorMode)
            {
                return new AssetDatabaseLoadAgent();
            }
            else
            {
                return new AssetBundleLoadAgent();
            }
        }

        public static async Task LoadSceneAsync(string sceneName, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Single)
            {
                //unload all assets before a scene loaded with single mode
                FinalPool.Reset();
                Reset();
            }
            await LoadAssetAsync<Object>(sceneName);
            SceneManager.LoadScene(sceneName, mode);
        }


        public static async Task UnloadSceneAsync(string sceneName)
        {
            await CreateAgent().UnloadSceneAsync(sceneName);
        }

        public static async Task<T> LoadAssetAsync<T>(string assetName) where T : Object
        {
            if (FinalPatcher.PatchReport == null)
            {
                Debug.LogError("Do you forget to patch? Refer to 'FinalPatcher.cs' for more details.");
                return default(T);
            }
            else
            {
                await new FinalPatcher.WaitForPatchFinish();
            }
            AssetCache assetCache = CreateAgent().GetAssetCache(assetName);
            switch (assetCache.AssetState)
            {
                case AssetCache.State.NotLoad:
                    return await LoadNewAssetAsync<T>(assetCache);
                case AssetCache.State.Loading:
                case AssetCache.State.Loaded:
                    return await WaitForLoading<T>(assetCache);
                default:
                    return null;
            }
        }

        private static async Task<T> LoadNewAssetAsync<T>(AssetCache assetCache) where T : Object
        {
            assetCache.AssetState = AssetCache.State.Loading;
            AssetLoadResult<T> result = await CreateAgent().LoadAssetAsync<T>(assetCache.Name);
            assetCache.Asset = result.Asset;
            assetCache.AssetBundle = result.Bundle;
            assetCache.AssetState = AssetCache.State.Loaded;
            OnNewAssetLoadFinish(assetCache);
            return LoadFinish<T>(assetCache);
        }

        private static void OnNewAssetLoadFinish(AssetCache assetCache)
        {
            if (assetCache.Asset == null)
                return;

            if (AssetObjectCaches.ContainsKey(assetCache.Asset))
                return;

            AssetObjectCaches.Add(assetCache.Asset, assetCache);
        }

        private static async Task<T> WaitForLoading<T>(AssetCache assetCache) where T : Object
        {
            while (assetCache.AssetState == AssetCache.State.Loading)
            {
                await new WaitForUpdate();
            }
            return LoadFinish<T>(assetCache);
        }

        private static T LoadFinish<T>(AssetCache assetCache) where T : Object
        {
            assetCache.ReferenceCount++;
            return assetCache.Asset as T;
        }

        public static void UnloadAsset(Object asset)
        {
            if (asset == null)
                return;

            AssetCache cache;
            if (!AssetObjectCaches.TryGetValue(asset, out cache))
                return;

            UnloadAsset(cache);
        }

        internal static void UnloadAsset(AssetCache cache)
        {
            if (cache == null)
                return;

            if (--cache.ReferenceCount <= 0)
            {
                //完全卸载
                cache.AssetBundle?.Unload(true);
                AssetNameCaches.Remove(cache.Name);
                if (cache.Asset != null)
                {
                    AssetObjectCaches.Remove(cache.Asset);
                }
                foreach (var dep in cache.Dependencies)
                {
                    UnloadAsset(dep);
                }
            }
        }
    }
}