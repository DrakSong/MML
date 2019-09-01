using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Networking;

namespace BBGo.FinalPatch
{
    public static class FinalPatcher
    {
        public static bool IsPatched { get { return PatchReport != null && PatchReport.Result == PatchResult.Success; } }
        public static PatchReport PatchReport { get; private set; }

        private static bool s_initialized;
        private static FinalPatchData s_data;
        private static string s_deployDataUrl;
        private static string s_channel;

        internal static void Init(FinalPatchData data, string deployDataUrl, string channel)
        {
            if (s_initialized)
                return;

            s_initialized = true;
            s_data = data;
            s_deployDataUrl = deployDataUrl;
            s_channel = channel;

            if (s_data.patchAutoPatch)
            {
                AutoPatch();
            }
        }

        internal static void Reset()
        {
            PatchReport = null;
        }

        public static async void AutoPatch()
        {

            PatchReport = await Collect();
            int retryCount = s_data.patchRetryCount;
            while (PatchReport.Result > PatchResult.Failure_OutOfDate)
            {
                if (retryCount <= 0)
                    break;

                Debug.LogError($"Patch Collect Failure, Retry in {s_data.patchRetryInterval} seconds");
                await new WaitForSeconds(s_data.patchRetryInterval);
                retryCount--;
                PatchReport = await Collect();
            }

            if (PatchReport.Result > PatchResult.Failure_OutOfDate)
            {
                Debug.LogError("Patch Failure");
                return;
            }

            if (PatchReport.Result >= PatchResult.Success)
                return;

            Debug.Log($"Collected {PatchReport.TotalSize / 1024f}KB resource to download");
            await Patch(PatchReport);
            retryCount = s_data.patchRetryCount;
            while (PatchReport.Result != PatchResult.Success)
            {
                if (retryCount <= 0)
                    break;

                Debug.LogError($"Patch Failure, Retry in {s_data.patchRetryInterval} seconds");
                await new WaitForSeconds(s_data.patchRetryInterval);
                retryCount--;
                await Patch(PatchReport);
            }

            if (PatchReport.Result != PatchResult.Success)
            {
                Debug.LogError("Patch Failure");
            }
        }

        private static async Task<DeployData> DownloadDeployData()
        {
            UnityWebRequest request = await FinalPatchUtility.WebRequestGet(s_deployDataUrl);
            if (request == null)
                return null;

            try
            {
                using (MemoryStream ms = new MemoryStream(request.downloadHandler.data))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(DeployData));
                    return serializer.Deserialize(ms) as DeployData;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }

