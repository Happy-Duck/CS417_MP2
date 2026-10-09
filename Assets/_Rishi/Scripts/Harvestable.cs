using Shared;
using UnityEngine;

namespace Rishi {
	public class Harvestable : MonoBehaviour {

		[SerializeField] string resourceName;
		[SerializeField] int amount = 1;

		[SerializeField] AudioClip pickUpSound;

		private bool collected = false;

		public void SetResource(string newResourceName) {
			resourceName = newResourceName;
		}

		public void Grabbed() {
			// both hands can grab in the same frame
			if (collected) return;

			if (string.IsNullOrEmpty(resourceName)) {
				Debug.LogWarning("Harvestable on " + gameObject.name + " has no resourceName");
				return;
			}
			if (ResourceManager.Instance == null) {
				Debug.LogWarning("Harvestable on " + gameObject.name + " can't find a ResourceManager");
				return;
			}
			AudioSource.PlayClipAtPoint(pickUpSound, gameObject.transform.position);
			collected = true;
			ResourceManager.Instance.Add(resourceName, amount);
			Destroy(gameObject);
		}
	}
}
