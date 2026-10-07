using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shared {

	// The shared resources, all resources from every section should be stored here
	// Everyone set your own starting resources from your own scripts
	// Resource names are case sensitive: "Egg" and "egg" are different resources
	// use like this: ResourceManager.Instance.Add("Egg", 5);
	//				  ResourceManager.Instance.TrySpend("Egg", 3);
	public class ResourceManager : MonoBehaviour {
		[SerializeField] private bool logChanges = true;
		private readonly Dictionary<string, int> amounts = new Dictionary<string, int>();

		// Lets any script reach this manager with ResourceManager.Instance
		public static ResourceManager Instance { get; private set; }

		// This event occurs whenever a resource amount changes: (resource name, new amount, change)
		// You would use it like this in your scripts:
		//		void Start() { ResourceManager.Instance.OnResourceChanged += HandleChange; }
		//		void OnDestroy() {
		//			if (ResourceManager.Instance != null) ResourceManager.Instance.OnResourceChanged -= HandleChange;
		//		}
		//		void HandleChange(string resource, int newAmount, int change) {
		//			do something here with that info
		//		}
		public event Action<string, int, int> OnResourceChanged;

		// Read-only view of every resource
		// ResourceManager.Instance.All will give you the dict of resources
		public IReadOnlyDictionary<string, int> All {
			get { return amounts; }
		}

		// Makes sure there's only ever one ResourceManager, and sets Instance so other scripts can find it
		private void Awake() {
			if (Instance != null && Instance != this) {
				Debug.LogWarning("There was a duplicate ResourceManager, it's been destroyed");
				Destroy(gameObject);
				return;
			}
			Instance = this;
		}
		private void OnDestroy() {
			if (Instance == this) Instance = null;
		}

		// returns the amount of a given resource that the player has (0 if they've never had any)
		public int Get(string resource) {
			if (string.IsNullOrEmpty(resource)) return 0;
			if (amounts.TryGetValue(resource, out int value)) return value;
			return 0;
		}

		// Adds an amount of a resource, can also add a negative amount, but it never goes under zero
		public void Add(string resource, int amount) {
			if (string.IsNullOrEmpty(resource) || amount == 0) return;

			int before = Get(resource);
			int after = Mathf.Max(0, before + amount);
			amounts[resource] = after;

			int change = after - before;
			if (change == 0) return;

			if (logChanges) Debug.Log($"ResourceManager: {resource}: {before} -> {after}");
			if (OnResourceChanged != null) OnResourceChanged.Invoke(resource, after, change);
		}

		// tells you if the player has enough of a resource
		public bool CanAfford(string resource, int amount) {
			return amount <= 0 || Get(resource) >= amount;
		}

		// tells you if the player has enough of every resource in a cost list
		// usage: CanAfford(new Dictionary<string, int> { { "Egg", 2 }, { "Flour", 1 } })
		public bool CanAfford(IReadOnlyDictionary<string, int> costs) {
			foreach (KeyValuePair<string, int> cost in costs)
				if (!CanAfford(cost.Key, cost.Value)) return false;
			return true;
		}

		// spends some amount of a resource if they can afford it, otherwise it returns false and changes nothing
		public bool TrySpend(string resource, int amount) {
			if (amount <= 0) return true;
			if (!CanAfford(resource, amount)) return false;
			Add(resource, -amount);
			return true;
		}

		// spends several resources at once if they can afford all of them, otherwise it returns false and changes nothing
		public bool TrySpend(IReadOnlyDictionary<string, int> costs) {
			if (!CanAfford(costs)) return false;
			foreach (KeyValuePair<string, int> cost in costs)
				if (cost.Value > 0) Add(cost.Key, -cost.Value);
			return true;
		}
	}
}