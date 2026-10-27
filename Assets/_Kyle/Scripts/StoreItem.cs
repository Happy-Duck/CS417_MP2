using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Shared;

namespace Kyle
{
    [RequireComponent(typeof(XRBaseInteractable))]
    public class StoreItem : MonoBehaviour
    {
        public string itemResource;
        public string costResource = "Dollar";
        public int cost = 2;

        public string requiredTrackerKey = "";
        public int requiredTrackerAmount = 0;

        public GameObject visualObject;
        public AudioClip successSound;
        //public AudioClip errorSound;

        private XRBaseInteractable interactable;

        [SerializeField] ErrorCatchingHint errorCatchingHint;

        private void Awake()
        {
            interactable = GetComponent<XRBaseInteractable>();
        }
        private void Start()
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnResourceChanged += HandleResourceChanged;
            }

            CheckVisibility();
        }
        private void OnDestroy()
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnResourceChanged -= HandleResourceChanged;
            }
        }

        private void HandleResourceChanged(string changedKey, int newAmount, int change)
        {
            if (changedKey == requiredTrackerKey)
            {
                CheckVisibility();
            }
        }
        private void CheckVisibility()
        {
            if (string.IsNullOrEmpty(requiredTrackerKey)) return;

            bool isUnlocked = ResourceManager.Instance.Get(requiredTrackerKey) >= requiredTrackerAmount;

            if (visualObject != null)
            {
                visualObject.SetActive(isUnlocked);
            }

            interactable.enabled = isUnlocked;
        }

        private void OnEnable()
        {
            interactable.selectEntered.AddListener(TryPurchase);
        }

        private void OnDisable()
        {
            interactable.selectEntered.RemoveListener(TryPurchase);
        }

        private void TryPurchase(SelectEnterEventArgs args)
        {
            if (!string.IsNullOrEmpty(requiredTrackerKey) && ResourceManager.Instance.Get(requiredTrackerKey) < requiredTrackerAmount)
            {
                errorCatchingHint.message = $"Locked! Must craft {requiredTrackerAmount} {itemResource} first.";
                errorCatchingHint.ShowError();
                args.manager.SelectCancel(args.interactorObject, args.interactableObject);
                return;
            }

            if (ResourceManager.Instance.CanAfford(costResource, cost))
            {
                ResourceManager.Instance.TrySpend(costResource, cost);
                ResourceManager.Instance.Add(itemResource, 1);
                InventoryManager.Instance?.UpdateNums();

                if (successSound != null)
                {
                    AudioSource.PlayClipAtPoint(successSound, transform.position);
                }
            }
            else
            {
                //if (errorSound != null)
                //{
                //    AudioSource.PlayClipAtPoint(errorSound, transform.position);
                //}

                errorCatchingHint.ShowError();
            }
        }
    }
}