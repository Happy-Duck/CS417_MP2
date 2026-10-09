using UnityEngine;

namespace Rishi {
	public class RefillReceiver : MonoBehaviour {

		[SerializeField] string acceptedStat = "Feed";
		[SerializeField] string targetStat = "Remaining Food";

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
			if (item.StatName != acceptedStat) return;

			if (stats.getStatNormalized(targetStat) >= 1f) return;

			stats.changeStat(targetStat, item.Amount);
			item.Consume();
		}
	}
}
