using UnityEngine;
namespace Rishi {
	public class HideOnPlay : MonoBehaviour {
		void Start() {
			if (TryGetComponent<Renderer>(out Renderer renderer)) {
				renderer.enabled = false;
			}
		}
	}
}