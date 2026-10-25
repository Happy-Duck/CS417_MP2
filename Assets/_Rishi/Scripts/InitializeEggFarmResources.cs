using Shared;
using System.Collections.Generic;
using UnityEngine;

namespace Rishi {
	public class InitializeEggFarmResources : MonoBehaviour {
		[SerializeField] int startingDollars = 1000;

		// for testing teammates stuff in my scene (HarvestedCorn, Leftover, Gasoline...), leave empty otherwise
		[SerializeField] List<string> testResources = new List<string>();
		[SerializeField] int testAmount = 20;

		// Start is called once before the first execution of Update after the MonoBehaviour is created
		void Start() {
			ResourceManager.Instance.Add("Dollar", startingDollars);
			foreach (string resource in testResources) {
				ResourceManager.Instance.Add(resource, testAmount);
			}
		}

		// Update is called once per frame
		void Update() {

		}
	}
}
