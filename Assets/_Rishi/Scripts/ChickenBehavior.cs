using Shared;
using System.Collections.Generic;
using UnityEngine;

namespace Rishi {
	[System.Serializable]
	public class EggVariant {
		public string resourceName;
		public Material material;
		public float weight = 1f;
	}

	[RequireComponent(typeof(AudioSource))]
	public class ChickenBehavior : MonoBehaviour {
		Stats stats;

		[SerializeField] GameObject egg;
		[SerializeField] GameObject poop;
		[SerializeField] GameObject rawChickenMeat;
		[SerializeField] List<EggVariant> eggVariants = new List<EggVariant>();

		AudioSource chickenAudioSource;
		[SerializeField] AudioClip poopSound;
		[SerializeField] AudioClip laySound;
		[SerializeField] AudioClip dieSound;

		// must be bigger than health regen
		private const float problemPenalty = 2f;

		private float normalHealthRate;
		private float normalEggRate;

		// Start is called once before the first execution of Update after the MonoBehaviour is created
		void Start() {
			stats = GetComponent<Stats>();
			normalHealthRate = stats.getStatRate("Health");
			normalEggRate = stats.getStatRate("EggProgress");
			if (ResourceManager.Instance != null) ResourceManager.Instance.Add("Living Chicken", 1);

			chickenAudioSource = GetComponent<AudioSource>();

			stats.OnStatReachedMax += HandleMax;
			stats.OnStatReachedMin += HandleMin;
			stats.OnStatLeftMax += HandleLeftMax;
			stats.OnStatLeftMin += HandleLeftMin;
		}

		private void HandleMax(string stat) {
			switch (stat) {
				case "EggProgress":
					LayEgg();
					stats.setStat("EggProgress", 0);
					break;
				case "Poopiness":
					Instantiate(poop, transform.position, Quaternion.identity, GetAreaRoot());
					stats.setStat("Poopiness", 0);
					chickenAudioSource.PlayOneShot(poopSound);
					break;
				case "Hunger":
				case "Thirst":
					StartProblem();
					break;
				case "Health":
					break;
				default:
					Debug.Log("Stat '" + stat + "' does not have a handled max");
					break;
			}
		}

		private void HandleMin(string stat) {
			switch (stat) {
				case "Warmth":
					StartProblem();
					break;
				case "Health":
					AudioSource.PlayClipAtPoint(dieSound, gameObject.transform.position);
					Instantiate(rawChickenMeat, transform.position, Quaternion.identity, GetAreaRoot());
					Destroy(gameObject);
					break;
				default:
					Debug.Log("Stat '" + stat + "' does not have a handled min");
					break;
			}
		}

		private void HandleLeftMax(string stat) {
			switch (stat) {
				case "Hunger":
				case "Thirst":
					EndProblem();
					break;
			}
		}

		private void HandleLeftMin(string stat) {
			switch (stat) {
				case "Warmth":
					EndProblem();
					break;
			}
		}

		private void StartProblem() {
			stats.changeStatRate("Health", -problemPenalty);
			stats.setStatRate("EggProgress", 0f);
		}

		private void EndProblem() {
			stats.changeStatRate("Health", problemPenalty);

			// only when no problems are left
			if (stats.getStatRate("Health") >= normalHealthRate) {
				stats.setStatRate("EggProgress", normalEggRate);
			}
		}

		private void LayEgg() {
			GameObject newEgg = Instantiate(egg, transform.position, Quaternion.identity, GetAreaRoot());
			chickenAudioSource.PlayOneShot(laySound);
			EggVariant variant = PickEggVariant();
			if (variant == null) return;

			Harvestable harvestable = newEgg.GetComponent<Harvestable>();
			if (harvestable != null) harvestable.SetResource(variant.resourceName);

			Renderer eggRenderer = newEgg.GetComponentInChildren<Renderer>();
			if (eggRenderer != null && variant.material != null) eggRenderer.sharedMaterial = variant.material;
		}

		private EggVariant PickEggVariant() {
			float totalWeight = 0f;
			foreach (EggVariant variant in eggVariants) {
				totalWeight += variant.weight;
			}
			if (totalWeight <= 0f) return null;

			float roll = Random.Range(0f, totalWeight);
			foreach (EggVariant variant in eggVariants) {
				if (roll < variant.weight) return variant;
				roll -= variant.weight;
			}
			// float rounding fallback
			return eggVariants[eggVariants.Count - 1];
		}

		private Transform GetAreaRoot() {
			if (transform.root == transform) return null;
			return transform.root;
		}

		private void OnDestroy() {
			if (ResourceManager.Instance != null) ResourceManager.Instance.Add("Living Chicken", -1);
			if (stats == null) return;
			stats.OnStatReachedMax -= HandleMax;
			stats.OnStatReachedMin -= HandleMin;
			stats.OnStatLeftMax -= HandleLeftMax;
			stats.OnStatLeftMin -= HandleLeftMin;
		}
	}
}
