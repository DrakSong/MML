using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BBGo.FinalPatch
{
    public class AssetBundleLoadAgent : ILoadAgent
    {
        private static ClientData s_clientData;
        private static AssetBundleManifest s_manifest;
        private static AssetBundle s_manifestBundle;
        private static FinalPatchData s_finalPatchData;
        private static IPathConverter s_pathConverter;

        public static void Reset()
        {
            s_clientData = null;
            s_manifestBundle?.Unload(true);
            s_manifestBundle = null;
            s_manifest = null;
            s_pathConverter = null;
        }

        private static FinalPatchData GetFinalPatchData()
        {
            if (s_finalPatchData == null)
            {
                s_finalPatchData = FinalPatchData.Load();
            }
            return s_finalPatchData;
        }

        private static IPathConverter GetPathConverter()
        {
            if (s_pathConverter == null)
            {
                if (string.IsNullOrEmpty(GetFinalPatchData().pathConverterType) ||
                    GetFinalPatchData().pathConverterType == "None")
                {
                    Debug.LogErrorFormat("Please set Path Converter in Final Patch Manager Editor");
                    return null;
                }

                try
                {
                    System.Type type = typeof(IPathConverter).Assembly.GetType(GetFinalPatchData().pathConverterType);
                    if (type == null)
                    {

                        Debug.LogErrorFormat("Not found Path Converter Type:'{0}'", GetFinalPatchData().pathConverterType);
                        return null;
                    }

                    s_pathConverter = System.Activator.CreateInstance(type) as IPathConverter;
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                    return null;
                }
            }
            return s_pathConverter;
        }

        private static AssetBundleManifest GetManifest()
        {
            if (s_manifest == null)
            {
                s_manifestBundle = AssetBundle.LoadFromFile(GetClientData().GetAssetBundlePath(FinalPatchConst.ASSET_BUNDLE_MANIFEST_NAME), 0, GetFinalPatchData().bundleBytesOffset);
                if (s_manifestBundle == null)
                {
                    Debug.LogError("Load AssetBundleManifest failure");
                    return null;
                }
                s_manifest = s_manifestBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            }
            return s_manifest;
        }

        private static ClientData GetClientData()
        {
            if (s_clientData == null)
            {
                s_clientData = ClientData.GetOrCreate(false);
            }
            return s_clientData;
        }

        private void FixShadersInEditor(GameObject gameObject)
        {
#if UNITY_EDITOR
            if (gameObject == null)
                return;

            foreach (var mat in gameObject.GetComponentsInChildren<Renderer>(true))
            {
                if (mat == null)
                    continue;

                if (mat.sharedMaterial == null)
                    continue;

                mat.sharedMaterial.shader = Shader.Find(mat.sharedMaterial.shader.name);
            }
#endif
        }

        public async Task<AssetLoadResult<T>> LoadAssetAsync<T>(string assetName) where T : Object
        {
            assetName = GetPathConverter().GetAssetBundleName(assetName);
            await LoadDependenciesAsync(assetName);

            string bundlePath = GetClientData().GetAssetBundlePath(assetName);
            AssetBundle bundle = await AssetBundle.LoadFromFileAsync(bundlePath);
            if (bundle == null)
            {
                Debug.LogError($"Load AssetBundle failure:`{bundlePath}`");
                return null;
            }
            if (bundle.isStreamedSceneAssetBundle)
            {
                return new AssetLoadResult<T>() { Asset = null, Bundle = bundle, };
            }
            else
            {
                T asset = bundle.LoadAsset<T>(Path.GetFileNameWithoutExtension(assetName));
                FixShadersInEditor(asset as GameObject);
                return new AssetLoadResult<T>() { Asset = asset, Bundle = bundle };
            }
        }

        private async Task LoadDependenciesAsync(string assetName)
        {
            string[] dps = GetManifest().GetDirectDependencies(assetName);
            for (int i = 0; i < dps.Length; i++)
            {
                await FinalLoader.LoadAssetAsync<Object>(dps[i]);
            }
        }

        public AssetCache[] GetDependencyCaches(string assetName)
        {
            string[] dependencies = GetManifest().GetDirectDependencies(assetName);
            AssetCache[] dps = new AssetCache[dependencies.Length];
            for (int i = 0; i < dependencies.Length; i++)
            {
                dps[i] = GetAssetCache(dependencies[i]);
            }
            return dps;
        }

        public AssetCache GetAssetCache(string assetPath)
        {
            string assetBundleName = GetPathConverter().GetAssetBundleName(assetPath);
            AssetCache cache;
            if (!FinalLoader.AssetNameCaches.TryGetValue(assetBundleName, out cache))
            {
                cache = new AssetCache()
                {
                    Name = assetBundleName,
                    Dependencies = GetDependencyCaches(assetBundleName),
                };
                FinalLoader.AssetNameCaches.Add(assetBundleName, cache);
            }
            return cache;
        }

        public async Task UnloadSceneAsync(string sceneName)
        {
            AssetCache cache;
            if (!FinalLoader.AssetNameCaches.TryGetValue(GetPathConverter().GetAssetBundleName(sceneName), out cache))
                return;

            await SceneManager.UnloadSceneAsync(sceneName);
            FinalLoader.UnloadAsset(cache);
        }
    }
}