using UnityEngine;

namespace Rishi {

	[RequireComponent(typeof(Renderer))]
	public class UVScroller : MonoBehaviour {

		[SerializeField] Vector2 scrollPerSecond = new Vector2(0f, 1f);

		private Material material;
		private Vector2 offset;

		void Start() {
			material = GetComponent<Renderer>().material;
			offset = material.mainTextureOffset;
		}

		void Update() {
			offset += scrollPerSecond * Time.deltaTime;
			offset.x = offset.x % 1f;
			offset.y = offset.y % 1f;

			material.mainTextureOffset = offset;
		}

		void OnDestroy() {
			if (material != null) Destroy(material);
		}
	}
}
