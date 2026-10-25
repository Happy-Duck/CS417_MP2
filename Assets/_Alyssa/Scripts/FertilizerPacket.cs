using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Alyssa
{
    // Single-use fertilizer packet. The packet itself is the token: touch it to a planted bed,
    // the plant gets the growth boost, and the packet is used up. Get packets from the shelf by
    // buying them or trading in Fertilizer (see DeviceDispenser).
    [RequireComponent(typeof(XRGrabInteractable))]
    public class FertilizerPacket : MonoBehaviour
    {
        public GameObject effectPrefab;      // sparkle effect shown on the plant while it is boosted
        bool used;

        void OnTriggerEnter(Collider other)
        {
            if (used) return;
            var bed = other.GetComponentInParent<SoilBed>();
            if (bed == null) return;

            var why = FertilizerBoost.WhyNot(bed.plant);      // empty bed, already fertilized, or already ripe
            if (why != null)
            {
                Feedback.Hint("fert-no", why, transform.position);
                return;
            }

            if (FertilizerBoost.Apply(bed.plant, effectPrefab))
            {
                used = true;
                Destroy(gameObject);
            }
        }
    }
}
