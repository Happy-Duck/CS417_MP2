using UnityEngine;
using Shared;

namespace Aster
{
    public class RandomSpawner : MonoBehaviour
    {
        public GridSpawner spawner;
        public string RandomName = "Random";
        public int maxAlive = 6;
        public Vector2 interval = new Vector2(10f, 25f);

        float timer, next;

        void Start() { next = Random.Range(interval.x, interval.y); }

        void Update()
        {
            timer += Time.deltaTime;
            if (timer < next) return;
            timer = 0f;
            next = Random.Range(interval.x, interval.y);
            if (Harvestable.CountOf(RandomName) < maxAlive) spawner.SpawnRandom(1);
        }
    }
}
