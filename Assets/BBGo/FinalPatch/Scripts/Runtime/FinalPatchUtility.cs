using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace BBGo.FinalPatch
{
    public static class FinalPatchUtility
    {
        public static string GetBuildDataPath(string outputPath)
        {
            return string.Format($"{outputPath}/{FinalPatchConst.BUILD_DATA_FILE_NAME}");
        }

        public static string GetDeployDataPath(string outputPath)
        {
            return string.Format($"{outputPath}/{FinalPatchConst.DEPLOY_DATA_FILE_NAME}");
        }

        public static string GetOutputFullPath(string outputPlatformPath, int version)
        {
            return string.Format($"{outputPlatformPath}/{FinalPatchConst.FULL_DIRECTORY_NAME}/{version}");
        }

        public static string GetFullPatchDataFilePath(string outputPlatformPath, int version)
        {
            return $"{GetOutputFullPath(outputPlatformPath, version)}/{FinalPatchConst.PATCH_DATA_FILE_NAME}";
        }

        public static string GetOutputPackagePath(string outputPlatformPath, int version)
        {
            return string.Format($"{outputPlatformPath}/{FinalPatchConst.PACKAGE_DIRECTORY_NAME}/{version}");
        }

        public static string GetPackagePatchDataFilePath(string outputPlatformPath, int version)
        {
            return $"{GetOutputPackagePath(outputPlatformPath, version)}/{FinalPatchConst.PATCH_DATA_FILE_NAME}";
        }

        public static string GetPersistentBundlePath(string bundleName)
        {
            return $"{Application.persistentDataPath}/{FinalPatchConst.ASSET_BUNDLE_SUBDIRECTORY_NAME}/{bundleName}";
        }

        public static string GetPersistentClientDataPath()
        {
            return $"{Application.persistentDataPath}/{FinalPatchConst.ASSET_BUNDLE_SUBDIRECTORY_NAME}/{FinalPatchConst.CLIENT_DATA_FILE_NAME}";
        }

        public static string GetStreamingAssetsClientDataPath()
        {
            return $"{Application.streamingAssetsPath}/{FinalPatchConst.ASSET_BUNDLE_SUBDIRECTORY_NAME}/{FinalPatchConst.CLIENT_DATA_FILE_NAME}";
        }

        public static string GetPersistentPatchDataPath()
        {
            return $"{Application.persistentDataPath}/{FinalPatchConst.ASSET_BUNDLE_SUBDIRECTORY_NAME}/{FinalPatchConst.PATCH_DATA_FILE_NAME}";
        }

        public static string GetStreamingAssetsPatchDataPath()
        {
            return $"{Application.streamingAssetsPath}/{FinalPatchConst.ASSET_BUNDLE_SUBDIRECTORY_NAME}/{FinalPatchConst.PATCH_DATA_FILE_NAME}";
        }

        public static async Task<UnityWebRequest> WebRequestGet(string url)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            await request.SendWebRequest();
            if (request.isHttpError || request.isNetworkError)
            {
                Debug.LogError($"{request.error}\nURL:'{url}'");
                return null;
            }
            return request;
        }
    }
}