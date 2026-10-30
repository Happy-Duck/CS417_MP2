using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Shared;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

namespace Kyle
{
    //[RequireComponent(typeof(XRBaseInteractable))]
    public abstract class StoreItem : MonoBehaviour
    {
        [Header("Purchase Settings")]
        public string costResource = "Dollar";
        public int cost;
        public AudioClip successSound;
        [SerializeField] protected ErrorCatchingHint errorCatchingHint;

        [Header("Unlock Requirement")]
        public string requiredTrackerKey = "";
        public int requiredTrackerAmount = 0;
        public string progressMessage = "Craft 5 items!";

        public string prerequisiteTrackerKey = "";
        public int prerequisiteAmount = 0;
        public string waitlistMessage = "Unlock previous items first!";

        public string unlockGrantedKey = "";

        [Header("Visuals and UI")]
        public GameObject lockModel;
        public Material lockedBlackMaterial;
        public GameObject floatingCanvas;
        public TextMeshProUGUI statusText;
        public Slider progressSlider;

        //public AudioClip errorSound;

        protected XRBaseInteractable interactable;
        private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
        protected bool isUnlocked = false;

        private void Awake()
        {
            interactable = GetComponent<XRBaseInteractable>();

            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in renderers)
            {
                originalMaterials[r] = r.sharedMaterials;
            }
        }
        private void Start()
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnResourceChanged += HandleResourceChanged;
            }

            CheckLockState();
        }
        private void OnDestroy()
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnResourceChanged -= HandleResourceChanged;
            }
        }

        protected virtual bool IsSoldOut() { return false; }

        protected virtual bool IsRelevantResource(string key) { return false; }

        private void HandleResourceChanged(string changedKey, int newAmount, int change)
        {
            if (changedKey == requiredTrackerKey || changedKey == prerequisiteTrackerKey || changedKey == unlockGrantedKey || IsRelevantResource(changedKey))
            {
                CheckLockState();
            }
        }
        protected void CheckLockState()
        {
            if (IsSoldOut())
            {
                UpdateMaterials(true);
                //interactable.enabled = false;

                if (floatingCanvas != null) floatingCanvas.SetActive(true);
                if (statusText != null) statusText.text = "Sold Out!";
                if (progressSlider != null) progressSlider.gameObject.SetActive(false);
                return;
            }

            bool prereqMet = string.IsNullOrEmpty(prerequisiteTrackerKey) ||
                             ResourceManager.Instance.Get(prerequisiteTrackerKey) >= prerequisiteAmount;

            int currentAmount = string.IsNullOrEmpty(requiredTrackerKey) ? 0 : ResourceManager.Instance.Get(requiredTrackerKey);
            isUnlocked = string.IsNullOrEmpty(requiredTrackerKey) || currentAmount >= requiredTrackerAmount;

            if (isUnlocked)
            {
                UpdateMaterials(true);
                if (floatingCanvas != null) floatingCanvas.SetActive(false);
                //interactable.enabled = true;
                if (lockModel != null) lockModel.SetActive(false);
            }
            else if (prereqMet)
            {
                UpdateMaterials(false);
                //interactable.enabled = false;

                if (floatingCanvas != null) floatingCanvas.SetActive(true);
                if (lockModel != null) lockModel.SetActive(true);

                if (statusText != null)
                {
                    if (progressMessage.Contains("{0}") || progressMessage.Contains("{1}"))
                    {
                        statusText.text = string.Format(progressMessage, currentAmount, requiredTrackerAmount);
                    }
                    else
                    {
                        statusText.text = progressMessage;
                    }
                }

                if (progressSlider != null)
                {
                    progressSlider.gameObject.SetActive(true);
                    progressSlider.maxValue = requiredTrackerAmount;
                    progressSlider.value = currentAmount;
                }
            }
            else
            {
                UpdateMaterials(false);
                //interactable.enabled = false;

                if (floatingCanvas != null) floatingCanvas.SetActive(true);
                if (lockModel != null) lockModel.SetActive(true);

                if (statusText != null) statusText.text = waitlistMessage;
                if (progressSlider != null) progressSlider.gameObject.SetActive(false);
            }
        }

        private void UpdateMaterials(bool restoreOriginal)
        {
            foreach (KeyValuePair<Renderer, Material[]> kvp in originalMaterials)
            {
                Renderer r = kvp.Key;
                Material[] originalMats = kvp.Value;

                if (restoreOriginal)
                {
                    r.sharedMaterials = originalMats;
                }
                else
                {
                    Material[] blackMats = new Material[originalMats.Length];
                    for (int i = 0; i < blackMats.Length; i++)
                    {
                        blackMats[i] = lockedBlackMaterial;
                    }
                    r.sharedMaterials = blackMats;
                }
            }
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
            args.manager.SelectCancel(args.interactorObject, args.interactableObject);

            if (IsSoldOut()) return;

            if (!isUnlocked)
            {
                errorCatchingHint.message = "Item is still locked!";
                errorCatchingHint.ShowError();

                StartCoroutine(ShakeEffect());
                return;
            }

            if (ResourceManager.Instance.CanAfford(costResource, cost))
            {
                ResourceManager.Instance.TrySpend(costResource, cost);

                if (!string.IsNullOrEmpty(unlockGrantedKey))
                {
                    ResourceManager.Instance.Add(unlockGrantedKey, 1);
                }

                OnPurchaseSuccess();

                if (successSound != null)
                {
                    AudioSource.PlayClipAtPoint(successSound, transform.position);
                }
            }
            else
            {
                errorCatchingHint.message = "You don't have enough money to buy this item!";
                errorCatchingHint.ShowError();
            }
        }

        private System.Collections.IEnumerator ShakeEffect()
        {
            Vector3 originalPos = lockModel.transform.localPosition;
            float duration = 0.2f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                lockModel.transform.localPosition = originalPos + UnityEngine.Random.insideUnitSphere * 0.05f;
                elapsed += Time.deltaTime;
                yield return null;
            }

            lockModel.transform.localPosition = originalPos;
        }

        protected abstract void OnPurchaseSuccess();
    }
}