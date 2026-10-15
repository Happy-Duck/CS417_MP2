using System.Collections;
using UnityEngine;

namespace Alyssa
{
    public class WeedSpawner : MonoBehaviour
    {
        public GameObject weedPrefab;
        public Transform spawnParent;                      // Alyssa_Scene_Root
        public int initialCount = 3;
        public float startDelay = 1f;                      // lets beds exist before the first weeds
        public bool intermittent = true;
        public Vector2 intervalRange = new Vector2(20f, 45f);

        IEnumerator Start()
        {
            yield return new WaitForSeconds(startDelay);
            for (int i = 0; i < initialCount; i++) TrySpawn();     // Wild Instances
            while (intermittent)                                   // Intermittent Instances
            {
                yield return new WaitForSeconds(Random.Range(intervalRange.x, intervalRange.y));
                TrySpawn();
            }
        }

        void TrySpawn()
        {
            if (weedPrefab == null) return;
            var beds = FindObjectsByType<SoilBed>();
            var open = new System.Collections.Generic.List<SoilBed>();
            foreach (var b in beds)
                if (b.plant == null && !b.HasWeed) open.Add(b);
            if (open.Count == 0) return;

            var bed = open[Random.Range(0, open.Count)];
            var go = Instantiate(weedPrefab, bed.transform.position + bed.plantOffset,
                     Quaternion.Euler(0, Random.Range(0, 360f), 0), bed.transform);
            bed.weed = go.GetComponent<Weed>();
        }
    }
}