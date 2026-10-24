using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Alyssa
{
    // Grabbable fertilizer. Touch it to a planted bed.
    [RequireComponent(typeof(XRGrabInteractable))]
    public class FertilizerItem : MonoBehaviour
    {
        bool used;

        void OnTriggerEnter(Collider other)
        {
            if (used) return;
            var bed = other.GetComponentInParent<SoilBed>();
            if (bed == null) return;

            if (bed.plant == null)       // Error Tolerant Lesson
            {
                Feedback.Hint("fert-empty", "Nothing to fertilize here. Plant a seed first, then add fertilizer.", transform.position);
                return;
            }
            if (!FertilizerBoost.Apply(bed.plant))
            {
                Feedback.Hint("fert-twice", "This plant is already fertilized this growth cycle.", transform.position);
                return;
            }
            used = true;
            Destroy(gameObject);
        }
    }
}
