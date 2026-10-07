using System.Collections.Generic;
using UnityEngine;

namespace Shared {

	// The shared grid, every "building" from every section should snap to this
	// cells are 0.5 units, and cell (0, 0) is centered on the world origin
	// A cell is a Vector2Int where x is the world X column and y is the world Z row (not to be confused with unity y which is height)
	// You usually don't need the occupancy functions yourself, put a GridOccupant on your object instead
	// use like this: Vector3 snapped = GridManager.Instance.Snap(somePosition);                       // for 1x1 things
	//				  Vector3 snapped = GridManager.Instance.SnapFootprint(somePosition, new Vector2Int(2, 3)); // for bigger things
	public class GridManager : MonoBehaviour {
		[SerializeField] private float cellSize = 0.5f;
		[SerializeField] private Vector3 origin = Vector3.zero;

		private readonly Dictionary<Vector2Int, GameObject> cellToObject = new Dictionary<Vector2Int, GameObject>();
		private readonly Dictionary<GameObject, List<Vector2Int>> objectToCells = new Dictionary<GameObject, List<Vector2Int>>();

		// lets any script reach the grid with GridManager.Instance
		public static GridManager Instance { get; private set; }

		public float CellSize {
			get { return cellSize; }
		}

		// Makes sure theres only ever one GridManager, and sets Instance so other scripts can find it
		private void Awake() {
			if (Instance != null && Instance != this) {
				Debug.LogWarning("There was a duplicate GridManager, its been destroyed");
				Destroy(gameObject);
				return;
			}
			Instance = this;
		}
		private void OnDestroy() {
			if (Instance == this) Instance = null;
		}

		private static int RoundHalfUp(float value) {
			return Mathf.FloorToInt(value + 0.5f);
		}

		// which cell a world position is in (rounds to the nearest cell)
		public Vector2Int WorldToCell(Vector3 worldPosition) {
			Vector3 local = worldPosition - origin;
			int x = RoundHalfUp(local.x / cellSize);
			int z = RoundHalfUp(local.z / cellSize);
			return new Vector2Int(x, z);
		}

		// the center of a cell in world space, at whatever height you give it
		public Vector3 CellToWorld(Vector2Int cell, float height) {
			float x = origin.x + cell.x * cellSize;
			float z = origin.z + cell.y * cellSize;
			return new Vector3(x, height, z);
		}

		// rounds a world position to the nearest cell center, keeps the height the same (for 1x1 things)
		public Vector3 Snap(Vector3 worldPosition) {
			return CellToWorld(WorldToCell(worldPosition), worldPosition.y);
		}


		// This is for multi cell objects, which is likely most of them
		// A footprint is described by its start cell (its lowest x, lowest z cell) and its size in cells
		// The object's pivot goes at the CENTER of the footprint, which for even sizes is on a cell edge, not a cell center

		// the start cell for an object of this size, placed so its center is as close as possible to worldPosition
		public Vector2Int GetFootprintStart(Vector3 worldPosition, Vector2Int size) {
			Vector3 local = worldPosition - origin;
			int width = Mathf.Max(1, size.x);
			int depth = Mathf.Max(1, size.y);
			// shift back by half the object's size so the object's center (not its corner) lands near the position
			int startX = RoundHalfUp(local.x / cellSize - (width - 1) / 2f);
			int startZ = RoundHalfUp(local.z / cellSize - (depth - 1) / 2f);
			return new Vector2Int(startX, startZ);
		}

		// the world position of the center of a footprint, this is where the object's pivot should go
		public Vector3 GetFootprintCenter(Vector2Int start, Vector2Int size, float height) {
			int width = Mathf.Max(1, size.x);
			int depth = Mathf.Max(1, size.y);
			float x = origin.x + (start.x + (width - 1) / 2f) * cellSize;
			float z = origin.z + (start.y + (depth - 1) / 2f) * cellSize;
			return new Vector3(x, height, z);
		}

		// snaps a world position to where an object of this size should sit, keeps the height the same
		public Vector3 SnapFootprint(Vector3 worldPosition, Vector2Int size) {
			return GetFootprintCenter(GetFootprintStart(worldPosition, size), size, worldPosition.y);
		}

		// every cell an object of this size covers, starting at the start cell
		public List<Vector2Int> GetFootprintCells(Vector2Int start, Vector2Int size) {
			int width = Mathf.Max(1, size.x);
			int depth = Mathf.Max(1, size.y);

			List<Vector2Int> cells = new List<Vector2Int>();
			for (int x = 0; x < width; x++) {
				for (int z = 0; z < depth; z++) {
					cells.Add(new Vector2Int(start.x + x, start.y + z));
				}
			}
			return cells;
		}


		// true if something is in this cell
		public bool IsOccupied(Vector2Int cell) {
			if (!cellToObject.TryGetValue(cell, out GameObject building)) return false;
			if (building != null) return true;

			// the building was destroyed but never released, clean it up
			cellToObject.Remove(cell);
			return false;
		}

		// the building in this cell, or null if its empty
		public GameObject GetOccupant(Vector2Int cell) {
			if (IsOccupied(cell)) return cellToObject[cell];
			return null;
		}

		// true if every cell a building of this size would cover is empty
		public bool IsAreaFree(Vector2Int start, Vector2Int size) {
			foreach (Vector2Int cell in GetFootprintCells(start, size)) {
				if (IsOccupied(cell)) return false;
			}
			return true;
		}

		// marks the cells as taken by this building, returns false and changes nothing if anything is in the way
		// GridOccupant calls this for you
		public bool TryOccupy(Vector2Int start, Vector2Int size, GameObject building) {
			if (building == null || !IsAreaFree(start, size)) return false;

			List<Vector2Int> cells = GetFootprintCells(start, size);
			foreach (Vector2Int cell in cells) {
				cellToObject[cell] = building;
			}
			objectToCells[building] = cells;
			return true;
		}

		// frees every cell this building covers, GridOccupant calls this for you when its picked up or destroyed
		public void Release(GameObject building) {
			if (building == null || !objectToCells.TryGetValue(building, out List<Vector2Int> cells)) return;
			foreach (Vector2Int cell in cells) {
				if (cellToObject.TryGetValue(cell, out GameObject inCell) && inCell == building) {
					cellToObject.Remove(cell);
				}
			}
			objectToCells.Remove(building);
		}

	}
}