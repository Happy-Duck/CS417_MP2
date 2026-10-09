using Shared;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Rishi {
	public class PurchaseShelf : MonoBehaviour {
		[SerializeField] private GameObject itemPrefab;
		[SerializeField] private Transform displayPoint;
		[SerializeField] private Transform spawnParent;
		[SerializeField] private float restockDelay = 1f;

		[SerializeField] private string costResource = "Coin";
		[SerializeField] private int costAmount = 0;

		[SerializeField] private string bonusResource = "";
		[SerializeField] private int bonusAmount = 0;

		[SerializeField] AudioClip paySound;

		private XRGrabInteractable stockGrab;
		private StatDisplay stockDisplay;

		private void Start() {
			Restock();
		}

		private void Restock() {
			if (itemPrefab == null) {
				Debug.LogError("PurchaseShelf: no item prefab assigned", this);
				return;
			}

			Transform point = displayPoint;
			if (point == null) point = transform;

			GameObject item = Instantiate(itemPrefab, point.position, point.rotation, spawnParent);

			stockGrab = item.GetComponent<XRGrabInteractable>();
			if (stockGrab == null) {
				Debug.LogError("PurchaseShelf: " + itemPrefab.name + " needs an XRGrabInteractable", this);
				return;
			}
			stockGrab.selectEntered.AddListener(OnItemGrabbed);

			stockDisplay = item.GetComponent<StatDisplay>();
			if (stockDisplay != null) stockDisplay.SetShopStock(true);
		}

		private void OnItemGrabbed(SelectEnterEventArgs args) {
			if (!TryPay()) {
				args.manager.CancelInteractableSelection(args.interactableObject);
				return;
			}

			stockGrab.selectEntered.RemoveListener(OnItemGrabbed);
			stockGrab = null;
			if (stockDisplay != null) stockDisplay.SetShopStock(false);
			stockDisplay = null;
			if (bonusResource != "") ResourceManager.Instance.Add(bonusResource, bonusAmount);
			AudioSource.PlayClipAtPoint(paySound, transform.position);
			Invoke(nameof(Restock), restockDelay);
		}

		private bool TryPay() {
			if (string.IsNullOrEmpty(costResource) || costAmount <= 0) return true;
			if (ResourceManager.Instance == null) return false;
			return ResourceManager.Instance.TrySpend(costResource, costAmount);
		}

		private void OnDestroy() {
			if (stockGrab != null) stockGrab.selectEntered.RemoveListener(OnItemGrabbed);
		}
	}
}
