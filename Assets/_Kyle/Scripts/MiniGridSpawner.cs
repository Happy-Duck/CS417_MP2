using UnityEngine;
using System.Collections;
using Shared;

namespace Kyle
{
    public class MiniGridSpawner : MonoBehaviour
    {
        [SerializeField] private CropVisuals prefab;
        [SerializeField] private Transform spawnParent;
        [SerializeField] private int spawnOnStart = 0;
        [SerializeField] private int gridWidth;
        [SerializeField] private int gridHeight;

        private IEnumerator Start()
        {
            yield return null;
            if (spawnOnStart > 0) { SpawnRandom(spawnOnStart); }
        }

        public GameObject SpawnAt(Vector2Int cell)
        {
            GridManager grid = GridManager.Instance;

            if (!grid.IsAreaFree(cell, prefab.GetComponent<GridOccupant>().Size)) { return null; }
            CropVisuals spawned = Instantiate(prefab, Vector3.zero, Quaternion.identity, spawnParent);

            if (spawned.Plant(cell))
            {
                return spawned.gameObject;
            } else
            {
                Destroy(spawned.gameObject);
                return null;
            }
        }

        public int SpawnRandom(int count)
        {
            int spawned = 0;
            int attempts = 0;
            while (spawned < count && attempts < count * 10)
            {
                attempts++;
                int randomX = Random.Range(0, gridWidth);
                int randomZ = Random.Range(0, gridHeight);

                if (SpawnAt(new Vector2Int(randomX, randomZ)) != null) { spawned++; }
            }
            return spawned;
        }
    }
}
