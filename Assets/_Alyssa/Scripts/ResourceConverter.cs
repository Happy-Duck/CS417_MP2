using Shared;
using UnityEngine;

namespace Alyssa
{
    // Compost heap: turns other teammates' resources into Compost.
    // Rename the inputs in the Inspector to match the real resource names.
    public class ResourceConverter : MonoBehaviour
    {
        [System.Serializable]
        public class Recipe
        {
            public string input;
            public string output = "Compost";
            public int inputCost = 1;
            public int outputGain = 1;
        }

        public Recipe[] recipes =
        {
            new Recipe { input = "ChickenPoop" },    // chicken coop sim
            new Recipe { input = "FoodScraps" },     // waffle house sim
        };
        public float tick = 1f;

        void Start() { InvokeRepeating(nameof(ConvertBatch), tick, tick); }

        void ConvertBatch()
        {
            var rm = ResourceManager.Instance;
            if (rm == null) return;
            foreach (var r in recipes)
            {
                if (!rm.CanAfford(r.input, r.inputCost)) continue;
                rm.TrySpend(r.input, r.inputCost);
                rm.Add(r.output, r.outputGain);
            }
        }
    }
}
