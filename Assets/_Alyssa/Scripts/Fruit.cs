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
            grab.throwOnDetach = false;
            grab.selectEntered.AddListener(_ =>
            {
                rb.useGravity = true;
                transform.SetParent(null, true);  // picked: leave the plant
            });
        }
    }
}