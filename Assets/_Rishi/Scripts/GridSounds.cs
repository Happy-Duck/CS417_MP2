using Shared;
using UnityEngine;

namespace Rishi {
	[RequireComponent(typeof(GridPlaceable))]
	public class GridSounds : MonoBehaviour {

		[SerializeField] AudioClip pickUpSound;
		[SerializeField] AudioClip placeSound;
		[SerializeField] AudioClip purchaseSound;

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
		}

		void OnDisable() {
			placeable.PickedUp -= HandlePickedUp;
			placeable.Placed -= HandlePlaced;
			placeable.Purchased -= HandlePurchased;
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

		private void Play(AudioClip clip) {
			if (clip == null) return;
			AudioSource.PlayClipAtPoint(clip, transform.position, volume);
		}
	}
}
