using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

namespace BBGo.FinalPatch
{
    public class BuildTabView : ITabView
    {
        private string m_outputPlatformPath;
        private string m_outputFullPath;
        private string m_outputPackagePath;
        private FinalPatchManagerWindow m_managerWindow;
        private string m_searchText;
        private bool m_hasEdit;
        private GUILayoutOption[] m_contentOptions;
        private GUIContent m_contentBuild;
        private GUIContent m_contentCopy;
        private GUIContent m_contentReveal;
        private GUIContent m_contentEdit;
        private GUIContent m_contentDelete;
        private GUIContent m_contentSave;
        private GUIContent m_contentDiscard;

        public BuildTabView(FinalPatchManagerWindow managerWindow)
        {
            m_managerWindow = managerWindow;
            m_contentOptions = new GUILayoutOption[]
            {
                GUILayout.Width(25),
                GUILayout.Height(25),
            };
            m_contentBuild = new GUIContent(Resources.Load<Texture2D>("make"));
            m_contentCopy = new GUIContent(Resources.Load<Texture2D>("copy"));
            m_contentReveal = new GUIContent(Resources.Load<Texture2D>("reveal"));
            m_contentEdit = new GUIContent(Resources.Load<Texture2D>("edit"));
            m_contentDelete = new GUIContent(Resources.Load<Texture2D>("delete"));
            m_contentSave = new GUIContent(Resources.Load<Texture2D>("save"));
            m_contentDiscard = new GUIContent(Resources.Load<Texture2D>("discard"));
            Localization.OnLanguageChanged += OnLocalizationChanged;
            OnLocalizationChanged();
        }

        public void OnDispose()
        {
            Localization.OnLanguageChanged -= OnLocalizationChanged;
        }

        private void OnLocalizationChanged()
        {
            m_contentBuild.tooltip = Localization.GetString("build_tab_build_tip");
            m_contentCopy.tooltip = Localization.GetString("build_tab_copy_tip");
            m_contentReveal.tooltip = Localization.GetString("build_tab_reveal_tip");
            m_contentEdit.tooltip = Localization.GetString("build_tab_edit_tip");
            m_contentDelete.tooltip = Localization.GetString("build_tab_delete_tip");
            m_contentSave.tooltip = Localization.GetString("build_tab_save_tip");
            m_contentDiscard.tooltip = Localization.GetString("build_tab_discard_tip");
        }

        private void CalcHasEditChannel()
        {
            m_hasEdit = false;
            for (int i = 0; m_managerWindow.FinalPatchEditorData.builds != null && i < m_managerWindow.FinalPatchEditorData.builds.Count; i++)
            {
                if (m_managerWindow.FinalPatchEditorData.builds[i].isEdit)
                {
                    m_hasEdit = true;
                    break;
                }
            }
        }

