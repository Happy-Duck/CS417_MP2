using UnityEngine;

namespace Shared {

	// Put this on anything that lives on the grid (feeders, nest boxes, hay bales, etc)
	// Anything with this placed in the scene by hand snaps to the grid and claims its cells when the game starts
	// It frees its cells automatically when its destroyed
	// Make sure the prefab's pivot is at the center of its base, or it'll sink into the floor or sit off center when it snaps
	// use like this: GetComponent<GridOccupant>().TryPlaceAt(somePosition);   // move it on the grid
	//				  GetComponent<GridOccupant>().RemoveFromGrid();           // take it off the grid
	public class GridOccupant : MonoBehaviour {
		// size in cells, for example (3, 3) for a 1.5x1.5 unity units object
		[SerializeField] private Vector2Int size = Vector2Int.one;

		// turn off for things that shouldn't start on the grid (GridPlaceable turns this off and handles it itself)
		[SerializeField] private bool registerOnStart = true;

		public Vector2Int Size {
			get { return size; }
		}
		public bool RegisterOnStart {
			get { return registerOnStart; }
			set { registerOnStart = value; }
		}
		public bool IsPlaced { get; private set; }
		// the lowest x, lowest z cell this object covers
		public Vector2Int StartCell { get; private set; }

		private void Start() {
			if (registerOnStart && !IsPlaced) {
				if (!TryPlaceAt(transform.position)) {
					Debug.LogWarning($"GridOccupant: {name} overlaps something on the grid and wasn't placed", this);
				}
			}
		}

		// snaps this object to the nearest spot at that position and claims the cells
		// returns false and doesn't move it if anything is in the way
		public bool TryPlaceAt(Vector3 worldPosition) {
			GridManager grid = GridManager.Instance;
			if (grid == null) {
				Debug.LogError("GridOccupant: there's no GridManager in the scene");
				return false;
			}

			Vector2Int start = grid.GetFootprintStart(worldPosition, size);

			// if its already on the grid this is a move, so free the old cells first so it doesn't block itself
			bool wasPlaced = IsPlaced;
			Vector2Int oldStart = StartCell;
			if (wasPlaced) RemoveFromGrid();

			if (!grid.TryOccupy(start, size, gameObject)) {
				// couldn't move there, so put it back on its old cells
				if (wasPlaced && grid.TryOccupy(oldStart, size, gameObject)) IsPlaced = true;
				return false;
			}

			MoveTo(grid.GetFootprintCenter(start, size, worldPosition.y));
			StartCell = start;
			IsPlaced = true;
			return true;
		}

		// frees this object's cells (it stays in the scene, its just not on the grid anymore)
		public void RemoveFromGrid() {
			if (!IsPlaced) return;
			// the grid might already be gone if the game is shutting down
			if (GridManager.Instance != null) GridManager.Instance.Release(gameObject);
			IsPlaced = false;
		}

		private void OnDestroy() {
			RemoveFromGrid();
		}

		// moves the transform and the rigidbody together, otherwise a rigidbody (especially one with interpolation on)
		// can keep drawing the object at its old physics position
		private void MoveTo(Vector3 position) {
			transform.position = position;
			Rigidbody body = GetComponent<Rigidbody>();
			if (body != null) body.position = position;
		}
	}
}