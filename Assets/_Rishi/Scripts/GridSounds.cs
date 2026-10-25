using Shared;
using UnityEngine;

namespace Rishi {
	[RequireComponent(typeof(GridPlaceable))]
	public class GridSounds : MonoBehaviour {

		[SerializeField] AudioClip pickUpSound;
		[SerializeField] AudioClip placeSound;
		[SerializeField] AudioClip purchaseSound;
		// cant afford it or the spot is taken
		[SerializeField] AudioClip failSound;

		[Range(0f, 1f)]
		[SerializeField] float volume = 1f;

		private GridPlaceable placeable;

		void Awake() {
			placeable = GetComponent<GridPlaceable>();
		}

		void OnEnable() {
			placeable.PickedUp += HandlePickedUp;
			placeable.Placed += HandlePlaced;
			placeable.Purchased += HandlePurchased;
			placeable.PlaceFailed += HandlePlaceFailed;
		}

		void OnDisable() {
			placeable.PickedUp -= HandlePickedUp;
			placeable.Placed -= HandlePlaced;
			placeable.Purchased -= HandlePurchased;
			placeable.PlaceFailed -= HandlePlaceFailed;
		}

		private void HandlePickedUp(GridPlaceable item) {
			Play(pickUpSound);
		}

		private void HandlePlaced(GridPlaceable item) {
			Play(placeSound);
		}

		private void HandlePurchased(GridPlaceable item) {
			Play(purchaseSound);
		}

		private void HandlePlaceFailed(GridPlaceable item, string reason) {
			if (reason == "") return;
			Play(failSound);
			Popups.Hint("place" + GetHashCode(), reason, transform.position);
		}

		private void Play(AudioClip clip) {
			if (clip == null) return;
			AudioSource.PlayClipAtPoint(clip, transform.position, volume);
		}
	}
}
