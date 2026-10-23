using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using Shared;

namespace Kyle
{
    [RequireComponent(typeof(XRGrabInteractable))]
    public class InventoryItem : MonoBehaviour
    {
        private string resourceName;
        private string resourceKey;
        private Transform originalSpawnPoint;
        private bool hasBeenGrabbed = false;
        private bool isCrafted = false;
        private bool isInSocket = false;

        private XRGrabInteractable grabInteractable;
        private Coroutine returnRoutine;

        public string ResourceName => resourceName;

        public void Initialize(string resName, string resKey, Transform spawnPoint)
        {
            resourceName = resName;
            resourceKey = resKey;
            originalSpawnPoint = spawnPoint;
        }

        private void Awake()
        {
            grabInteractable = GetComponent<XRGrabInteractable>();
        }

        private void OnEnable()
        {
            grabInteractable.selectEntered.AddListener(OnSelected);
            grabInteractable.selectExited.AddListener(OnDeselected);
        }

        private void OnDisable()
        {
            grabInteractable.selectEntered.RemoveListener(OnSelected);
            grabInteractable.selectExited.RemoveListener(OnDeselected);
        }

        private void OnSelected(SelectEnterEventArgs args)
        {
            if (returnRoutine != null)
            {
                StopCoroutine(returnRoutine);
                returnRoutine = null;
            }

            if (args.interactorObject is XRSocketInteractor)
            {
                isInSocket = true;
                return;
            }

            if (!hasBeenGrabbed)
            {
                hasBeenGrabbed = true;
                ResourceManager.Instance.TrySpend(resourceKey, 1);
                InventoryManager.Instance.ClearShelfReference(gameObject);
                InventoryManager.Instance.UpdateNums();
            }
        }

        private void OnDeselected(SelectExitEventArgs args)
        {
            if (!isActiveAndEnabled) return;

            if (args.interactorObject is XRSocketInteractor)
            {
                isInSocket = false;
                return;
            }

            if (!isCrafted)
            {
                returnRoutine = StartCoroutine(ReturnToInventoryRoutine());
            }
        }

        private IEnumerator ReturnToInventoryRoutine()
        {
            yield return new WaitForSeconds(0.1f);

            if (isInSocket || grabInteractable.isSelected || isCrafted)
            {
                yield break;
            }

            ResourceManager.Instance.Add(resourceKey, 1);
            InventoryManager.Instance.UpdateNums();
            Destroy(gameObject);
        }

        public void MarkAsCrafted()
        {
            isCrafted = true;
            isInSocket = true;
        }
    }
}