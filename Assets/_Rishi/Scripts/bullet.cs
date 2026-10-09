using UnityEngine;

namespace Rishi {
	public class bullet : MonoBehaviour {

		[SerializeField] float damage = 200;

		void OnTriggerEnter(Collider other) {
			Stats stats = other.GetComponentInParent<Stats>();
			if (stats == null) return;
			if (!stats.HasStat("Health")) return;

			stats.changeStat("Health", -damage);
			Debug.Log("shot " + stats.gameObject.name);
			gameObject.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
			Destroy(gameObject, 1f);
		}
	}
}
