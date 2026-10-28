using Aster;
using Shared;
using UnityEngine;

namespace Aster
{
    
    public class Sink : MonoBehaviour
    {
        public float waterLevel = 100f;  
        public float maxWater = 100f;
        public float fillCost = 25f;
        public float refillRate = 4f;
        public Water Prefab;
        public Transform basin;         //water starts here
        public StatBar bar;

        Water current;

        void Update()
        {
            if (waterLevel < maxWater)
                waterLevel = Mathf.Min(maxWater, waterLevel + refillRate * Time.deltaTime);

            if (bar != null) bar.Set(waterLevel, maxWater);
        }

        public void Interact()
        {
            if (current != null || waterLevel < fillCost) return;
            AchievementsManager.Instance.CompleteAchievement("Not for drinking");
            waterLevel -= fillCost;
            current = Instantiate(Prefab, basin.position, basin.rotation, transform.root);
        }
    }
}
