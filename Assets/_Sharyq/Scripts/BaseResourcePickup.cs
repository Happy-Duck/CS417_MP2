using Shared;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Sharyq
{
    public class BaseResourcePickup : MonoBehaviour
    {
        public UnityEvent onResourcePickup;
        [SerializeField]
        protected string resource;

        private XRBaseInteractable _interactable;

        protected virtual void Awake()
        {
            _interactable = GetComponent<XRBaseInteractable>();

            if (_interactable == null)
            {
                Debug.LogError($"{name}: BaseResourcePickup needs an XRBaseInteractable on the same object.", this);
                enabled = false;
            }
        }

        protected virtual void OnEnable()
        {
            if (_interactable != null)
                _interactable.selectEntered.AddListener(Collect);
        }

        protected virtual void OnDisable()
        {
            if (_interactable != null)
                _interactable.selectEntered.RemoveListener(Collect);
        }

        protected virtual void Collect(SelectEnterEventArgs args)
        {
            if (args.interactorObject is not NearFarInteractor)
                return;
            
            ResourceManager.Instance.Add(resource, 1);
            onResourcePickup.Invoke();
            Destroy(gameObject);
        }
    }
}