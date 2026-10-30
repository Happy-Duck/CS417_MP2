using UnityEngine.Events;
using Shared;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine;

namespace Kyle
{
    [RequireComponent(typeof(XRSimpleInteractable))]
    public class UpgradeStoreItem : StoreItem
    {
        public string upgradeKey;
        public UnityEvent onUpgradePurchased;

        protected override bool IsRelevantResource(string key)
        {
            return key == upgradeKey;
        }
        protected override bool IsSoldOut()
        {
            return ResourceManager.Instance != null && ResourceManager.Instance.Get(upgradeKey) >= 1;
        }

        protected override void OnPurchaseSuccess()
        {
            ResourceManager.Instance.Add(upgradeKey, 1);
            onUpgradePurchased?.Invoke();

            CheckLockState();
        }
    }
}