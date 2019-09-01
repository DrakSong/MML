using System;
using UnityEditor;
using UnityEngine;

namespace BBGo.FinalPatch
{
    public class AdvancedTabView : ITabView
    {
        private FinalPatchManagerWindow m_managerWindow;
        private GUILayoutOption[] m_contentOptions;
        private GUIContent m_contentBrowse;
        private GUIContent m_contentReveal;
        private GUIContent m_contentAssetPath;
        private GUIContent m_contentExtractDuplicated;
        private GUIContent m_contentBundleOffset;
        private GUIContent m_contentBuildCallback;
        private GUIContent m_contentPathConverter;
        private GUIContent m_contentAutoPatch;
        private GUIContent m_contentAutoRelease;
        private GUIContent m_contentApplyEditorMode;

        public AdvancedTabView(FinalPatchManagerWindow managerWindow)
        {
            m_managerWindow = managerWindow;
            m_contentOptions = new GUILayoutOption[]
            {
                GUILayout.Width(25),
                GUILayout.Height(25),
            };
            m_contentBrowse = new GUIContent(Resources.Load<Texture2D>("browse"));
            m_contentReveal = new GUIContent(Resources.Load<Texture2D>("reveal"));
            m_contentAssetPath = new GUIContent();
            m_contentExtractDuplicated = new GUIContent();
            m_contentBundleOffset = new GUIContent();
            m_contentBuildCallback = new GUIContent();
            m_contentPathConverter = new GUIContent();
            m_contentAutoPatch = new GUIContent();
            m_contentAutoRelease = new GUIContent();
            m_contentApplyEditorMode = new GUIContent();
            Localization.OnLanguageChanged += OnLocalizationChanged;
            OnLocalizationChanged();
        }

        public void OnDispose()
        {
            Localization.OnLanguageChanged -= OnLocalizationChanged;
        }

        private void OnLocalizationChanged()
        {
            m_contentBrowse.tooltip = Localization.GetString("advanced_tab_browse_tip");
            m_contentReveal.tooltip = Localization.GetString("advanced_tab_reveal_tip");

            m_contentAssetPath.text = Localization.GetString("advanced_tab_asset_path_label");
            m_contentAssetPath.tooltip = Localization.GetString("advanced_tab_asset_path_tip");
            m_contentExtractDuplicated.text = Localization.GetString("advanced_tab_extract_duplicated_dependency_label");
            m_contentExtractDuplicated.tooltip = Localization.GetString("advanced_tab_extract_duplicated_dependency_tip");
            m_contentBundleOffset.text = Localization.GetString("advanced_tab_bundle_offset_label");
            m_contentBundleOffset.tooltip = Localization.GetString("advanced_tab_bundle_offset_tip");
            m_contentBuildCallback.text = Localization.GetString("advanced_tab_build_callback_label");
            m_contentBuildCallback.tooltip = Localization.GetString("advanced_tab_build_callback_tip");
            m_contentPathConverter.text = Localization.GetString("advanced_tab_path_converter_label");
            m_contentPathConverter.tooltip = Localization.GetString("advanced_tab_path_converter_tip");
            m_contentAutoPatch.text = Localization.GetString("advanced_tab_auto_patch_label");
            m_contentAutoPatch.tooltip = Localization.GetString("advanced_tab_auto_patch_tip");
            m_contentAutoRelease.text = Localization.GetString("advanced_tab_auto_release_label");
            m_contentAutoRelease.tooltip = Localization.GetString("advanced_tab_auto_release_tip");
            m_contentApplyEditorMode.text = Localization.GetString("advanced_tab_apply_editor_mode_label");
            m_contentApplyEditorMode.tooltip = Localization.GetString("advanced_tab_apply_editor_mode_tip");
        }

