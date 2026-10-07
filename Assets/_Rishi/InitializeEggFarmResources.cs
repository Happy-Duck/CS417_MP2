using Shared;
using UnityEngine;

public class InitializeEggFarmResources : MonoBehaviour {
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start() {
		ResourceManager.Instance.Add("Coin", 50);
	}

	// Update is called once per frame
	void Update() {

	}
}
