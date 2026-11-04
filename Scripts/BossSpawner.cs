using Photon.Pun;
using UnityEngine;

namespace MarsFPSKit
{
    // Attach to a persistent scene object.
    // MasterClient-only boss spawning keyed by era string.
    public class BossSpawner : MonoBehaviour
    {
        [System.Serializable]
        public struct EraBoss
        {
            public string eraId;
            public GameObject prefab;
        }

        [SerializeField] EraBoss[] eraBosses;
        [SerializeField] float spawnOffsetRadius = 5f;

        string _currentEraId;

        public void SetEra(string eraId) => _currentEraId = eraId;

        public void SpawnBossAt(Vector3 position)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            GameObject prefab = GetPrefabForEra(_currentEraId);
            if (prefab == null) return;

            Vector2 offset = Random.insideUnitCircle * spawnOffsetRadius;
            Vector3 spawnPos = position + new Vector3(offset.x, 0f, offset.y);

            PhotonNetwork.InstantiateSceneObject(prefab.name, spawnPos, Quaternion.identity);
        }

        GameObject GetPrefabForEra(string eraId)
        {
            foreach (var entry in eraBosses)
                if (entry.eraId == eraId) return entry.prefab;
            // Fallback to first entry
            return eraBosses.Length > 0 ? eraBosses[0].prefab : null;
        }
    }
}
