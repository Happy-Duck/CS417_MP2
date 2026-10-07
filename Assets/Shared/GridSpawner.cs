using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Shared {

	// Example of spawning a prefab snapped to the grid, for things the GAME spawns (not the player)
	// The prefab needs a GridOccupant on it (the Inspector won't let you drag in one that doesn't)
	// Put the spawner on the ground in your area, random spawns land around it at its height
	// Copy it into your own folder and edit it, or call it from your own scripts:
	//		[SerializeField] private GridSpawner cornSpawner;
	//		cornSpawner.SpawnAt(somePosition);     // returns null if the spot was taken
	//		cornSpawner.SpawnRandom(1);            // one at a random free spot near the spawner
	public class GridSpawner : MonoBehaviour {
		[SerializeField] private GridOccupant prefab;

		// put your area's root here so spawned stuff stays organized under your section
		[SerializeField] private Transform spawnParent;

		// how many to spawn at random spots when the game starts (0 for none)
		[SerializeField] private int spawnOnStart = 0;
		[SerializeField] private float randomRadius = 5f;

		[SerializeField] private bool spaceKeyTesting = true;

		// Start can be a coroutine, waiting one frame lets everything placed by hand claim its cells first
		// so random spawns don't land on top of them
		private IEnumerator Start() {
			yield return null;
			if (spawnOnStart > 0) SpawnRandom(spawnOnStart);
		}

		// snaps the position to the grid and spawns the prefab there
		// returns the new object, or null if the spot was taken
		public GameObject SpawnAt(Vector3 worldPosition) {
			GridManager grid = GridManager.Instance;
			if (grid == null) {
				Debug.LogError("GridSpawner: there's no GridManager in the scene");
				return null;
			}
			if (prefab == null) {
				Debug.LogError("GridSpawner: no prefab assigned", this);
				return null;
			}

			// 1. find where it would go and make sure the whole footprint is empty (so we never spawn something we'd have to delete)
			Vector2Int start = grid.GetFootprintStart(worldPosition, prefab.Size);
			if (!grid.IsAreaFree(start, prefab.Size)) return null;

			// 2. spawn it at the footprint's center, keeping the prefab's own rotation
			Vector3 position = grid.GetFootprintCenter(start, prefab.Size, worldPosition.y);
			GridOccupant spawned = Instantiate(prefab, position, prefab.transform.rotation, spawnParent);

			// 3. claim the cells (the GridOccupant frees them when its destroyed)
			spawned.TryPlaceAt(position);
			return spawned.gameObject;
		}

		// spawns up to count copies at random free spots within randomRadius of the spawner
		// returns how many it actually managed to place (it skips spots that are taken)
		public int SpawnRandom(int count) {
			int spawned = 0;
			int attempts = 0;
			while (spawned < count && attempts < count * 10) {
				attempts++;
				Vector2 offset = Random.insideUnitCircle * randomRadius;
				Vector3 spot = transform.position + new Vector3(offset.x, 0f, offset.y);
				if (SpawnAt(spot) != null) spawned++;
			}
			return spawned;
		}
	}
}