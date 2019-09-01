using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

namespace BBGo.FinalPatch
{
    public class DeployTabView : ITabView
    {
        private string m_deployDataPath = null;
        private DeployData m_deployData = null;
        private FinalPatchManagerWindow m_managerWindow;
        private bool m_hasEdit;
        private string m_searchText;
        private GUILayoutOption[] m_contentOptions;
        private GUIContent m_contentEdit;
        private GUIContent m_contentDelete;
        private GUIContent m_contentSave;
        private GUIContent m_contentDiscard;

        public DeployTabView(FinalPatchManagerWindow managerWindow)
        {
            m_managerWindow = managerWindow;
            LoadDeployData(PlayerPrefs.GetString(FinalPatchConst.KEY_DEPLOY_DATA));

            m_contentOptions = new GUILayoutOption[]
            {
                GUILayout.Width(25),
                GUILayout.Height(25),
            };
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
            m_contentEdit.tooltip = Localization.GetString("deploy_tab_edit_tip");
            m_contentDelete.tooltip = Localization.GetString("deploy_tab_delete_tip");
            m_contentSave.tooltip = Localization.GetString("deploy_tab_save_tip");
            m_contentDiscard.tooltip = Localization.GetString("deploy_tab_discard_tip");
        }

        private void CalcHasEditChannel()
        {
            m_hasEdit = false;
            for (int i = 0; m_deployData != null && m_deployData.Channels != null && i < m_deployData.Channels.Count; i++)
            {
                if (m_deployData.Channels[i].IsEdit)
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

            if (m_deployData != null)
            {
                m_searchText = BBGoGUI.SearchField(m_searchText);
                EditorGUILayout.Space();

                for (int i = 0; m_deployData.Channels != null && i < m_deployData.Channels.Count; i++)
                {
                    DrawChannel(m_deployData.Channels[i]);
                }
            }
        }

        private void DrawToolbar()
        {
            bool newDeployData = false;
            bool selectDeployData = false;
            bool newChannel = false;
            EditorGUILayout.BeginHorizontal();
            if (m_deployData == null)
            {
                newDeployData = GUILayout.Button(Localization.GetString("deploy_tab_new_data"), BBGoGUI.styleBreadcrumbLeft);
                selectDeployData = GUILayout.Button(Localization.GetString("deploy_tab_select_data"), BBGoGUI.styleBreadcrumbMid);
            }
            else
            {
                BBGoGUI.BeginEnable(!m_hasEdit);
                newChannel = GUILayout.Button(Localization.GetString("deploy_tab_new_channel"), BBGoGUI.styleBreadcrumbLeft);
                BBGoGUI.EndEnable();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (newDeployData)
            {
                CreateDeployData(EditorUtility.SaveFolderPanel(Localization.GetString("deploy_tab_new_data"), string.Empty, string.Empty));
            }
            if (selectDeployData)
            {
                LoadDeployData(EditorUtility.OpenFilePanel(Localization.GetString("deploy_tab_select_data"), "Assets", string.Empty));
            }
            if (newChannel)
            {
                if (m_deployData.Channels == null)
                {
                    m_deployData.Channels = new List<ChannelData>();
                }
                m_deployData.Channels.Add(new ChannelData() { IsEdit = true });
            }
        }

        private void CreateDeployData(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            string dataPath = $"{path}/{FinalPatchConst.DEPLOY_DATA_FILE_NAME}";
            if (File.Exists(dataPath))
            {
                bool overwrite = EditorUtility.DisplayDialog(Localization.GetString("deploy_tab_overwrite_data_title"),
                                                             Localization.GetString("deploy_tab_overwrite_data_message"),
                                                             Localization.GetString("deploy_tab_overwrite_data_yes"),
                                                             Localization.GetString("deploy_tab_overwrite_data_no"));
                if (!overwrite)
                    return;
            }

            m_deployDataPath = dataPath;
            PlayerPrefs.SetString(FinalPatchConst.KEY_DEPLOY_DATA, m_deployDataPath);

            m_deployData = new DeployData();
            SaveData();
        }

        private void LoadDeployData(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(DeployData));
                    m_deployData = serializer.Deserialize(fs) as DeployData;
                    m_deployDataPath = path;
                    PlayerPrefs.SetString(FinalPatchConst.KEY_DEPLOY_DATA, m_deployDataPath);
                }
            }
            catch
            {
                PlayerPrefs.DeleteKey(FinalPatchConst.KEY_DEPLOY_DATA);
            }
        }

