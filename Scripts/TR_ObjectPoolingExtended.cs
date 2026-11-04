using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsFPSKit.Optimization
{
    // Extends the kit's default pool with:
    // - WarmUp(): preloads a list of prefabs before match start
    // - GetPooled<T>(): typed convenience wrapper
    // Assign this in place of Kit_ObjectPoolingDefault on the IngamePrefab.
    public class TR_ObjectPoolingExtended : Kit_ObjectPoolingDefault
    {
        [System.Serializable]
        public class PoolEntry
        {
            public GameObject prefab;
            public int preloadCount = 5;
        }

        [SerializeField] PoolEntry[] warmUpEntries;

        // Call from Kit_PvE_TreasureRoyale.GamemodeSetup or a loading screen
        public IEnumerator WarmUp()
        {
            foreach (var entry in warmUpEntries)
            {
                if (entry.prefab == null) continue;
                EnqueueInstantiateable(entry.prefab, entry.preloadCount);
                yield return null; // spread across frames
            }
        }

        public T GetPooled<T>(GameObject prefab, Vector3 pos, Quaternion rot) where T : Component
        {
            var go = GetInstantiateable(prefab, pos, rot);
            return go != null ? go.GetComponent<T>() : null;
        }

        public void ReturnToPool(GameObject go)
        {
            DestroyInstantiateable(go);
        }
    }
}
