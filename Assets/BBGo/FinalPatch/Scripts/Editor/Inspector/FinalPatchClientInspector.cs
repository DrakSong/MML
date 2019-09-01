using UnityEditor;
using UnityEngine;

namespace BBGo.FinalPatch
{
    [CustomEditor(typeof(FinalPatchClient))]
    public class FinalPatchInspector : Editor
    {
        private bool m_cacheFoldout = true;
        private bool m_poolFoldout = true;
        private FinalPatchClient m_target;

        private void OnEnable()
        {
            EditorApplication.update += OnUpdate;
            m_target = target as FinalPatchClient;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnUpdate;
        }

        private void OnUpdate()
        {
            if (!Application.isPlaying)
                return;

            Repaint();
        }

        public override void OnInspectorGUI()
        {
            BBGoGUI.BeginEnable(!Application.isPlaying);
            {
                m_target.deployDataUrl = EditorGUILayout.TextField(new GUIContent(Localization.GetString("final_patch_deploy_url")), m_target.deployDataUrl);
                m_target.channel = EditorGUILayout.TextField(new GUIContent(Localization.GetString("final_patch_deploy_channel")), m_target.channel);
            }
            BBGoGUI.EndEnable();

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox(Localization.GetString("final_patch_playing_mode_tip"), MessageType.Info);
            }
            else
            {
                m_poolFoldout = EditorGUILayout.Foldout(m_poolFoldout, Localization.GetString("final_patch_game_object_pool_label"));
                if (m_poolFoldout)
                {
                    foreach (var pool in FinalPool.Pools.Values)
                    {
                        EditorGUILayout.BeginVertical(GUI.skin.box);
                        {
                            EditorGUILayout.LabelField(Localization.GetString("final_patch_pool_asset_name_label"), pool.AssetName);
                            EditorGUILayout.LabelField(Localization.GetString("final_patch_pool_ref_count_label"), pool.ReferenceCount.ToString());
                            EditorGUILayout.LabelField(Localization.GetString("final_patch_pool_free_count_label"), pool.FreeCount.ToString());
                            if (pool.AutoRelease)
                            {
                                BBGoGUI.BeginEnable(pool.ReferenceCount == 0);
                                {
                                    EditorGUILayout.LabelField(Localization.GetString("final_patch_pool_release_countdown_label"), pool.ReferenceCount > 0 ? pool.AutoReleaseDuration.ToString() : (pool.ExpiredTime - Time.time).ToString());
                                }
                                BBGoGUI.EndEnable();
                            }
                        }
                        EditorGUILayout.EndVertical();
                    }
                }

                m_cacheFoldout = EditorGUILayout.Foldout(m_cacheFoldout, Localization.GetString("final_patch_cache_label"));
                if (m_cacheFoldout)
                {
                    foreach (var cache in FinalLoader.AssetNameCaches.Values)
                    {
                        EditorGUILayout.BeginVertical(GUI.skin.box);
                        {
                            EditorGUILayout.LabelField(Localization.GetString("final_patch_cache_name_label"), cache.Name);
                            EditorGUILayout.LabelField(Localization.GetString("final_patch_cache_state_label"), cache.AssetState.ToString());
                            EditorGUILayout.LabelField(Localization.GetString("final_patch_cache_ref_count_label"), cache.ReferenceCount.ToString());
                        }
                        EditorGUILayout.EndVertical();
                    }
                }
            }
        }
    }
}