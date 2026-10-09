using UnityEngine;

namespace Rishi {
	[RequireComponent(typeof(Stats))]
	[RequireComponent(typeof(Harvestable))]
	public class Freshness : MonoBehaviour {

		[SerializeField] string rottenResourceName = "RottenBullshit";

		private Stats stats;
		private Harvestable harvestable;

		void Start() {
			stats = GetComponent<Stats>();
			harvestable = GetComponent<Harvestable>();
			stats.OnStatReachedMin += HandleMin;
		}

		private void HandleMin(string stat) {
			switch (stat) {
				case "Freshness":
					harvestable.SetResource(rottenResourceName);
					break;
			}
		}

		void OnDestroy() {
			if (stats == null) return;
			stats.OnStatReachedMin -= HandleMin;
		}
	}
}
