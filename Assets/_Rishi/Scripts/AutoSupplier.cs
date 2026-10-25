using Shared;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rishi {
	// feed hopper, water pump, fuel hopper
	public class AutoSupplier : MonoBehaviour {

		[SerializeField] string targetStat = "Remaining Food";
		// spends 1 of the first one you have, leave empty for free
		[SerializeField] List<string> supplyResources = new List<string>();
		[SerializeField] float amountPerSupply = 25f;
		[SerializeField] float radius = 3f;
		[SerializeField] float interval = 2f;
		[SerializeField] float refillBelow = 0.5f;

		[SerializeField] string emptyHint = "Out of supplies!";
		[SerializeField] string nothingNearbyHint = "Nothing nearby to refill. Move it closer.";
		[SerializeField] string firstRefillHint = "";

		[SerializeField] AudioClip refillSound;
		// quieter since these go off a lot
		[Range(0f, 1f)]
		[SerializeField] float refillVolume = 0.4f;

		private GridOccupant occupant;

		IEnumerator Start() {
			occupant = GetComponent<GridOccupant>();

			while (true) {
				yield return new WaitForSeconds(interval);
				if (occupant != null && !occupant.IsPlaced) continue;
				Supply();
			}
		}

		private void Supply() {
			bool foundAny = false;

			foreach (Stats target in FindObjectsByType<Stats>(FindObjectsSortMode.None)) {
				if (!target.HasStat(targetStat)) continue;
				if (Vector3.Distance(target.transform.position, transform.position) > radius) continue;

				// skip ones still on a shelf or in your hand
				GridOccupant targetOccupant = target.GetComponentInParent<GridOccupant>();
				if (targetOccupant != null && !targetOccupant.IsPlaced) continue;

				foundAny = true;
				if (target.getStatNormalized(targetStat) >= refillBelow) continue;

				if (!TakeSupply()) {
					Popups.Hint("empty" + GetHashCode(), emptyHint, transform.position, 20f);
					return;
				}
				target.changeStat(targetStat, amountPerSupply);
				FarmAchievements.Count("Auto " + targetStat);
				if (refillSound != null) AudioSource.PlayClipAtPoint(refillSound, target.transform.position, refillVolume);
				if (firstRefillHint != "") Popups.FirstTime("first" + targetStat, firstRefillHint, target.transform.position);
			}

			if (!foundAny) Popups.Hint("nothing" + GetHashCode(), nothingNearbyHint, transform.position, 20f);
		}

		private bool TakeSupply() {
			if (supplyResources.Count == 0) return true;
			if (ResourceManager.Instance == null) return false;
			foreach (string resource in supplyResources) {
				if (ResourceManager.Instance.TrySpend(resource, 1)) return true;
			}
			return false;
		}
	}
}