        public static async Task<PatchReport> Collect()
        {
            PatchReport patchReport = new PatchReport();
            if (s_data.applyEditorMode)
            {
                Debug.Log("Patched in Editor Mode");
                patchReport.Result = PatchResult.Success;
                return patchReport;
            }

            DeployData deployData = await DownloadDeployData();
            if (deployData == null)
            {
                Debug.LogErrorFormat("Deploy Data Not Found");
                patchReport.Result = PatchResult.Failure_NotFoundDeployData;
                return patchReport;
            }

            ChannelData channelData = deployData.Channels.Find(c => c.Name == s_channel);
            if (channelData == null)
            {
                Debug.LogErrorFormat("Channel Not Found:'{0}'", s_channel);
                patchReport.Result = PatchResult.Failure_NotFoundChannel;
                return patchReport;
            }

            if (channelData.Version == 0)
            {
                Debug.Log("No patch released");
                patchReport.Result = PatchResult.Success;
                return patchReport;
            }

            string patchDataUrl = $"{channelData.URL}/{channelData.Build}/{channelData.Version}/{FinalPatchConst.PATCH_DATA_FILE_NAME}";
            PatchData serverPatchData = await PatchData.DownloadFromUrl(patchDataUrl);
            if (serverPatchData == null)
            {
                Debug.LogErrorFormat("Patch Data Not Found:'{0}'", patchDataUrl);
                patchReport.Result = PatchResult.Failure_NotFoundServerPatchData;
                return patchReport;
            }

            PatchData persistentPatchData = PatchData.LoadAtPath(FinalPatchUtility.GetPersistentPatchDataPath());
            PatchData packagePatchData = PatchData.LoadAtPath(FinalPatchUtility.GetStreamingAssetsPatchDataPath());
            if (packagePatchData?.Hash == serverPatchData.Hash)
            {
                // use package bundles
                Debug.LogFormat("Newest Package Version:'{0}[{1}]'", channelData.Build, channelData.Version);
                if (persistentPatchData != null)
                {
                    // reset build
                    persistentPatchData.Build = null;
                    persistentPatchData.SaveToPersistent();

                    // set client data obsolete
                    ClientData persistentClientData = ClientData.LoadAtPath(FinalPatchUtility.GetPersistentClientDataPath());
                    persistentClientData.IsObsolete = true;
                    persistentClientData.SaveToPersistent();
                }
                patchReport.Result = PatchResult.Success;
                return patchReport;
            }

            if (persistentPatchData != null)
            {
                if (persistentPatchData.Build == null)
                {
                    persistentPatchData.Build = channelData.Build;
                }

                if (persistentPatchData.Build != channelData.Build)
                {
                    Debug.LogErrorFormat("Current Build Is Out Of Date:'{0}[{1}]'", persistentPatchData.Build, channelData.Version);
                    patchReport.Result = PatchResult.Failure_OutOfDate;
                    return patchReport;
                }
            }

            ClientData clientData = ClientData.GetOrCreate(true);
            if (persistentPatchData?.Hash == serverPatchData.Hash)
            {
                Debug.LogFormat("Newest Version:'{0}[{1}]'", channelData.Build, channelData.Version);
                if (clientData.IsObsolete)
                {
                    clientData.IsObsolete = false;
                    clientData.SaveToPersistent();
                }
                patchReport.Result = PatchResult.Success;
                return patchReport;
            }

            patchReport.PatchBundles = new List<BundleData>();
            foreach (var serverBundle in serverPatchData.Bundles)
            {
                BundleData packageBundle = packagePatchData?.Bundles?.Find((bundle) => bundle.Name == serverBundle.Name);

                if (packageBundle?.Hash == serverBundle.Hash)
                    continue;

                BundleData persistentBundle = persistentPatchData?.Bundles?.Find((bundle) => bundle.Name == serverBundle.Name);
                if (persistentBundle?.Hash != serverBundle.Hash)
                {
                    patchReport.PatchBundles.Add(serverBundle);
                    patchReport.TotalSize += serverBundle.Size;
                }
                persistentPatchData?.Bundles?.Remove(persistentBundle);
            }

            patchReport.DeleteBundles = new List<BundleData>();
            if (persistentPatchData != null)
            {
                foreach (var persistentBundle in persistentPatchData.Bundles)
                {
                    patchReport.DeleteBundles.Add(persistentBundle);
                }
            }

            patchReport.ServerPatchData = serverPatchData;
            patchReport.ClientData = clientData;
            patchReport.ChannelData = channelData;
            patchReport.Result = PatchResult.Collected;
            return patchReport;
        }

        public static async Task Patch(PatchReport patchReport)
        {
            if (IsPatched)
                return;

            if (patchReport == null || patchReport.Result != PatchResult.Collected)
                return;

            patchReport.Result = PatchResult.Patching;

            // download bundle
            if (patchReport.TotalSize > 0)
            {
                patchReport.PatchedSize = 0;
                foreach (var bundle in patchReport.PatchBundles)
                {
                    bool success = await DownloadBundle(patchReport, bundle);
                    if (!success)
                    {
                        patchReport.Result = PatchResult.Failure_BundleDownloadFailure;
                        return;
                    }
                }
            }

            // delete bundle
            foreach (var bundle in patchReport.DeleteBundles)
            {
                DeleteBundle(patchReport.ClientData, bundle.Name);
            }

            patchReport.ClientData.IsObsolete = false;
            patchReport.ClientData.SaveToPersistent();
            patchReport.ServerPatchData.SaveToPersistent();
            patchReport.Result = PatchResult.Success;
            Debug.Log($"Patch Completed. Current Version:'{patchReport.ChannelData.Build}[{patchReport.ChannelData.Version}]'");
        }

        private static async Task<bool> DownloadBundle(PatchReport patchReport, BundleData bundleData)
        {
            string url = $"{patchReport.ChannelData.URL}/{patchReport.ChannelData.Build}/{bundleData.Version}/{bundleData.Name}";
            Debug.Log($"Downloading Bundle:'{url}'");
            UnityWebRequest request = await FinalPatchUtility.WebRequestGet(url);
            if (request == null)
                return false;

            //Save
            string path = FinalPatchUtility.GetPersistentBundlePath(bundleData.Name);
            string saveDirectory = Path.GetDirectoryName(path);
            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
            }
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                fs.Write(request.downloadHandler.data, 0, request.downloadHandler.data.Length);
            }
            patchReport.PatchedSize += bundleData.Size;
            patchReport.ClientData.UpdateBundle(bundleData.Name);
            return true;
        }

        private static void DeleteBundle(ClientData clientData, string name)
        {
            Debug.Log($"Deleting Bundle:'{name}'");
            string path = $"{Application.persistentDataPath}/{FinalPatchConst.ASSET_BUNDLE_SUBDIRECTORY_NAME}/{name}";
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            clientData.DeleteBundle(name);
        }

        public class WaitForPatchFinish : CustomYieldInstruction
        {
            public override bool keepWaiting => !IsPatched;
        }
    }
}