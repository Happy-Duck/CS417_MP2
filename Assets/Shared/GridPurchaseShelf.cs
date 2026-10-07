using UnityEngine;

namespace Shared {

	// An endless supply of one placeable item 
	// Keeps one item sitting at the display point, when the player grabs it a new one shows up after a short delay
	// The item isn't paid for until the player actually places it on the grid
	public class GridPurchaseShelf : MonoBehaviour {
		[SerializeField] private GridPlaceable itemPrefab;

		// where the item sits on the shelf, uses this object's position if left empty
		[SerializeField] private Transform displayPoint;

		// put your area's root here so the items stay organized under your section
		[SerializeField] private Transform spawnParent;

		// gives the player's hand time to move away so the new item doesn't bump into the one they're holding
		[SerializeField] private float restockDelay = 1f;

		private void Start() {
			Restock();
		}

		private void Restock() {
			if (itemPrefab == null) {
				Debug.LogError("GridPurchaseShelf: no item prefab assigned", this);
				return;
			}

			Transform point = displayPoint;
			if (point == null) point = transform;

			GridPlaceable item = Instantiate(itemPrefab, point.position, point.rotation, spawnParent);
			item.MakeShelfStock();
			item.TakenFromShelf += OnItemTaken;
		}

		private void OnItemTaken(GridPlaceable item) {
			item.TakenFromShelf -= OnItemTaken;
			Invoke(nameof(Restock), restockDelay);
		}
	}
}
