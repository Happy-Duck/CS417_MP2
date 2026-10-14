using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Alyssa
{
    [RequireComponent(typeof(XRGrabInteractable))]
    public class Fruit : MonoBehaviour
    {
        public string resourceName = "Strawberry";

        void Awake()
        {
            var rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            var grab = GetComponent<XRGrabInteractable>();
            grab.throwOnDetach = false;                 // fixes the warning
            grab.selectEntered.AddListener(_ => rb.useGravity = true);
        }
    }
}