        public void OnGUI()
        {
            CalcHasEditChannel();
            DrawToolbar();
            m_searchText = BBGoGUI.SearchField(m_searchText);
            EditorGUILayout.Space();

            for (int i = 0; m_managerWindow.FinalPatchEditorData.builds != null && i < m_managerWindow.FinalPatchEditorData.builds.Count; i++)
            {
                DrawBuild(m_managerWindow.FinalPatchEditorData.builds[i]);
            }
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal();
            BBGoGUI.BeginEnable(!m_hasEdit);
            if (GUILayout.Button(Localization.GetString("build_tab_new"), BBGoGUI.styleBreadcrumbLeft))
            {
                m_managerWindow.FinalPatchEditorData.builds.Add(new BuildVersion() { isEdit = true });
            }
            BBGoGUI.EndEnable();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        private void DrawBuild(BuildVersion buildVersion)
        {
            if (!string.IsNullOrEmpty(m_searchText) &&
                (!buildVersion.name.Contains(m_searchText) && !buildVersion.description.Contains(m_searchText)))
                return;

            bool browse = false;
            bool copy = false;
            bool build = false;
            bool edit = false;
            bool delete = false;
            bool save = false;
            bool discard = false;
            string foldoutName = $"{buildVersion.name}[{buildVersion.version}]";
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    buildVersion.foldout = EditorGUILayout.Foldout(buildVersion.foldout, foldoutName) || buildVersion.isEdit;
                    BBGoGUI.BeginEnable(!m_hasEdit);
                    {
                        browse = GUILayout.Button(m_contentReveal, BBGoGUI.styleCompactButton, m_contentOptions);
                        copy = GUILayout.Button(m_contentCopy, BBGoGUI.styleCompactButton, m_contentOptions);
                        build = GUILayout.Button(m_contentBuild, BBGoGUI.styleCompactButton, m_contentOptions);
                    }
                    BBGoGUI.EndEnable();
                }
                EditorGUILayout.EndHorizontal();
                if (buildVersion.foldout)
                {
                    if (buildVersion.isEdit)
                    {
                        bool canSave = true;

                        if (string.IsNullOrEmpty(buildVersion.name) || m_managerWindow.FinalPatchEditorData.builds.Find((b) => b != buildVersion && b.name == buildVersion.name) != null)
                        {
                            canSave = false;
                            buildVersion.name = BBGoGUI.ErrorTextField(Localization.GetString("build_tab_build_name"), buildVersion.name)?.Trim();
                        }
                        else
                        {
                            buildVersion.name = EditorGUILayout.TextField(Localization.GetString("build_tab_build_name"), buildVersion.name)?.Trim();
                        }

                        buildVersion.description = EditorGUILayout.TextField(Localization.GetString("build_tab_build_desc"), buildVersion.description);
                        buildVersion.buildTarget = (BuildTarget)EditorGUILayout.EnumPopup(Localization.GetString("build_tab_build_target"), buildVersion.buildTarget);
                        EditorGUILayout.LabelField(Localization.GetString("build_tab_build_version"), buildVersion.version.ToString());
                        EditorGUILayout.Space();

                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.Space();
                            BBGoGUI.BeginEnable(canSave);
                            {
                                save = GUILayout.Button(m_contentSave, BBGoGUI.styleCompactButton, m_contentOptions);
                            }
                            BBGoGUI.EndEnable();
                            discard = GUILayout.Button(m_contentDiscard, BBGoGUI.styleCompactButton, m_contentOptions);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        EditorGUILayout.LabelField(Localization.GetString("build_tab_build_name"), buildVersion.name);
                        EditorGUILayout.LabelField(Localization.GetString("build_tab_build_desc"), buildVersion.description);
                        EditorGUILayout.LabelField(Localization.GetString("build_tab_build_target"), buildVersion.buildTarget.ToString());
                        EditorGUILayout.LabelField(Localization.GetString("build_tab_build_version"), buildVersion.version.ToString());
                        EditorGUILayout.Space();

                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.Space();
                            BBGoGUI.BeginEnable(!m_hasEdit);
                            {
                                edit = GUILayout.Button(m_contentEdit, BBGoGUI.styleCompactButton, m_contentOptions);
                                delete = GUILayout.Button(m_contentDelete, BBGoGUI.styleCompactButton, m_contentOptions);
                            }
                            BBGoGUI.EndEnable();
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
            EditorGUILayout.EndVertical();

            if (browse)
            {
                EditorUtility.RevealInFinder(string.Format($"{FinalPatchConst.ROOT_PATH}/{buildVersion.name}"));
            }
            if (copy)
            {
                CopyToStreamingAssets(buildVersion);
            }
            if (build)
            {
                Build(buildVersion, buildVersion.version + 1);
            }
            if (save)
            {
                buildVersion.isEdit = false;
                m_managerWindow.FinalPatchEditorData.Save();
                GUI.FocusControl(null);
            }
            if (discard)
            {
                Resources.UnloadAsset(m_managerWindow.FinalPatchEditorData);
                m_managerWindow.FinalPatchEditorData = FinalPatchEditorData.Load();
                GUI.FocusControl(null);
            }
            if (edit)
            {
                buildVersion.isEdit = true;
            }
            if (delete)
            {
                bool comfirmDelete = EditorUtility.DisplayDialog(Localization.GetString("build_tab_delete_comfirm_title"),
                                                                 Localization.GetString("build_tab_delete_comfirm_message"),
                                                                 Localization.GetString("build_tab_delete_comfirm_yes"),
                                                                 Localization.GetString("build_tab_delete_comfirm_no"));
                if (comfirmDelete)
                {
                    string dir = string.Format($"{FinalPatchConst.ROOT_PATH}/{buildVersion.name}");
                    if (Directory.Exists(dir))
                    {
                        Directory.Delete(dir, true);
                    }
                    m_managerWindow.FinalPatchEditorData.builds.Remove(buildVersion);
                    m_managerWindow.FinalPatchEditorData.Save();
                }
            }
        }

        private void Build(BuildVersion buildVersion, int newVersion)
        {
            m_outputPlatformPath = string.Format($"{FinalPatchConst.ROOT_PATH}/{buildVersion.name}");
            m_outputFullPath = FinalPatchUtility.GetOutputFullPath(m_outputPlatformPath, newVersion);
            m_outputPackagePath = FinalPatchUtility.GetOutputPackagePath(m_outputPlatformPath, newVersion);
            if (Validate(newVersion))
            {
                AssetDatabase.SaveAssets();
                IBuildEditorCallback callback = CreateBuildCallback();
                BuildBundles(buildVersion, newVersion);
                callback?.OnBuildFullFinished(buildVersion);

                if (newVersion == 1)
                {
                    BuildFullPackage(newVersion);
                }
                else
                {
                    BuildIncrementalPackage(newVersion);
                }
                callback?.OnBuildPackageFinished(buildVersion);

                Debug.Log(Localization.GetString("msg_build_success"));
            }
        }

        private IPathConverter GetPathConverter()
        {
            if (string.IsNullOrEmpty(m_managerWindow.FinalPatchData.pathConverterType) ||
                m_managerWindow.FinalPatchData.pathConverterType == "None")
            {
                Debug.LogErrorFormat("Please set Path Converter in Final Patch Manager Editor");
                return null;
            }

            try
            {
                Type type = typeof(IPathConverter).Assembly.GetType(m_managerWindow.FinalPatchData.pathConverterType);
                if (type == null)
                {

                    Debug.LogErrorFormat("Not found Path Converter Type:'{0}'", m_managerWindow.FinalPatchData.pathConverterType);
                    return null;
                }

                return Activator.CreateInstance(type) as IPathConverter;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }

        private AssetBundleBuild[] CollectAssetBundleBuilds()
        {
            IPathConverter pathConverter = GetPathConverter();
            string[] guids = AssetDatabase.FindAssets("", new string[] { m_managerWindow.FinalPatchEditorData.assetPath });
            List<AssetBundleBuild> builds = new List<AssetBundleBuild>();
            Dictionary<string, int> refMap = new Dictionary<string, int>();
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (builds.FindIndex((b) => ArrayUtility.Contains(b.assetNames, assetPath)) >= 0)
                    continue;

                builds.Add(CreateAssetBundleBuild(pathConverter, assetPath));

                if (m_managerWindow.FinalPatchEditorData.extractDuplicatedDependency)
                {
                    foreach (var dep in AssetDatabase.GetDependencies(assetPath, true))
                    {
                        if (dep == assetPath)
                            continue;

                        int count;
                        refMap.TryGetValue(dep, out count);
                        refMap[dep] = count + 1;
                    }
                }
            }

            if (m_managerWindow.FinalPatchEditorData.extractDuplicatedDependency)
            {
                foreach (var item in refMap)
                {
                    if (item.Value <= 1)
                        continue;

                    if (builds.FindIndex((b) => ArrayUtility.Contains(b.assetNames, item.Key)) >= 0)
                        continue;
                    builds.Add(CreateAssetBundleBuild(pathConverter, item.Key));
                }
            }

            return builds.ToArray();
        }

        private AssetBundleBuild CreateAssetBundleBuild(IPathConverter pathConverter, string assetPath)
        {
            AssetBundleBuild build = new AssetBundleBuild
            {
                assetBundleName = pathConverter.GetAssetBundleName(assetPath),
                assetNames = new string[] { assetPath }
            };
            return build;
        }

        private IBuildEditorCallback CreateBuildCallback()
        {
            if (string.IsNullOrEmpty(m_managerWindow.FinalPatchEditorData.buildCallbackType) ||
                m_managerWindow.FinalPatchEditorData.buildCallbackType == "None")
                return null;

            Type callbackType = Assembly.GetCallingAssembly().GetType(m_managerWindow.FinalPatchEditorData.buildCallbackType);
            if (callbackType == null)
            {
                Debug.LogErrorFormat("Not found build callback type:'{0}'", m_managerWindow.FinalPatchEditorData.buildCallbackType);
                return null;
            }

            try
            {
                return Activator.CreateInstance(callbackType) as IBuildEditorCallback;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }

        private bool Validate(int version)
        {
            bool validation = true;
            if (version > 1)
            {
                // check previous version file
                string previousVersionPath = $"{FinalPatchUtility.GetFullPatchDataFilePath(m_outputPlatformPath, version - 1)}";
                if (!File.Exists(previousVersionPath))
                {
                    Debug.LogError(Localization.GetString("msg_build_failure_no_previous_patch_data"));
                    validation = false;
                }
            }

            if (!validation)
            {
                m_managerWindow.ShowNotification(new GUIContent(Localization.GetString("msg_build_failure_notification")));
            }
            return validation;
        }

        private PatchData BuildBundles(BuildVersion buildVersion, int version)
        {
            if (Directory.Exists(m_outputFullPath))
            {
                Directory.Delete(m_outputFullPath, true);
            }
            Directory.CreateDirectory(m_outputFullPath);

            BuildPipeline.BuildAssetBundles(m_outputFullPath, CollectAssetBundleBuilds(), BuildAssetBundleOptions.None, buildVersion.buildTarget);

            //change name of AssetBundleManifest
            File.Move($"{m_outputFullPath}/{version}", $"{m_outputFullPath}/{FinalPatchConst.ASSET_BUNDLE_MANIFEST_NAME}");
            File.Move($"{m_outputFullPath}/{version}.manifest", $"{m_outputFullPath}/{FinalPatchConst.ASSET_BUNDLE_MANIFEST_NAME}.manifest");

            PatchData patchData = SavePatchData(buildVersion.name, version);

            //save build data
            buildVersion.version = version;
            m_managerWindow.FinalPatchEditorData.Save();

            return patchData;
        }

        private PatchData SavePatchData(string build, int version)
        {
            var bundleFiles = Directory.GetFiles(m_outputFullPath, "*", SearchOption.AllDirectories);

            PatchData currentPatchData = new PatchData
            {
                Build = build,
                Bundles = new List<BundleData>()
            };

            SHA1 hashAlgorithm = SHA1.Create();
            foreach (var file in bundleFiles)
            {
                string generalFile = file.Replace('\\', '/');
                string bundleName = generalFile.Remove(0, m_outputFullPath.Length + 1);
                string hash = null;
                long size = 0;
                try
                {
                    using (FileStream fs = File.Open(generalFile, FileMode.Open, FileAccess.Read))
                    {
                        hash = BitConverter.ToString(hashAlgorithm.ComputeHash(fs)).Replace("-", string.Empty);
                        size = fs.Length;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    continue;
                }

                BundleData bundleData = new BundleData()
                {
                    Name = bundleName,
                    Hash = hash.ToString(),
                    Size = size,
                    Version = version,
                };
                currentPatchData.Bundles.Add(bundleData);
            }

            // calc bundle version
            PatchData prevPatchData = PatchData.LoadAtPath($"{FinalPatchUtility.GetFullPatchDataFilePath(m_outputPlatformPath, version - 1)}");
            if (prevPatchData != null)
            {
                foreach (var currentBundle in currentPatchData.Bundles)
                {
                    BundleData prevBundle = prevPatchData.Bundles.Find((bundle) => bundle.Name == currentBundle.Name);
                    if (prevBundle == null)
                        continue;

                    if (currentBundle.Hash == prevBundle.Hash)
                    {
                        currentBundle.Version = prevBundle.Version;
                    }
                }
            }

            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(ms, currentPatchData);
                ms.Seek(0, SeekOrigin.Begin);
                currentPatchData.Hash = BitConverter.ToString(hashAlgorithm.ComputeHash(ms)).Replace("-", string.Empty);
            }
            currentPatchData.Save($"{FinalPatchUtility.GetFullPatchDataFilePath(m_outputPlatformPath, version)}");
            return currentPatchData;
        }

        private void BuildFullPackage(int version)
        {
            if (Directory.Exists(m_outputPackagePath))
            {
                Directory.Delete(m_outputPackagePath, true);
            }
            Directory.CreateDirectory(m_outputPackagePath);

            //copy all files to package
            foreach (var file in Directory.GetFiles(m_outputFullPath, "*", SearchOption.AllDirectories))
            {
                string fromPath = file.Replace('\\', '/');
                string toPath = $"{m_outputPackagePath}/{fromPath.Remove(0, m_outputFullPath.Length + 1)}";
                CopyBundle(fromPath, toPath);
            }
        }

        private void BuildIncrementalPackage(int version)
        {
            if (Directory.Exists(m_outputPackagePath))
            {
                Directory.Delete(m_outputPackagePath, true);
            }
            Directory.CreateDirectory(m_outputPackagePath);

            // copy current version bundle files
            foreach (var bundle in PatchData.LoadAtPath($"{FinalPatchUtility.GetFullPatchDataFilePath(m_outputPlatformPath, version)}").Bundles)
            {
                if (bundle.Version != version)
                    continue;

                string fromPath = $"{m_outputFullPath}/{bundle.Name}";
                string toPath = $"{m_outputPackagePath}/{bundle.Name}";
                CopyBundle(fromPath, toPath);
            }

            // copy patch data
            File.Copy($"{m_outputFullPath}/{FinalPatchConst.PATCH_DATA_FILE_NAME}", $"{m_outputPackagePath}/{FinalPatchConst.PATCH_DATA_FILE_NAME}", true);
        }

        private void CopyBundle(string fromPath, string toPath)
        {
            string toDirectory = Path.GetDirectoryName(toPath);
            if (!Directory.Exists(toDirectory))
            {
                Directory.CreateDirectory(toDirectory);
            }
            byte[] head = new byte[m_managerWindow.FinalPatchData.bundleBytesOffset];
            byte[] content = File.ReadAllBytes(fromPath);
            using (FileStream fs = new FileStream(toPath, FileMode.Create, FileAccess.Write))
            {
                fs.Write(head, 0, head.Length);
                fs.Write(content, 0, content.Length);
            }
        }

        private void CopyToStreamingAssets(BuildVersion buildVersion)
        {
            m_outputPlatformPath = string.Format($"{FinalPatchConst.ROOT_PATH}/{buildVersion.name}");
            m_outputFullPath = FinalPatchUtility.GetOutputFullPath(m_outputPlatformPath, buildVersion.version);
            // copy asset bundles
            foreach (var file in Directory.GetFiles(m_outputFullPath, "*", SearchOption.AllDirectories))
            {
                string fileName = Path.GetFileName(file);
                if (fileName == FinalPatchConst.PATCH_DATA_FILE_NAME)
                    continue;

                string toPath = $"Assets/StreamingAssets/{FinalPatchConst.ASSET_BUNDLE_SUBDIRECTORY_NAME}{file.Remove(0, m_outputFullPath.Length)}";
                string toDir = Path.GetDirectoryName(toPath);
                if (!Directory.Exists(toDir))
                {
                    Directory.CreateDirectory(toDir);
                }

                File.Copy(file, toPath, true);
            }

            PatchData patchData = PatchData.LoadAtPath(FinalPatchUtility.GetFullPatchDataFilePath(m_outputPlatformPath, buildVersion.version));
            patchData.SaveToStreamingAssets();
            AssetDatabase.Refresh();
        }
    }
}