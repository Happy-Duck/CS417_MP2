using Shared;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Rishi {
	public class PurchaseShelf : MonoBehaviour {
		public UnityEvent<string, int> onPurchase;
		[SerializeField] private GameObject itemPrefab;
		[SerializeField] private Transform displayPoint;
		[SerializeField] private Transform spawnParent;
		[SerializeField] private float restockDelay = 1f;

		[SerializeField] private string costResource = "Dollar";
		[SerializeField] private int costAmount = 0;

		// tries these first (like farm eggs), then falls back to costResource
		[SerializeField] private List<string> altCostResources = new List<string>();
		[SerializeField] private int altCostAmount = 1;

		// empty = "Not enough money! This costs $N"
		[SerializeField] private string brokeHint = "";

		[SerializeField] private string bonusResource = "";
		[SerializeField] private int bonusAmount = 0;

		[SerializeField] AudioClip paySound;
		[SerializeField] AudioClip failSound;

		private XRGrabInteractable stockGrab;
		private StatDisplay stockDisplay;
		private string paidResource;
		private int paidAmount;

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
				if (failSound != null) AudioSource.PlayClipAtPoint(failSound, transform.position);
				ShowBrokeHint();
				return;
			}

			stockGrab.selectEntered.RemoveListener(OnItemGrabbed);
			stockGrab = null;
			if (stockDisplay != null) stockDisplay.SetShopStock(false);
			stockDisplay = null;
			if (bonusResource != "") ResourceManager.Instance.Add(bonusResource, bonusAmount);
			if (paySound != null) AudioSource.PlayClipAtPoint(paySound, transform.position);
			Invoke(nameof(Restock), restockDelay);
			FarmAchievements.Count("Bought " + itemPrefab.name);
			onPurchase.Invoke(paidResource, paidAmount);
		}

		private bool TryPay() {
			paidResource = costResource;
			paidAmount = costAmount;
			if (ResourceManager.Instance != null) {
				foreach (string alt in altCostResources) {
					if (ResourceManager.Instance.TrySpend(alt, altCostAmount)) {
						paidResource = alt;
						paidAmount = altCostAmount;
						return true;
					}
				}
			}

			if (string.IsNullOrEmpty(costResource) || costAmount <= 0) return true;
			if (ResourceManager.Instance == null) return false;
			return ResourceManager.Instance.TrySpend(costResource, costAmount);
		}

		private void ShowBrokeHint() {
			string message = brokeHint;
			if (message == "") {
				if (costResource == "Dollar") {
					message = "Not enough money! This costs $" + costAmount;
				} else {
					message = "Not enough " + costResource + " (costs " + costAmount + ")";
				}
			}
			Popups.Hint("broke" + GetHashCode(), message, Popups.TowardPlayer(transform.position, 0.3f));
		}

		private void OnDestroy() {
			if (stockGrab != null) stockGrab.selectEntered.RemoveListener(OnItemGrabbed);
		}
	}
}
