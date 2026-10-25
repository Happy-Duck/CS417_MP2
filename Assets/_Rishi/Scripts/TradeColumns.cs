using System.Collections.Generic;
using UnityEngine;

namespace Rishi {
	// splits the visible trade buttons evenly between the two columns
	// runs whenever the number of visible buttons changes, so it keeps up as FarmProgression unlocks more
	public class TradeColumns : MonoBehaviour {

		[SerializeField] Transform leftColumn;
		[SerializeField] Transform rightColumn;

		private List<Transform> buttons = new List<Transform>();
		private int lastVisible = -1;

		void Start() {
			// keeps the order from the editor, left column first then right
			foreach (Transform child in leftColumn) {
				buttons.Add(child);
			}
			foreach (Transform child in rightColumn) {
				buttons.Add(child);
			}
		}

		void LateUpdate() {
			int visible = 0;
			foreach (Transform button in buttons) {
				if (button != null && button.gameObject.activeSelf) visible++;
			}
			if (visible == lastVisible) return;
			lastVisible = visible;
			Rebalance(visible);
		}

		private void Rebalance(int visible) {
			int leftCount = (visible + 1) / 2;
			int placed = 0;
			int leftIndex = 0;
			int rightIndex = 0;

			foreach (Transform button in buttons) {
				// hidden ones just stay wherever they are
				if (button == null || !button.gameObject.activeSelf) continue;

				if (placed < leftCount) {
					if (button.parent != leftColumn) button.SetParent(leftColumn, false);
					button.SetSiblingIndex(leftIndex);
					leftIndex++;
				} else {
					if (button.parent != rightColumn) button.SetParent(rightColumn, false);
					button.SetSiblingIndex(rightIndex);
					rightIndex++;
				}
				placed++;
			}
		}
	}
}
