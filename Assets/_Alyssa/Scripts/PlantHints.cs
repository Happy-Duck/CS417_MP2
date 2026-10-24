using UnityEngine;

namespace Alyssa
{
    // Add to each plant prefab. Covers "plant died" and "fruit left on the plant".
    [RequireComponent(typeof(Plant))]
    public class PlantHints : MonoBehaviour
    {
        public float ripeReminderSeconds = 30f;

        Plant plant;
        float ripeFor;
        bool reminded, quitting;

        void Awake() { plant = GetComponent<Plant>(); }
        void OnApplicationQuit() { quitting = true; }

        void Update()
        {
            var ripe = plant.Get("Ripeness");
            if (ripe != null && ripe.value >= 100f)
            {
                ripeFor += Time.deltaTime;
                if (!reminded && ripeFor >= ripeReminderSeconds)
                {
                    reminded = true;
                    Feedback.Hint("ripe-" + System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this),
                                  "Ripe! Pick the fruit and drop it in the basket.",
                                  transform.position + Vector3.up * 0.3f, 4f, 0f);
                }
            }
            else { ripeFor = 0f; reminded = false; }    // harvesting resets the timer
        }

        void OnDestroy()
        {
            if (quitting) return;
            var thirst = plant.Get("Thirst");
            if (thirst != null && thirst.value <= 0f)   // died of thirst, not because its bed was removed
                Feedback.Hint("plant-died", "A plant dried out and died. Water your plants before Thirst hits 0.",
                              transform.position);
        }
    }
}
