using System.Collections;
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
        private GameObject batterPrefab;
        [SerializeField]
        private Transform instanceLocation;
        [SerializeField] 
        private ResourceNotifier popup;
        [SerializeField] 
        private bool ejectRejectedItems = true;
        [SerializeField] 
        private float rejectCooldown = 1f;

        private Batter _currentBatter;
        private WaffleStat _readyWaffle;
        private XRBaseInteractable _currentInteractable;
        private Coroutine _ejectRoutine;
        
        private void Awake()
        {
            if (batterDetection == null)
                batterDetection = GetComponentInChildren<XRSocketInteractor>();
            if (_currentInteractable == null)
                _currentInteractable = GetComponent<XRBaseInteractable>();
            if (popup == null)
                popup = GetComponentInChildren<ResourceNotifier>();
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
            
            _ejectRoutine = null;
            batterDetection.socketActive = true;
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
            if (!args.interactableObject.transform.TryGetComponent(out Batter batter))
            {
                RejectIngredient(args.interactableObject);
                return;
            }

            args.interactableObject.transform.gameObject.SetActive(false);
            BeginCooking(batter);
        }

        private void RejectIngredient(IXRSelectInteractable interactable)
        {
            batterPreview.SetActive(false);

            if (popup != null)
                popup.InvalidIngredient();

            if (ejectRejectedItems && _ejectRoutine == null)
                _ejectRoutine = StartCoroutine(EjectRoutine(interactable));
        }

        private IEnumerator EjectRoutine(IXRSelectInteractable interactable)
        {
            yield return null;

            if (batterDetection != null && batterDetection.interactionManager != null &&
                interactable != null && batterDetection.IsSelecting(interactable))
            {
                batterDetection.interactionManager.SelectExit(batterDetection, interactable);
            }
            
            batterDetection.socketActive = false;
            yield return new WaitForSeconds(rejectCooldown);
            batterDetection.socketActive = true;

            _ejectRoutine = null;
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

        public bool TryAutomate(Employee employee)
        {
            if (_readyWaffle != null)
            {
                _readyWaffle.EmployeeCollect();
                _readyWaffle = null;
                ActivateStoveMovement();
                return true;
            }
            
            if (_currentBatter != null || !batterDetection.enabled) return false;
            if (ResourceManager.Instance.Get("Batter") <= 0) return false;

            ResourceManager.Instance.TrySpend("Batter", 1);
            var batter = Instantiate(batterPrefab).GetComponent<Batter>();
            batter.gameObject.SetActive(false);
            BeginCooking(batter);
            if (!AchievementsManager.Instance.IsAchieved("Sit and Relax"))
                AchievementsManager.Instance.CompleteAchievement("Sit and Relax");
            return true;
        }
    }
}