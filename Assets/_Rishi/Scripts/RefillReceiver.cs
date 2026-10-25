using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Rishi {
	public class RefillReceiver : MonoBehaviour {

		[SerializeField] string acceptedStat = "Feed";
		[SerializeField] string targetStat = "Remaining Food";

		[SerializeField] string fullHint = "It's already full!";
		[SerializeField] string wrongItemHint = "That doesn't go in here.";

		private Stats stats;

		void Start() {
			stats =GetComponentInParent<Stats>();
			if (stats == null) {
				Debug.LogWarning("RefillReceiver on " + gameObject.name + " can't find a Stats on it or its parents");
			}
		}

		void OnTriggerEnter(Collider other) {
			if (stats == null) return;

			RefillItem item = other.GetComponentInParent<RefillItem>();
			if (item == null) return;
			if (item.IsUsed) return;
			if (item.StatName != acceptedStat) {
				if (IsHeld(item)) Popups.Hint("wrong" + GetHashCode(), wrongItemHint, transform.position);
				return;
			}

			if (stats.getStatNormalized(targetStat) >= 1f) {
				if (IsHeld(item)) Popups.Hint("full" + GetHashCode(), fullHint, transform.position);
				return;
			}

			stats.changeStat(targetStat, item.Amount);
			item.Consume();
		}

		// so hay spawning next to it doesnt spam
		private bool IsHeld(RefillItem item) {
			XRGrabInteractable grab = item.GetComponent<XRGrabInteractable>();
			return grab != null && grab.isSelected;
		}
	}
}
