using System.Collections;
using Shared;
using UnityEngine;

namespace Rishi {
	// starting hay comes from GridSpawner's spawnOnStart
	public class HaySpawnTimer : MonoBehaviour {

		[SerializeField] GridSpawner spawner;
		[SerializeField] string countedStatName = "Feed";
		[SerializeField] float minSeconds = 15f;
		[SerializeField] float maxSeconds = 40f;
		[SerializeField] int maxAlive = 6;

		IEnumerator Start() {
			if (spawner == null) {
				Debug.LogWarning("HaySpawnTimer on " + gameObject.name + " has no spawner");
				yield break;
			}

			while (true) {
				yield return new WaitForSeconds(Random.Range(minSeconds, maxSeconds));

				if (CountAlive() < maxAlive) {
					spawner.SpawnRandom(1);
				}
			}
		}

		private int CountAlive() {
			int count = 0;
			RefillItem[] items = FindObjectsByType<RefillItem>(FindObjectsSortMode.None);
			foreach (RefillItem item in items) {
				if (item.StatName == countedStatName) count++;
			}
			return count;
		}
	}
}
