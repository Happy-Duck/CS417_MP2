using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Alyssa
{
    // Add next to SeedPacket on each packet prefab. Explains why a seed can't be used.
    [RequireComponent(typeof(XRGrabInteractable))]
    public class SeedPacketHints : MonoBehaviour
    {
        int bedsTouching;

        void Awake()
        {
            // pressing the trigger while holding the packet = "I want to use this"
            GetComponent<XRGrabInteractable>().activated.AddListener(_ => OnUseAttempt());
        }

        void OnTriggerEnter(Collider other)
        {
            var bed = other.GetComponentInParent<SoilBed>();
            if (bed == null) return;
            bedsTouching++;
            if (bed.HasWeed)
                Feedback.Hint("seed-weed", "This bed has a weed. Pull the weed first, then plant.", transform.position);
        }

        void OnTriggerExit(Collider other)
        {
            if (other.GetComponentInParent<SoilBed>() != null) bedsTouching = Mathf.Max(0, bedsTouching - 1);
        }

        void OnUseAttempt()
        {
            if (bedsTouching == 0)
                Feedback.Hint("seed-nobed", "Seeds need a soil bed. Place a bed first, then touch the packet to it.", transform.position);
        }
    }
}
