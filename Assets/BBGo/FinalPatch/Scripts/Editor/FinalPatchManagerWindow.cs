using UnityEditor;
using UnityEngine;

namespace BBGo.FinalPatch
{
    public class FinalPatchManagerWindow : EditorWindow
    {
        [MenuItem("Tools/BBGo/Add Final Patch Client", false, 2)]
        private static void AddFinalPatchClient()
        {
            Object finalPatchObj = Object.FindObjectOfType(typeof(FinalPatchClient));
            if (finalPatchObj == null)
            {
                finalPatchObj = new GameObject("FinalPatchClient", new System.Type[] { typeof(FinalPatchClient) });
            }
            Selection.activeObject = finalPatchObj;
        }

        [MenuItem("Tools/BBGo/Final Patch Manager", false, 1)]
        private static void Open()
        {
            GetWindow<FinalPatchManagerWindow>();
        }

        public FinalPatchData FinalPatchData { get; set; }
        public FinalPatchEditorData FinalPatchEditorData { get; set; }

        private string[] m_tabLabels;
        private TabSet m_mainTabSet;

        private void OnEnable()
        {
            FinalPatchData = FinalPatchData.Load();
            FinalPatchEditorData = FinalPatchEditorData.Load();
            Localization.OnLanguageChanged += OnLanguageChanged;

            m_mainTabSet = new TabSet(new Tab[]
            {
                new Tab(new BuildTabView(this)),
                new Tab(new DeployTabView(this)),
                new Tab(new AdvancedTabView(this)),
            }, FinalPatchEditorData.activeTab);
            m_mainTabSet.OnActiveIndexChanged += OnActiveTabChanged;

            m_tabLabels = new string[]
            {
                "tab_label_build",
                "tab_label_deploy",
                "tab_label_advanced",
            };

            OnLanguageChanged();
        }

        private void OnDisable()
        {
            m_mainTabSet.OnActiveIndexChanged -= OnActiveTabChanged;
            Localization.OnLanguageChanged -= OnLanguageChanged;
            for (int i = 0; i < m_mainTabSet.Tabs.Length; i++)
            {
                m_mainTabSet.Tabs[i].Dispose();
            }
        }

        private void OnActiveTabChanged(int activeTab)
        {
            FinalPatchEditorData.activeTab = activeTab;
            FinalPatchEditorData.Save();
        }

        private void OnLanguageChanged()
        {
            titleContent.text = Localization.GetString("manager_window_title");
            for (int i = 0; i < m_mainTabSet.Tabs.Length; i++)
            {
                m_mainTabSet.Tabs[i].Title.text = Localization.GetString(m_tabLabels[i]);
            }
            Repaint();
        }

        private void OnGUI()
        {
            BBGoGUI.Tab(m_mainTabSet);
        }
    }
}