        public void OnGUI()
        {
            LocalizationPopup();
            EditorGUI.BeginChangeCheck();
            {
                //build settings
                EditorGUILayout.LabelField(Localization.GetString("advanced_tab_build_label"));
                EditorGUI.indentLevel++;
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField(m_contentAssetPath, new GUIContent(m_managerWindow.FinalPatchEditorData.assetPath));
                        if (GUILayout.Button(m_contentBrowse, BBGoGUI.styleCompactButton, m_contentOptions))
                        {
                            string path = EditorUtility.OpenFolderPanel("", "", "Assets");
                            if (!string.IsNullOrEmpty(path))
                            {
                                Uri pathUri = new Uri(path);
                                Uri rootUri = new Uri(Application.dataPath.Remove(Application.dataPath.LastIndexOf("Assets")));
                                path = rootUri.MakeRelativeUri(pathUri).ToString();
                                if (path.StartsWith("../"))
                                {
                                    Debug.LogErrorFormat(Localization.GetString("msg_select_asset_path_failure"), path);
                                }
                                else
                                {
                                    m_managerWindow.FinalPatchEditorData.assetPath = path;
                                }
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    m_managerWindow.FinalPatchEditorData.extractDuplicatedDependency = EditorGUILayout.Toggle(m_contentExtractDuplicated, m_managerWindow.FinalPatchEditorData.extractDuplicatedDependency);
                    m_managerWindow.FinalPatchData.bundleBytesOffset = (ulong)EditorGUILayout.IntSlider(m_contentBundleOffset, (int)m_managerWindow.FinalPatchData.bundleBytesOffset, 0, 255);
                    m_managerWindow.FinalPatchEditorData.buildCallbackType = BBGoGUI.SubTypeField(m_contentBuildCallback, m_managerWindow.FinalPatchEditorData.buildCallbackType, typeof(IBuildEditorCallback), true);
                    m_managerWindow.FinalPatchData.pathConverterType = BBGoGUI.SubTypeField(m_contentPathConverter, m_managerWindow.FinalPatchData.pathConverterType, typeof(IPathConverter), false);
                }
                EditorGUI.indentLevel--;

                //patch settings
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(Localization.GetString("advanced_tab_patch_label"));
                EditorGUI.indentLevel++;
                {
                    m_managerWindow.FinalPatchData.patchAutoPatch = EditorGUILayout.BeginToggleGroup(m_contentAutoPatch, m_managerWindow.FinalPatchData.patchAutoPatch);
                    {
                        EditorGUI.indentLevel++;
                        {
                            m_managerWindow.FinalPatchData.patchRetryCount = EditorGUILayout.IntSlider(Localization.GetString("advanced_tab_patch_retry_count_label"), m_managerWindow.FinalPatchData.patchRetryCount, 0, 5);
                            m_managerWindow.FinalPatchData.patchRetryInterval = EditorGUILayout.IntSlider(Localization.GetString("advanced_tab_patch_retry_interval_label"), m_managerWindow.FinalPatchData.patchRetryInterval, 0, 10);
                        }
                        EditorGUI.indentLevel--;
                    }
                    EditorGUILayout.EndToggleGroup();
                }
                EditorGUI.indentLevel--;

                //pool settings
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(Localization.GetString("advanced_tab_pool_label"));
                EditorGUI.indentLevel++;
                {
                    m_managerWindow.FinalPatchData.poolMaxCache = EditorGUILayout.IntField(Localization.GetString("advanced_tab_max_cache_label"), m_managerWindow.FinalPatchData.poolMaxCache);
                    m_managerWindow.FinalPatchData.poolEnableAutoRelease = EditorGUILayout.BeginToggleGroup(m_contentAutoRelease, m_managerWindow.FinalPatchData.poolEnableAutoRelease);
                    {
                        EditorGUI.indentLevel++;
                        {
                            m_managerWindow.FinalPatchData.poolAutoReleaseDuration = EditorGUILayout.FloatField(Localization.GetString("advanced_tab_auto_release_delay_label"), m_managerWindow.FinalPatchData.poolAutoReleaseDuration);
                        }
                        EditorGUI.indentLevel--;
                    }
                    EditorGUILayout.EndToggleGroup();
                }
                EditorGUI.indentLevel--;

                //other settings
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(Localization.GetString("advanced_tab_other_label"));
                EditorGUI.indentLevel++;
                {
                    m_managerWindow.FinalPatchData.applyEditorMode = EditorGUILayout.Toggle(m_contentApplyEditorMode, m_managerWindow.FinalPatchData.applyEditorMode);
                    EditorGUILayout.BeginHorizontal();
                    {
                        string persistentBundlePath = $"{Application.persistentDataPath}/{FinalPatchConst.ASSET_BUNDLE_SUBDIRECTORY_NAME}";
                        EditorGUILayout.LabelField(Localization.GetString("advanced_tab_persistent_path_label"), persistentBundlePath);
                        if (GUILayout.Button(m_contentReveal, BBGoGUI.styleCompactButton, m_contentOptions))
                        {
                            EditorUtility.RevealInFinder(persistentBundlePath);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
            }
            if (EditorGUI.EndChangeCheck())
            {
                m_managerWindow.FinalPatchData.Save();
                m_managerWindow.FinalPatchEditorData.Save();
            }
        }

        private void LocalizationPopup()
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.Space();
                EditorGUI.BeginChangeCheck();
                LocalizationType selectType = (LocalizationType)EditorGUILayout.Popup((int)Localization.CurrentType, Localization.LOCALIZATION_LABEL, GUILayout.Width(150));
                if (EditorGUI.EndChangeCheck())
                {
                    Localization.SetLocalizationType(selectType);
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }
    }
}