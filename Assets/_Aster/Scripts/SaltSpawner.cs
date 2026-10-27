using Aster;
using Shared;
using UnityEngine;

namespace Aster
{
    public class SaltSpawner : MonoBehaviour
    {
        public Salt depositPrefab;  
        public float respawnTime = 30f;
        public float spawnTimer;         
        public StatBar bar;

        Salt current;
        GridOccupant plot;
        bool started;

        void Start() { plot = GetComponentInParent<GridOccupant>(); }

        void Update()
        {
            if (!started)
            {
                if (plot != null && !plot.IsPlaced) return;  // not yet placed
                AchievementsManager.Instance.CompleteAchievement("Edible decorations");
                started = true;
                Spawn();
            }

            if (current == null)
            {
                spawnTimer += Time.deltaTime;
                if (spawnTimer >= respawnTime) Spawn();
            }
            if (bar != null) bar.Set(current == null ? spawnTimer : respawnTime, respawnTime);
        }

        void Spawn()
        {
            spawnTimer = 0f;
            current = Instantiate(depositPrefab, transform.position, transform.rotation, transform);
        }
    }
}
