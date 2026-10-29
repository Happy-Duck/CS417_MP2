using Shared;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Rishi {
	[System.Serializable]
	public class FarmGoal {
		public string description;
		public List<string> resources = new List<string>();
		public int amount = 1;
		// true = how many you have right now, false = how many you've picked up at the farm total
		public bool held = false;

		public int Progress(FarmProgression farm) {
			int total = 0;
			foreach (string resource in resources) {
				if (held) {
					if (ResourceManager.Instance != null) total += ResourceManager.Instance.Get(resource);
				} else {
					total += farm.Collected(resource);
				}
			}
			return total;
		}

		public bool IsMet(FarmProgression farm) {
			return Progress(farm) >= amount;
		}
	}

	[System.Serializable]
	public class FarmUnlock {
		public FarmGoal goal;
		public List<string> needsTurnedOn = new List<string>();
		public List<GameObject> unlocks = new List<GameObject>();
		[TextArea(3, 6)] public string revealText;
	}

	public class FarmProgression : MonoBehaviour {
		public static FarmProgression Instance { get; private set; }
		public static event System.Action<string> NeedTurnedOn;

		[SerializeField] List<FarmUnlock> steps = new List<FarmUnlock>();
		[SerializeField] GameObject nextGoalBoard;
		[SerializeField] TMP_Text nextGoalText;
		[SerializeField] float revealDelay = 3f;
		[SerializeField] float cardSeconds = 10f;
		[SerializeField] AudioClip unlockSound;

		// shown on the board before the first goal is done, empty = board stays hidden till then
		[TextArea(2, 4)]
		[SerializeField] string introText = "";
		// locked shelves get a copy of their price label with this on it, empty = no signs
		[SerializeField] string lockedShelfText = "Restocking soon!";
		// gets marked done in the shared ProgressionManager once every step is finished
		[SerializeField] string areaName = "Chicken Farm";

		// only stuff picked up at the farm (Harvestable reports it), so the waffle house's starting eggs dont count
		private Dictionary<string, int> collected = new Dictionary<string, int>();
		private Dictionary<string, int> spent = new Dictionary<string, int>();
		private HashSet<string> needsOn = new HashSet<string>();
		private Dictionary<GameObject, GameObject> lockedSigns = new Dictionary<GameObject, GameObject>();
		private int current = 0;
		private bool revealing = false;

		public bool IsRevealing {
			get { return revealing; }
		}

		void Awake() {
			Instance = this;
			foreach (FarmUnlock step in steps) {
				foreach (GameObject thing in step.unlocks) {
					if (thing == null) continue;
					MakeLockedSign(thing);
					thing.SetActive(false);
				}
			}
			if (nextGoalBoard != null && introText == "") nextGoalBoard.SetActive(false);
			UpdateBoard();
		}

		// only for shelves, copies the shelf's price label so the depot doesnt just look empty
		private void MakeLockedSign(GameObject thing) {
			if (lockedShelfText == "") return;
			if (thing.GetComponent<PurchaseShelf>() == null && thing.GetComponent<GridPurchaseShelf>() == null) return;

			TMP_Text label = thing.GetComponentInChildren<TMP_Text>(true);
			if (label == null) return;
			Canvas labelCanvas = label.GetComponentInParent<Canvas>(true);
			if (labelCanvas == null) return;

			Transform original = labelCanvas.transform;
			GameObject sign = Instantiate(labelCanvas.gameObject);
			sign.name = thing.name + " LockedSign";
			sign.transform.position = original.position;
			sign.transform.rotation = original.rotation;
			sign.transform.localScale = original.lossyScale;
			sign.transform.SetParent(thing.transform.parent, true);
			sign.GetComponentInChildren<TMP_Text>(true).text = lockedShelfText;
			lockedSigns[thing] = sign;
		}

		void Start() {
			if (ResourceManager.Instance != null) ResourceManager.Instance.OnResourceChanged += Tally;
		}

		void OnDestroy() {
			if (ResourceManager.Instance != null) ResourceManager.Instance.OnResourceChanged -= Tally;
			if (Instance == this) Instance = null;
		}

		private void Tally(string resource, int newAmount, int change) {
			if (change < 0) spent[resource] = Spent(resource) - change;
		}

		public void AddCollected(string resource, int amount) {
			collected[resource] = Collected(resource) + amount;
		}

		public int Collected(string resource) {
			int amount;
			if (collected.TryGetValue(resource, out amount)) return amount;
			return 0;
		}

		public int Spent(string resource) {
			int amount;
			if (spent.TryGetValue(resource, out amount)) return amount;
			return 0;
		}

		public bool IsNeedOn(string need) {
			return needsOn.Contains(need);
		}

		void Update() {
			if (revealing) return;
			if (current >= steps.Count) return;

			// update first so the board actually shows 6 / 6 before it moves on
			UpdateBoard();

			// waits for any achievement card to go away first
			if (steps[current].goal.IsMet(this) && !Popups.CardBusy) {
				StartCoroutine(Reveal(steps[current]));
			}
		}

		private IEnumerator Reveal(FarmUnlock step) {
			revealing = true;
			Popups.Card(step.revealText, cardSeconds);
			if (unlockSound != null && Camera.main != null) AudioSource.PlayClipAtPoint(unlockSound, Camera.main.transform.position);

			// card goes up first, then the stuff shows up
			yield return new WaitForSeconds(revealDelay);

			foreach (string need in step.needsTurnedOn) {
				needsOn.Add(need);
				if (NeedTurnedOn != null) NeedTurnedOn(need);
			}
			foreach (GameObject thing in step.unlocks) {
				if (thing == null) continue;
				thing.SetActive(true);
				if (lockedSigns.ContainsKey(thing)) {
					Destroy(lockedSigns[thing]);
					lockedSigns.Remove(thing);
				}
			}
			

			current++;
			if (nextGoalBoard != null) nextGoalBoard.SetActive(true);
			UpdateBoard();
			if (current >= steps.Count && areaName != "") ProgressionManager.CompleteArea(areaName);
			revealing = false;
		}

		private void UpdateBoard() {
			if (nextGoalText == null) return;
			if (current == 0 && introText != "") {
				nextGoalText.text = introText;
				return;
			}
			if (current >= steps.Count) {
				nextGoalText.text = "All done!\nThe farm basically runs itself now";
				return;
			}
			FarmGoal goal = steps[current].goal;
			int progress = Mathf.Min(goal.Progress(this), goal.amount);
			nextGoalText.text = goal.description + "\n" + progress + " / " + goal.amount;
		}
	}
}
