using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using Shared;

namespace Kyle
{
    [RequireComponent(typeof(XRGrabInteractable))]
    [RequireComponent(typeof(Rigidbody))]
    public class InventoryItem : MonoBehaviour
    {
        private string resourceName;
        private string resourceKey;
        private Transform originalSpawnPoint;
        private bool hasBeenGrabbed = false;
        private bool isCrafted = false;
        private bool isInSocket = false;

        private XRGrabInteractable grabInteractable;
        private Rigidbody rb;
        private Coroutine returnRoutine;
        private ErrorCatchingHint errorHint;

        public string ResourceName => resourceName;

        public void Initialize(string resName, string resKey, Transform spawnPoint)
        {
            resourceName = resName;
            resourceKey = resKey;
            originalSpawnPoint = spawnPoint;

            if (grabInteractable == null)
                grabInteractable = GetComponent<XRGrabInteractable>();
        }

        private void Awake()
        {
            grabInteractable = GetComponent<XRGrabInteractable>();
            rb = GetComponent<Rigidbody>();

            rb.isKinematic = true;
            rb.useGravity = false;

            errorHint = GetComponent<ErrorCatchingHint>();
        }

        //private void Start()
        //{
        //    if (ResourceManager.Instance != null && !string.IsNullOrEmpty(resourceKey))
        //    {
        //        grabInteractable.enabled = ResourceManager.Instance.Get(resourceKey) > 0;
        //    }
        //}

        private void OnEnable()
        {
            grabInteractable.selectEntered.AddListener(OnSelected);
            grabInteractable.selectExited.AddListener(OnDeselected);
            //ResourceManager.Instance.OnResourceChanged += OnResourceAmountChanged;
        }

        private void OnDisable()
        {
            grabInteractable.selectEntered.RemoveListener(OnSelected);
            grabInteractable.selectExited.RemoveListener(OnDeselected);
            //ResourceManager.Instance.OnResourceChanged -= OnResourceAmountChanged;
        }

        private void OnResourceAmountChanged(string changedKey, int newAmount, int change)
        {
            if (!hasBeenGrabbed && changedKey == resourceKey)
            {
                grabInteractable.enabled = newAmount > 0;
            }
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

            // if player tries to grab 0 resource
            if (ResourceManager.Instance.Get(resourceKey) <= 0)
            {
                if (errorHint != null)
                {
                    errorHint.ShowError();
                }

                args.manager.SelectCancel(args.interactorObject, args.interactableObject);
                return;
            }

            if (!hasBeenGrabbed)
            {
                hasBeenGrabbed = true;

                rb.isKinematic = false;
                rb.useGravity = true;

                ResourceManager.Instance.TrySpend(resourceKey, 1);
                InventoryManager.Instance.ClearShelfReference(gameObject);
                //InventoryManager.Instance.UpdateNums();
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

            if (!isCrafted && hasBeenGrabbed)
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
            //InventoryManager.Instance.UpdateNums();
            Destroy(gameObject);
        }

        public void MarkAsCrafted()
        {
            isCrafted = true;
            isInSocket = true;
        }
    }
}