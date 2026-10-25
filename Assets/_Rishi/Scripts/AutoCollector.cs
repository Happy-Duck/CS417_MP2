using Shared;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Rishi {
	// egg collector, picks up eggs/poop/meat around it
	public class AutoCollector : MonoBehaviour {

		[SerializeField] float radius = 6f;
		[SerializeField] float interval = 3f;

		[SerializeField] string nothingNearbyHint = "Nothing to collect here. Put it in the pen.";
		[SerializeField] string firstCollectHint = "";

		private GridOccupant occupant;
		private bool everCollected = false;

		IEnumerator Start() {
			occupant = GetComponent<GridOccupant>();

			while (true) {
				yield return new WaitForSeconds(interval);
				if (occupant != null && !occupant.IsPlaced) continue;
				Collect();
			}
		}

		private void Collect() {
			bool chickensNearby = false;
			foreach (ChickenBehavior chicken in FindObjectsByType<ChickenBehavior>(FindObjectsSortMode.None)) {
				if (Vector3.Distance(chicken.transform.position, transform.position) <= radius) {
					chickensNearby = true;
					break;
				}
			}

			foreach (Harvestable item in FindObjectsByType<Harvestable>(FindObjectsSortMode.None)) {
				if (Vector3.Distance(item.transform.position, transform.position) > radius) continue;

				XRGrabInteractable grab = item.GetComponent<XRGrabInteractable>();
				if (grab != null && grab.isSelected) continue;

				Vector3 spot = item.transform.position;
				item.Grabbed();
				FarmAchievements.Count("Egg Collected");
				if (!everCollected && firstCollectHint != "") Popups.FirstTime("firstCollect", firstCollectHint, spot);
				everCollected = true;
			}

			if (!chickensNearby) Popups.Hint("nothing" + GetHashCode(), nothingNearbyHint, transform.position, 20f);
		}
	}
}
