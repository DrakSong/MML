using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BBGo.FinalPatch
{
    public class DemoTester : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private Queue<GameObject> list = new Queue<GameObject>();
        public const float BTN_HEIGHT = 80;
        public const float BTN_WIDTH = 150;
        public const float BTN_MARGIN = 10;

        private void OnGUI()
        {
            float top = 50;
            if (GUI.Button(new Rect(5, top, BTN_WIDTH, BTN_HEIGHT), new GUIContent("Load Bundle")))
            {
                LoadBundle();
            }

            top += BTN_HEIGHT + BTN_MARGIN;
            if (GUI.Button(new Rect(5, top, BTN_WIDTH, BTN_HEIGHT), new GUIContent("Unload Bundle")))
            {
                UnloadBundle();
            }

            top += BTN_HEIGHT + BTN_MARGIN;
            if (GUI.Button(new Rect(5, top, BTN_WIDTH, BTN_HEIGHT), new GUIContent("Load Scene")))
            {
                LoadScene();
            }

            top += BTN_HEIGHT + BTN_MARGIN;
            if (GUI.Button(new Rect(5, top, BTN_WIDTH, BTN_HEIGHT), new GUIContent("Unload Scene")))
            {
                UnloadScene();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                LoadBundle();
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                UnloadBundle();
            }
            if (Input.GetKeyDown(KeyCode.Z))
            {
                FinalPool.Reset();
                FinalLoader.Reset();
                FinalPatcher.Reset();

                FinalPatcher.AutoPatch();
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                LoadScene();
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                UnloadScene();
            }
        }

        private async void LoadBundle()
        {
            GameObject go = await FinalPool.GetGameObjectAsync("Assets/BBGo/FinalPatch/Demo/Bundles/Cube_B.prefab");
            go.transform.position = Random.insideUnitSphere * 5;
            list.Enqueue(go);
        }

        private void UnloadBundle()
        {
            if (list.Count == 0)
                return;

            FinalPool.ReleaseGameObject(list.Dequeue());
        }

        private async void LoadScene()
        {
            await FinalLoader.LoadSceneAsync("Assets/BBGo/FinalPatch/Demo/Bundles/BundleScene.unity", LoadSceneMode.Additive);
        }

        private async void UnloadScene()
        {
            await FinalLoader.UnloadSceneAsync("Assets/BBGo/FinalPatch/Demo/Bundles/BundleScene.unity");
        }
    }
}