        private void SaveData()
        {
            if (m_deployData == null)
                return;

            if (string.IsNullOrEmpty(m_deployDataPath))
                return;

            using (FileStream fs = new FileStream(m_deployDataPath, FileMode.Create, FileAccess.Write))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(DeployData));
                serializer.Serialize(fs, m_deployData);
            }
        }

        private void DrawChannel(ChannelData channelData)
        {
            if (channelData == null)
                return;

            if (!string.IsNullOrEmpty(m_searchText) &&
                channelData.Name?.Contains(m_searchText) == false)
                return;

            bool delete = false;
            bool save = false;
            bool edit = false;
            bool discard = false;
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    channelData.Foldout = EditorGUILayout.Foldout(channelData.Foldout, $"{channelData.Name}: {channelData.Build}[{channelData.Version}]") || channelData.IsEdit;
                }
                EditorGUILayout.EndHorizontal();
                if (channelData.Foldout)
                {
                    if (channelData.IsEdit)
                    {
                        bool canSave = true;

                        if (string.IsNullOrEmpty(channelData.Name) || m_deployData.Channels.Find((c) => c != channelData && c.Name == channelData.Name) != null)
                        {
                            canSave = false;
                            channelData.Name = BBGoGUI.ErrorTextField(Localization.GetString("deploy_tab_channel_name"), channelData.Name)?.Trim();
                        }
                        else
                        {
                            channelData.Name = EditorGUILayout.TextField(Localization.GetString("deploy_tab_channel_name"), channelData.Name)?.Trim();
                        }

                        if (string.IsNullOrEmpty(channelData.URL))
                        {
                            canSave = false;
                            channelData.URL = BBGoGUI.ErrorTextField(Localization.GetString("deploy_tab_channel_url"), channelData.URL)?.Trim();
                        }
                        else
                        {
                            channelData.URL = EditorGUILayout.TextField(Localization.GetString("deploy_tab_channel_url"), channelData.URL)?.Trim();
                        }

                        int buildIndex = m_managerWindow.FinalPatchEditorData.builds.FindIndex((buildData) => { return buildData.name == channelData.Build; });
                        if (buildIndex < 0)
                        {
                            canSave = false;
                            buildIndex = BBGoGUI.ErrorPopup(Localization.GetString("deploy_tab_channel_build"), buildIndex, m_managerWindow.FinalPatchEditorData.builds.ConvertAll((patchVersion) => { return patchVersion.name; }).ToArray());
                        }
                        else
                        {
                            buildIndex = EditorGUILayout.Popup(Localization.GetString("deploy_tab_channel_build"), buildIndex, m_managerWindow.FinalPatchEditorData.builds.ConvertAll((patchVersion) => { return patchVersion.name; }).ToArray());
                        }
                        BuildVersion buildVersion = null;
                        if (buildIndex >= 0)
                        {
                            buildVersion = m_managerWindow.FinalPatchEditorData.builds[buildIndex];
                            channelData.Build = buildVersion.name;
                        }
                        else
                        {
                            channelData.Build = null;
                        }

                        BBGoGUI.BeginEnable(!string.IsNullOrEmpty(channelData.Build));
                        {
                            if (buildVersion != null)
                            {
                                channelData.Version = EditorGUILayout.IntSlider(Localization.GetString("deploy_tab_channel_version"), channelData.Version, 0, buildVersion.version);
                            }
                            else
                            {
                                channelData.Version = EditorGUILayout.IntSlider(Localization.GetString("deploy_tab_channel_version"), 0, 0, 0);
                            }
                        }
                        BBGoGUI.EndEnable();
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
                        EditorGUILayout.LabelField(Localization.GetString("deploy_tab_channel_name"), channelData.Name);
                        EditorGUILayout.LabelField(Localization.GetString("deploy_tab_channel_url"), channelData.URL);
                        EditorGUILayout.LabelField(Localization.GetString("deploy_tab_channel_build"), channelData.Build);
                        EditorGUILayout.LabelField(Localization.GetString("deploy_tab_channel_version"), channelData.Version.ToString());

                        EditorGUILayout.Space();
                        BBGoGUI.BeginEnable(!m_hasEdit);
                        {
                            EditorGUILayout.BeginHorizontal();
                            {
                                EditorGUILayout.Space();
                                edit |= GUILayout.Button(m_contentEdit, BBGoGUI.styleCompactButton, m_contentOptions);
                                delete = GUILayout.Button(m_contentDelete, BBGoGUI.styleCompactButton, m_contentOptions);
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        BBGoGUI.EndEnable();
                    }
                }
            }
            EditorGUILayout.EndVertical();

            if (edit)
            {
                channelData.IsEdit = true;
            }

            if (discard)
            {
                LoadDeployData(PlayerPrefs.GetString(FinalPatchConst.KEY_DEPLOY_DATA));
                GUI.FocusControl(null);
            }

            if (save)
            {
                channelData.IsEdit = false;
                SaveData();
                GUI.FocusControl(null);
            }

            if (delete)
            {
                bool comfirmDelete = EditorUtility.DisplayDialog(Localization.GetString("deploy_tab_delete_channel_comfirm_title"),
                                                                 Localization.GetString("deploy_tab_delete_channel_comfirm_message"),
                                                                 Localization.GetString("deploy_tab_delete_channel_comfirm_yes"),
                                                                 Localization.GetString("deploy_tab_delete_channel_comfirm_no"));
                if (comfirmDelete)
                {
                    m_deployData.Channels.Remove(channelData);
                    SaveData();
                }
            }
        }
    }
}