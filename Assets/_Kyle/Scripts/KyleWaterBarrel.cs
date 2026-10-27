using UnityEngine;
using Alyssa;

namespace Kyle
{
    public class KyleWaterBarrel : WaterBarrel
    {
        new void OnTriggerStay(Collider other)
        {
            var pourController = other.GetComponentInParent<PourController>();
            if (pourController == null) return;

            var level = pourController.Get("WaterLevel");
            var res = Get("Reservoir");

            if (level == null || res == null) return;
            if (level.value >= level.max || res.value <= 0) return;

            float amt = Mathf.Min(refillPerSec * Time.deltaTime, res.value / drainMultiplier, level.max - level.value);
            level.value += amt;
            res.value -= amt * drainMultiplier;
        }
    }
}
