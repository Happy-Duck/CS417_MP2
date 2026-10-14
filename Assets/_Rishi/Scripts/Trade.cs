using Shared;
using UnityEngine;

namespace Rishi {
	public class Trade : MonoBehaviour {

		[SerializeField] string inputResource;
		[SerializeField] int inputAmount = 1;
		[SerializeField] string outputResource = "Coin";
		[SerializeField] int outputAmount = 1;

		// spawned things add their own resource (chickens do)
		[SerializeField] GameObject spawnedObject;
		[SerializeField] Transform spawnPosition;
		[SerializeField] AudioClip buySound;


		public void Exchange() {
			if (ResourceManager.Instance == null) return;
			if (!ResourceManager.Instance.TrySpend(inputResource, inputAmount)) return;
			AudioSource.PlayClipAtPoint(buySound, gameObject.transform.position);
			if (spawnedObject != null) {
				Instantiate(spawnedObject, spawnPosition);
			} else {
				ResourceManager.Instance.Add(outputResource, outputAmount);
			}
		}
	}
}
