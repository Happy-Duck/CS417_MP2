using UnityEngine;
namespace Rishi {
	[System.Serializable]
	public class Stat {
		public string statName;
		public float statValue;
		public float statRate;
		public float statMin = 0f;
		public float statMax = 100f;

		[System.NonSerialized] public bool wasAtMin;
		[System.NonSerialized] public bool wasAtMax;

		public void Tick(float dt) {
			statValue += statRate * dt;
			Clamp();
		}

		public void Clamp() {
			statValue = Mathf.Clamp(statValue, statMin, statMax);
		}

		public float GetNormalized() {
			if (statMax <= statMin) return 0f;
			return (statValue - statMin) / (statMax - statMin);
		}

		public bool IsAtMin() {
			return statValue <= statMin;
		}

		public bool IsAtMax() {
			return statValue >= statMax;
		}

	}
}
