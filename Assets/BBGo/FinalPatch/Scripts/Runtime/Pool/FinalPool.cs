using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BBGo.FinalPatch
{
    public static class FinalPool
    {
        public static Dictionary<string, GameObjectPool> Pools { get; } = new Dictionary<string, GameObjectPool>();

        private static FinalPatchData s_data;
        internal static void Init(FinalPatchData data)
        {
            s_data = data;
        }

        internal static void Reset()
        {
            if (Pools.Count > 0)
            {
                GameObjectPool[] pools = new GameObjectPool[Pools.Count];
                int i = 0;
                foreach (var pool in Pools.Values)
                {
                    pools[i++] = pool;
                }
                foreach (var pool in pools)
                {
                    DestroyGameObjectPool(pool);
                }
            }
        }

        private static List<GameObjectPool> s_releasePools = new List<GameObjectPool>();
        internal static void Update()
        {
            foreach (var pool in Pools.Values)
            {
                if (pool.CanAutoRelease())
                {
                    s_releasePools.Add(pool);
                }
            }

            if (s_releasePools.Count > 0)
            {
                foreach (var pool in s_releasePools)
                {
                    DestroyGameObjectPool(pool);
                }
                s_releasePools.Clear();
            }
        }

        public static async Task<GameObject> GetGameObjectAsync(string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
                return null;

            GameObjectPool pool = await GetOrCreateGameObjectPoolAsync(assetName);
            return pool?.GetObject();
        }

        private static SemaphoreSlim s_getOrCreateGameObjectPoolSemaphore = new SemaphoreSlim(1, 1);
        private static async Task<GameObjectPool> GetOrCreateGameObjectPoolAsync(string assetName)
        {
            GameObjectPool pool;
            try
            {
                await s_getOrCreateGameObjectPoolSemaphore.WaitAsync();
                if (!Pools.TryGetValue(assetName, out pool))
                {
                    GameObject asset = await FinalLoader.LoadAssetAsync<GameObject>(assetName);
                    if (asset != null)
                    {
                        pool = new GameObjectPool(asset, assetName, s_data.poolMaxCache, s_data.poolEnableAutoRelease, s_data.poolAutoReleaseDuration);
                        Pools.Add(assetName, pool);
                    }
                }
            }
            finally
            {
                s_getOrCreateGameObjectPoolSemaphore.Release();
            }
            return pool;
        }

        public static void ReleaseGameObject(GameObject obj)
        {
            if (obj == null)
                return;

            GameObjectPool.PooledObject po = obj.GetComponent<GameObjectPool.PooledObject>();
            if (po == null)
            {
                Debug.LogWarning($"Release Object failure: can't find PooledObject component");
                return;
            }

            Object.Destroy(po);
            po.pool.ReleaseObject(obj);
        }

        internal static void DestroyGameObjectPool(GameObjectPool pool)
        {
            if (pool == null)
                return;

            pool.DestroyPool();
            Pools.Remove(pool.AssetName);
        }

        public static void DestroyUnusedPools()
        {
            List<GameObjectPool> toDesotry = new List<GameObjectPool>();
            foreach (var pool in Pools.Values)
            {
                if (pool.ReferenceCount == 0)
                {
                    toDesotry.Add(pool);
                }
            }

            for (int i = 0; i < toDesotry.Count; i++)
            {
                GameObjectPool pool = toDesotry[i];
                DestroyGameObjectPool(pool);
            }
        }
    }
}