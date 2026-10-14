using Shared;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Rishi {
	// trigger, needs a kinematic rigidbody
	public class ChickenMarket : MonoBehaviour {

		[SerializeField] string payResource = "Coin";
		[SerializeField] int payAmount = 30;

		[SerializeField] AudioClip buySound;

		private List<ChickenBehavior> onTheirWay = new List<ChickenBehavior>();
		private Collider sellArea;

		void Start() {
			sellArea = GetComponent<Collider>();
		}

		public void SendChicken() {
			ChickenBehavior chicken = FindClosestUnsoldChicken();
			if (chicken == null) {
				Debug.Log("ChickenMarket: no chicken to sell");
				return;
			}

			NavMeshAgent agent = chicken.GetComponent<NavMeshAgent>();
			if (agent == null) return;

			ChickenWander wander = chicken.GetComponent<ChickenWander>();
			if (wander != null) wander.StopWandering();

			onTheirWay.Add(chicken);
			// pivot is outside the trigger box
			agent.SetDestination(sellArea.bounds.center);
		}

		private ChickenBehavior FindClosestUnsoldChicken() {
			ChickenBehavior closest = null;
			float closestDistance = Mathf.Infinity;

			foreach (ChickenBehavior chicken in FindObjectsByType<ChickenBehavior>(FindObjectsSortMode.None)) {
				if (onTheirWay.Contains(chicken)) continue;

				float distance = Vector3.Distance(chicken.transform.position, transform.position);
				if (distance < closestDistance) {
					closestDistance = distance;
					closest = chicken;
				}
			}
			return closest;
		}

		void OnTriggerStay(Collider other) {
			ChickenBehavior chicken = other.GetComponentInParent<ChickenBehavior>();
			if (chicken == null) return;

			if (!onTheirWay.Contains(chicken)) return;
			onTheirWay.Remove(chicken);

			if (ResourceManager.Instance != null) ResourceManager.Instance.Add(payResource, payAmount);
			AudioSource.PlayClipAtPoint(buySound, gameObject.transform.position);
			Destroy(chicken.gameObject);
		}
	}
}
