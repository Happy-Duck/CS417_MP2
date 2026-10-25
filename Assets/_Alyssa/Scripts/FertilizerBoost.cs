using UnityEngine;

namespace Alyssa
{
    // Doubles a plant's growth speed until its next harvest (one growth cycle).
    // Shows an optional effect on the plant for as long as the boost lasts.
    public class FertilizerBoost : MonoBehaviour
    {
        public float multiplier = 2f;
        Plant plant;
        GameObject effect;
        float baseRate, lastGrowth;

        // null if this plant can be fertilized right now, otherwise the reason it can't
        public static string WhyNot(Plant p)
        {
            if (p == null)
                return "Nothing to fertilize here. Plant a seed first, then add fertilizer.";
            if (p.GetComponent<FertilizerBoost>() != null)
                return "This plant is already fertilized this growth cycle.";
            if (p.Get("Growth").value >= 100f)
                return "This plant is already ripe. Harvest it first, then fertilize the next cycle.";
            return null;
        }

        public static bool Apply(Plant p, GameObject effectPrefab = null)
        {
            if (WhyNot(p) != null) return false;
            var b = p.gameObject.AddComponent<FertilizerBoost>();
            b.plant = p;
            b.baseRate = p.growthPerSec;
            b.lastGrowth = p.Get("Growth").value;
            p.growthPerSec = b.baseRate * b.multiplier;
            if (effectPrefab != null)
                b.effect = Instantiate(effectPrefab, p.transform.position, Quaternion.identity, p.transform);
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

        void OnDestroy()
        {
            if (effect != null) Destroy(effect);  // the effect lasts exactly as long as the boost
        }
    }
}