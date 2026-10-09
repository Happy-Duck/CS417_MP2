using System.Collections.Generic;
using UnityEngine;

namespace Rishi {
	public class Stats : MonoBehaviour {

		public List<Stat> statList = new List<Stat>();

		public event System.Action<string> OnStatReachedMin;
		public event System.Action<string> OnStatReachedMax;

		public event System.Action<string> OnStatLeftMin;
		public event System.Action<string> OnStatLeftMax;

		void Awake() {
			foreach (Stat stat in statList) {
				stat.Clamp();
				stat.wasAtMin = stat.IsAtMin();
				stat.wasAtMax = stat.IsAtMax();
				if (stat.statName != "Health") stat.statRate *= Random.Range(0.9f, 1.1f);
			}
		}

		void Update() {
			foreach (Stat stat in statList) {
				stat.Tick(Time.deltaTime);
				CheckLimits(stat);
			}
		}

		private void CheckLimits(Stat stat) {
			if (stat.IsAtMin()) {
				if (!stat.wasAtMin) {
					stat.wasAtMin = true;
					if (OnStatReachedMin != null) OnStatReachedMin(stat.statName);
				}
			} else {
				if (stat.wasAtMin) {
					stat.wasAtMin = false;
					if (OnStatLeftMin != null) OnStatLeftMin(stat.statName);
				}
			}

			if (stat.IsAtMax()) {
				if (!stat.wasAtMax) {
					stat.wasAtMax = true;
					if (OnStatReachedMax != null) OnStatReachedMax(stat.statName);
				}
			} else {
				if (stat.wasAtMax) {
					stat.wasAtMax = false;
					if (OnStatLeftMax != null) OnStatLeftMax(stat.statName);
				}
			}
		}

		public bool HasStat(string name) {
			foreach (Stat stat in statList) {
				if (stat.statName == name) return true;
			}
			return false;
		}

		private Stat getStatObj(string name) {
			foreach (Stat stat in statList) {
				if (stat.statName == name) {
					return stat;
				}
			}
			Debug.LogWarning("Statted on " + gameObject.name + " has no stat named '" + name + "'");
			return null;
		}

		// 0 to 1
		public float getStatNormalized(string name) {
			Stat stat = getStatObj(name);
			if (stat == null) return 0f;
			return stat.GetNormalized();
		}

		public float getStat(string name) {
			Stat stat = getStatObj(name);
			if (stat != null) return stat.statValue;
			return -1f;
		}

		public float getStatRate(string name) {
			Stat stat = getStatObj(name);
			if (stat != null) return stat.statRate;
			return 0f;
		}

		public void setStat(string name, float val) {
			Stat stat = getStatObj(name);
			if (stat != null) {
				stat.statValue = val;
				stat.Clamp();
			}
		}

		public void setStatRate(string name, float val) {
			Stat stat = getStatObj(name);
			if (stat != null) stat.statRate = val;
		}


		public void changeStat(string name, float diff) {
			Stat stat = getStatObj(name);
			if (stat != null) {
				stat.statValue += diff;
				stat.Clamp();
				// check now, otherwise regen in Tick can move it off the limit before Update sees it
				CheckLimits(stat);
			}
		}

		public void changeStatRate(string name, float diff) {
			Stat stat = getStatObj(name);
			if (stat != null) {
				stat.statRate += diff;
				stat.Clamp();
			}
		}

	}
}