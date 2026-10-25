using Shared;
using UnityEngine;

namespace Rishi {
	public class Trade : MonoBehaviour {

		[SerializeField] string inputResource;
		[SerializeField] int inputAmount = 1;
		[SerializeField] string outputResource = "Dollar";
		[SerializeField] int outputAmount = 1;

		// empty = "You need N X" (or $N for money)
		[SerializeField] string missingHint = "";

		// spawned things add their own resource (chickens do)
		[SerializeField] GameObject spawnedObject;
		[SerializeField] Transform spawnPosition;
		[SerializeField] AudioClip buySound;
		[SerializeField] AudioClip failSound;


		public void Exchange() {
			if (ResourceManager.Instance == null) return;
			if (!ResourceManager.Instance.TrySpend(inputResource, inputAmount)) {
				if (failSound != null) AudioSource.PlayClipAtPoint(failSound, transform.position);
				string message = missingHint;
				if (message == "") {
					if (inputResource == "Dollar") {
						message = "Not enough money! You need $" + inputAmount;
					} else {
						message = "You need " + inputAmount + " " + inputResource + " for this";
					}
				}
				Popups.Hint("trade" + GetHashCode(), message, Popups.TowardPlayer(transform.position, 0.4f));
				return;
			}
			if (buySound != null) AudioSource.PlayClipAtPoint(buySound, gameObject.transform.position);
			if (spawnedObject != null) {
				Instantiate(spawnedObject, spawnPosition);
				FarmAchievements.Count("Bought " + spawnedObject.name);
			} else {
				ResourceManager.Instance.Add(outputResource, outputAmount);
			}
		}
	}
}
