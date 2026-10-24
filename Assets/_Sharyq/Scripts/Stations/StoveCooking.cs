using Shared;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Sharyq
{
    public class StoveCooking : MonoBehaviour, IAutomatable
    {
        public UnityEvent StartCooking;
        [SerializeField]
        private XRSocketInteractor batterDetection;
        [SerializeField]
        private GameObject batterPreview;
        [SerializeField]
        private GameObject waffleCooking;
        [SerializeField]
        private Transform instanceLocation;
        private Batter _currentBatter;
        private WaffleStat _readyWaffle;
        private XRBaseInteractable _currentInteractable;
        
        private void Awake()
        {
            if (batterDetection == null)
                batterDetection = GetComponentInChildren<XRSocketInteractor>();
            if (_currentInteractable == null)
                _currentInteractable = GetComponent<XRBaseInteractable>();
        }

        private void OnEnable()
        {
            batterDetection.hoverEntered.AddListener(OnHoverEntered);
            batterDetection.hoverExited.AddListener(OnHoverExited);
            batterDetection.selectEntered.AddListener(CookWaffle);
        }

        private void OnDisable()
        {
            batterDetection.hoverEntered.RemoveListener(OnHoverEntered);
            batterDetection.hoverExited.RemoveListener(OnHoverExited);
            batterDetection.selectEntered.RemoveListener(CookWaffle);
        }

        private void OnHoverEntered(HoverEnterEventArgs args)
        {
            if (args.interactorObject is not NearFarInteractor)
                return;
            
            var detector = batterDetection.GetComponent<DetectInArea>();
            if (detector.IsInArea())
                batterPreview.SetActive(true);
        }
        private void OnHoverExited(HoverExitEventArgs args) => batterPreview.SetActive(false);

        private void CookWaffle(SelectEnterEventArgs args)
        {
            if (!args.interactableObject.transform.TryGetComponent(out Batter batter)) return;
            args.interactableObject.transform.gameObject.SetActive(false);
            BeginCooking(batter);
        }

        private void BeginCooking(Batter batter)
        {
            _currentInteractable.enabled = false;
            batterPreview.SetActive(false);
            batterDetection.enabled = false;
            _currentBatter = batter;
            waffleCooking.SetActive(true);
            StartCooking.Invoke();
        }
        
        public void FinishCooking()
        {
            batterPreview.SetActive(false);
            waffleCooking.SetActive(false);
            _readyWaffle = _currentBatter.CreateWaffle(instanceLocation);
            _readyWaffle.stove = this;
            Destroy(_currentBatter.gameObject);
        }

        public void ActivateStoveMovement()
        {
            _currentInteractable.enabled = true;
            batterDetection.enabled = true;
        }

        public bool TryAutomate()
        {
            if (_readyWaffle != null)
            {
                Destroy(_readyWaffle.gameObject);
                _readyWaffle = null;
                ActivateStoveMovement();
                return true;
            }
            
            if (_currentBatter != null || !batterDetection.enabled) return false;
            if (ResourceManager.Instance.Get("Batter") <= 0) return false;

            ResourceManager.Instance.TrySpend("Batter", 1);
            var batter = Instantiate(new GameObject()).AddComponent<Batter>();
            batter.gameObject.SetActive(false);
            BeginCooking(batter);
            return true;
        }
    }
}