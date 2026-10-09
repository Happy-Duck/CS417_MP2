using UnityEngine;

namespace Rishi {
	[RequireComponent(typeof(Stats))]
	public class WateringCan : MonoBehaviour {

		[SerializeField] string canStatName = "Fullness";
		[SerializeField] string targetStatName = "Remaining Water";
		[SerializeField] float pourPerSecond = 30f;
		[SerializeField] float tiltThreshold = 0.6f;
		[SerializeField] GameObject pourWater;

		[SerializeField] AudioClip pickupWater;

		private Stats canStats;

		void Start() {
			canStats = GetComponent<Stats>();
			if (pourWater != null) pourWater.SetActive(false);
		}

		void Update() {
			bool pouring = IsTilted() && canStats.getStat(canStatName) > 0f;
			if (pouring) {
				canStats.changeStat(canStatName, -pourPerSecond * Time.deltaTime);
			}
			if (pourWater != null) pourWater.SetActive(pouring);
		}

		private bool IsTilted() {
			return transform.up.y < tiltThreshold;
		}
		void OnTriggerStay(Collider other) {
			if (canStats == null) return;

			if (IsTilted()) {
				TryPourInto(other);
			} else {
				TryRefillFrom(other);
			}
		}

		void OnTriggerEnter(Collider other) {
			if (canStats == null) return;
			WaterSource source = other.GetComponentInParent<WaterSource>();
			if (source == null) return;
			if (pickupWater != null) AudioSource.PlayClipAtPoint(pickupWater, gameObject.transform.position);

		}

		private void TryPourInto(Collider other) {
			Stats target = other.GetComponentInParent<Stats>();
			if (target == null) return;
			if (target == canStats) return;
			if (!target.HasStat(targetStatName)) return;
			if (target.getStatNormalized(targetStatName) >= 1f) return;
			if (canStats.getStat(canStatName) <= 0f) return;
			target.changeStat(targetStatName, pourPerSecond * Time.deltaTime);
		}

		private void TryRefillFrom(Collider other) {
			WaterSource source = other.GetComponentInParent<WaterSource>();
			if (source == null) return;

			canStats.changeStat(canStatName, source.RefillPerSecond * Time.deltaTime);
		}
	}
}
