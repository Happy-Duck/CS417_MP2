using UnityEngine;

namespace Alyssa
{
    public class WaterBarrel : Statted
    {
        public float refillPerSec = 40f;
        public float rainPerSec = 2f;          // barrel slowly refills itself
        public float drainMultiplier = 3f;     

        void Awake()
        {
            if (Get("Reservoir") == null)
                stats.Add(new Stat { name = "Reservoir", value = 100, ratePerSec = rainPerSec });
        }

        void OnTriggerStay(Collider other)
        {
            var can = other.GetComponentInParent<WateringCan>();
            if (can == null) return;
            var level = can.Get("WaterLevel");
            var res = Get("Reservoir");
            if (level == null || res == null) return;
            if (level.value >= level.max || res.value <= 0) return;
            float amt = Mathf.Min(refillPerSec * Time.deltaTime, res.value / drainMultiplier, level.max - level.value);
            level.value += amt;
            res.value -= amt * drainMultiplier;
        }
    }
}