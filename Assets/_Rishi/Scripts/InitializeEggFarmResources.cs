using Shared;
using UnityEngine;

namespace Rishi {
	public class InitializeEggFarmResources : MonoBehaviour {
		// Start is called once before the first execution of Update after the MonoBehaviour is created
		void Start() {
			ResourceManager.Instance.Add("Coin", 1000);
		}

		// Update is called once per frame
		void Update() {

		}
	}
}
