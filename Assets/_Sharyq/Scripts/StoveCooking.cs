using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Sharyq
{
    public class StoveCooking : MonoBehaviour
    {
        [SerializeField]
        private XRSocketInteractor batterDetection;
        [SerializeField]
        private GameObject batterPreview;
        [SerializeField]
        private GameObject waffleCooking;
        
        private void Awake()
        {
            if (batterDetection == null)
                batterDetection = GetComponentInChildren<XRSocketInteractor>();
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

        private void OnHoverEntered(HoverEnterEventArgs args) => batterPreview.SetActive(true);
        private void OnHoverExited(HoverExitEventArgs args) => batterPreview.SetActive(false);

        private void CookWaffle(SelectEnterEventArgs args)
        {
            batterPreview.SetActive(false);
            args.interactableObject.transform.gameObject.SetActive(false);
            waffleCooking.SetActive(true);
        }
    }
}