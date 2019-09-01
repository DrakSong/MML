using BBGo.FinalPatch;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BBGo
{
    public static class BBGoGUI
    {
        public static GUIStyle styleTab;
        public static GUIStyle styleTabSelected;
        public static GUIStyle styleSearchField;
        public static GUIStyle styleSearchCancelButton;
        public static GUIStyle styleSearchCancelButtonEmpty;
        public static GUIStyle styleCompactButton;
        public static GUIStyle styleBreadcrumbLeft;
        public static GUIStyle styleBreadcrumbMid;
        public static GUIStyle styleErrorLabel;
        public static GUIStyle styleErrorText;

        static BBGoGUI()
        {
            InitStyles();
        }

        private static void InitStyles()
        {
            styleErrorLabel = new GUIStyle(GUI.skin.label);
            styleErrorLabel.normal.textColor = Color.red;

            styleErrorText = new GUIStyle(GUI.skin.textField);
            styleErrorText.normal.textColor = Color.red;
            styleErrorText.focused.textColor = Color.red;

            styleBreadcrumbLeft = new GUIStyle("GUIEditor.BreadcrumbLeft");
            styleBreadcrumbMid = new GUIStyle("GUIEditor.BreadcrumbMid");
            styleCompactButton = new GUIStyle(GUI.skin.button)
            {
                margin = new RectOffset(),
                padding = new RectOffset(4, 4, 2, 2)
            };

            styleSearchField = "SearchTextField";
            styleSearchCancelButton = "SearchCancelButton";
            styleSearchCancelButtonEmpty = "SearchCancelButtonEmpty";

            styleTab = new GUIStyle(GUI.skin.box)
            {
                margin = new RectOffset(),
                stretchWidth = true,
                wordWrap = false,
            };
            if (EditorGUIUtility.isProSkin)
            {
                styleTab.normal.background = Resources.Load<Texture2D>("tabPro");
                styleTab.normal.textColor = new Color(180, 180, 180, 255) / 255f;

                styleTabSelected = new GUIStyle(styleTab);
                styleTabSelected.normal.background = Resources.Load<Texture2D>("tabSelectedPro");
                styleTabSelected.normal.textColor = new Color(180, 180, 180, 255) / 255f;
            }
            else
            {
                styleTab.normal.background = Resources.Load<Texture2D>("tab");

                styleTabSelected = new GUIStyle(styleTab);
                styleTabSelected.normal.background = Resources.Load<Texture2D>("tabSelected");
            }
        }

        public static string SearchField(string searchText, params GUILayoutOption[] options)
        {
            return SearchField(GUIContent.none, searchText, options);
        }

        public static string SearchField(GUIContent label, string searchText, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginHorizontal(options);
            searchText = EditorGUILayout.TextField(label, searchText, styleSearchField);
            if (GUILayout.Button(string.Empty, string.IsNullOrEmpty(searchText) ? styleSearchCancelButtonEmpty : styleSearchCancelButton))
            {
                searchText = string.Empty;
                GUI.FocusControl(null);
            }
            EditorGUILayout.EndHorizontal();
            return searchText;
        }

        public static void Tab(TabSet tabSet)
        {
            // draw tab bar
            EditorGUILayout.BeginHorizontal();
            {
                for (int i = 0; i < tabSet.Tabs.Length; i++)
                {
                    bool select = GUILayout.Button(tabSet.Tabs[i].Title, i == tabSet.ActiveIndex ? styleTabSelected : styleTab);
                    if (select)
                    {
                        tabSet.ActiveIndex = i;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            // draw tab content
            tabSet.ActiveTab.ScrollRect = EditorGUILayout.BeginScrollView(tabSet.ActiveTab.ScrollRect);
            {
                EditorGUILayout.Space();
                tabSet.ActiveTab.TabView?.OnGUI();
            }
            EditorGUILayout.EndScrollView();
        }

        public static string ErrorTextField(string label, string text)
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.PrefixLabel(label, styleErrorLabel, styleErrorLabel);
                text = EditorGUILayout.TextField(text, styleErrorText);
            }
            EditorGUILayout.EndHorizontal();
            return text;
        }

        public static int ErrorPopup(string label, int selectIndex, string[] displayedOptions)
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.PrefixLabel(label, styleErrorLabel, styleErrorLabel);
                selectIndex = EditorGUILayout.Popup(selectIndex, displayedOptions);
            }
            EditorGUILayout.EndHorizontal();
            return selectIndex;
        }

        private static Dictionary<Type, string[]> s_subTypes = new Dictionary<Type, string[]>();
        public static string SubTypeField(GUIContent label, string selectedType, Type type, bool allowNone)
        {
            string[] subTypes;
            if (!s_subTypes.TryGetValue(type, out subTypes))
            {
                List<Type> types = TypeUtil.GetSubTypes(type, type.Assembly);
                subTypes = new string[allowNone ? types.Count + 1 : types.Count];
                if (allowNone)
                {
                    subTypes[0] = "None";
                }
                for (int i = 0; i < types.Count; i++)
                {
                    if (allowNone)
                    {
                        subTypes[i + 1] = types[i].FullName;
                    }
                    else
                    {
                        subTypes[i] = types[i].FullName;
                    }
                }
                s_subTypes.Add(type, subTypes);
            }
            int selectIndex = EditorGUILayout.Popup(label, ArrayUtility.FindIndex(subTypes, (t) => t == selectedType), Array.ConvertAll(subTypes, (input) => new GUIContent(input)));
            return subTypes.Length > 0 ? subTypes[Mathf.Max(selectIndex, 0)] : string.Empty;
        }

        private static Color s_savedColor;
        public static void BeginColor(Color color)
        {
            s_savedColor = GUI.color;
            GUI.color = color;
        }

        public static void EndColor()
        {
            GUI.color = s_savedColor;
        }

        private static bool s_savedEnable;
        public static void BeginEnable(bool enable)
        {
            s_savedEnable = GUI.enabled;
            GUI.enabled = enable;
        }

        public static void EndEnable()
        {
            GUI.enabled = s_savedEnable;
        }
    }
}