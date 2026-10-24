using UnityEngine;

namespace Alyssa
{
    // Doubles a plant's growth speed until its next harvest (one growth cycle).
    public class FertilizerBoost : MonoBehaviour
    {
        public float multiplier = 2f;
        Plant plant;
        float baseRate, lastGrowth;

        public static bool Apply(Plant p)
        {
            if (p == null || p.GetComponent<FertilizerBoost>() != null) return false;
            var b = p.gameObject.AddComponent<FertilizerBoost>();
            b.plant = p;
            b.baseRate = p.growthPerSec;
            b.lastGrowth = p.Get("Growth").value;
            p.growthPerSec = b.baseRate * b.multiplier;
            GameEvents.RaiseFertilized();
            Feedback.Hint("fertilized-" + System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(p),
                          "Fertilized! Growing twice as fast this cycle.",
                          p.transform.position + Vector3.up * 0.3f, 3f, 0f);
            return true;
        }

        void Update()
        {
            if (plant == null) return;
            float g = plant.Get("Growth").value;
            if (g < lastGrowth - 1f)             // harvesting resets Growth, so the cycle is over
            {
                plant.growthPerSec = baseRate;
                Destroy(this);
                return;
            }
            lastGrowth = g;
        }
    }
}
