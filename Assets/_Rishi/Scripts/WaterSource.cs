using UnityEngine;

namespace Rishi {
	// basically just for the well
	public class WaterSource : MonoBehaviour {

		[SerializeField] float refillPerSecond = 40f;

		public float RefillPerSecond {
			get { return refillPerSecond; }
		}
	}
}
