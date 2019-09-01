using System.Collections.Generic;
using UnityEngine;

namespace BBGo.FinalPatch
{
    public class GameObjectPool
    {
        public string AssetName { get; private set; }
        public bool AutoRelease { get; private set; }
        public float AutoReleaseDuration { get; private set; }
        public float ExpiredTime { get; private set; }
        public int ReferenceCount { get { return m_reference.Count; } }
        public int FreeCount { get { return m_free.Count; } }

        private GameObject m_asset;
        private int m_maxCacheCount;
        private LinkedList<GameObject> m_reference;
        private LinkedList<GameObject> m_free;

        internal GameObjectPool(GameObject asset, string assetName, int maxCacheCount, bool autoRelease, float autoReleaseDuration)
        {
            m_asset = asset;
            AutoRelease = autoRelease;
            AutoReleaseDuration = autoReleaseDuration;
            ExpiredTime = Time.time + autoReleaseDuration;
            AssetName = assetName;
            m_maxCacheCount = maxCacheCount;
            m_reference = new LinkedList<GameObject>();
            m_free = new LinkedList<GameObject>();
        }

        public bool CanAutoRelease()
        {
            if (!AutoRelease)
                return false;

            if (m_reference.Count > 0)
                return false;

            if (Time.time < ExpiredTime)
                return false;

            return true;
        }

        internal GameObject GetObject()
        {
            GameObject go = null;
            if (m_free.Count == 0)
            {
                go = Object.Instantiate(m_asset);
            }
            else
            {
                go = m_free.First.Value;
                m_free.RemoveFirst();
            }
            go.AddComponent<PooledObject>().pool = this;
            go.SetActive(true);
            m_reference.AddLast(go);
            return go;
        }

        internal void ReleaseObject(GameObject go)
        {
            if (!m_reference.Contains(go))
                return;

            m_reference.Remove(go);
            if (m_reference.Count == 0)
            {
                ExpiredTime = Time.time + AutoReleaseDuration;
            }
            if (m_free.Count < m_maxCacheCount)
            {
                m_free.AddLast(go);
                go.SetActive(false);
            }
            else
            {
                Object.Destroy(go.gameObject);
            }
        }

        internal void DestroyPool()
        {
            if (m_reference.Count > 0)
            {
                GameObject[] objects = new GameObject[m_reference.Count];
                int i = 0;
                foreach (var obj in m_reference)
                {
                    objects[i++] = obj;
                }
                foreach (var obj in objects)
                {
                    ReleaseObject(obj);
                }
            }

            foreach (var free in m_free)
            {
                Object.Destroy(free);
            }
            m_free.Clear();

            FinalLoader.UnloadAsset(m_asset);
            m_asset = null;
        }

        internal class PooledObject : MonoBehaviour
        {
            public GameObjectPool pool;
        }
    }
}