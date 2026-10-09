using UnityEngine;

namespace Rishi {
	public class ChickenConsumer : MonoBehaviour {

		[SerializeField] string chickenStat = "Hunger";
		[SerializeField] string sourceStat = "Remaining Food";
		[SerializeField] float drainPerSecond = 5f;
		// negative raises it
		[SerializeField] float reducePerSecond = 10f;

		private Stats sourceStats;

		void Start() {
			if (sourceStat == "") return;

			sourceStats = GetComponentInParent<Stats>();
			if (sourceStats == null) {
				Debug.LogWarning("ChickenConsumer on " + gameObject.name + " has a sourceStat but can't find a Stats on it or its parents");
			}
		}

		void OnTriggerStay(Collider other) {
			Stats chicken = other.GetComponentInParent<Stats>();
			if (chicken == null) return;
			if (!chicken.HasStat(chickenStat)) return;

			bool hasSource = sourceStat != "";
			if (hasSource) {
				if (sourceStats == null) return;
				if (sourceStats.getStat(sourceStat) <= 0f) return;
			}

			float normalized = chicken.getStatNormalized(chickenStat);
			if (reducePerSecond > 0f && normalized <= 0f) return;
			if (reducePerSecond < 0f && normalized >= 1f) return;

			chicken.changeStat(chickenStat, -reducePerSecond * Time.deltaTime);
			if (hasSource) sourceStats.changeStat(sourceStat, -drainPerSecond * Time.deltaTime);
		}
	}
}
