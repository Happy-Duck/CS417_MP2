using Shared;
using System.Collections.Generic;
using UnityEngine;

namespace Rishi {
	// names have to match the AchievementsManager list on the Manager prefab exactly
	public class FarmAchievements : MonoBehaviour {

		[SerializeField] float cardSeconds = 8f;
		[SerializeField] AudioClip achievementSound;

		// for stuff that isnt a resource, other scripts call Count("Chicken Shot") etc
		private static Dictionary<string, int> counts = new Dictionary<string, int>();

		private HashSet<string> done = new HashSet<string>();
		private string[] eggColors = { "White Egg", "Brown Egg", "Blue Egg", "Red Egg", "Golden Egg" };

		public static void Count(string thing) {
			int amount;
			counts.TryGetValue(thing, out amount);
			counts[thing] = amount + 1;
		}

		public static int GetCount(string thing) {
			int amount;
			if (counts.TryGetValue(thing, out amount)) return amount;
			return 0;
		}

		void Awake() {
			counts.Clear();
		}

		void Update() {
			FarmProgression farm = FarmProgression.Instance;
			if (farm == null) return;
			// one card at a time so they dont stack
			if (farm.IsRevealing) return;
			if (Popups.CardBusy) return;

			Check("Golden Goose", farm.Collected("Golden Egg") >= 1);
			Check("Last One There", farm.Collected("Rotten Egg") >= 1);
			Check("Gonna need more baskets", CollectedEggs(farm) >= 50);
			Check("What the flock", Held("Living Chicken") >= 15);
			Check("Omega Fard", farm.Collected("Chicken Poop") >= 50);
			Check("Easter Bunny", GetCount("Egg Collected") >= 1);
			Check("Gas Gas Gas", GetCount("Auto Remaining Fuel") >= 1);
			Check("Well Well Well", GetCount("Auto Remaining Water") >= 1);
			Check("What the glock?", GetCount("Bought McGlock") >= 1);
			Check("Bang Bang Chicken", GetCount("Chicken Shot") >= 1);
			Check("Who bought that?", farm.Spent("Rotten Egg") + farm.Spent("Rotten Chicken") >= 1);
			Check("Breeder", GetCount("Chicken Sold") >= 10);
			Check("All the Eggs in the Rainbow", GotEveryColor(farm));
			Check("Brooder", GetCount("Bought Chicken_NPC") >= 10);
		}

		private void Check(string achievementName, bool met) {
			if (!met) return;
			if (done.Contains(achievementName)) return;
			if (Popups.CardBusy) return;

			done.Add(achievementName);

			string description = "";
			AchievementsManager manager = AchievementsManager.Instance;
			if (manager != null) {
				AchievementsManager.Achievement achievement;
				if (manager.TryGetAchievementByName(achievementName, out achievement)) description = achievement.Description;
				manager.CompleteAchievement(achievementName);
			}

			Popups.Card("ACHIEVEMENT UNLOCKED!\n" + achievementName + "\n" + description, cardSeconds);
			if (achievementSound != null && Camera.main != null) AudioSource.PlayClipAtPoint(achievementSound, Camera.main.transform.position);
		}

		private int Held(string resource) {
			if (ResourceManager.Instance == null) return 0;
			return ResourceManager.Instance.Get(resource);
		}

		// picked up at the farm, rotten ones count too
		private int CollectedEggs(FarmProgression farm) {
			int total = farm.Collected("Rotten Egg");
			foreach (string color in eggColors) {
				total += farm.Collected(color);
			}
			return total;
		}

		private bool GotEveryColor(FarmProgression farm) {
			foreach (string color in eggColors) {
				if (farm.Collected(color) < 1) return false;
			}
			return true;
		}
	}
}
