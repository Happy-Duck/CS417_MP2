using UnityEngine;

namespace Rishi {
	public class RefillItem : MonoBehaviour {

		[SerializeField] string statName = "Feed";
		[SerializeField] float amount = 50f;

		[SerializeField] AudioClip refill;

		private bool used = false;

		public string StatName {
			get { return statName; }
		}

		public float Amount {
			get { return amount; }
		}

		public bool IsUsed {
			get { return used; }
		}

		public void Consume() {
			if (used) return;
			AudioSource.PlayClipAtPoint(refill, transform.position);
			used = true;
			Destroy(gameObject);
		}
	}
}
