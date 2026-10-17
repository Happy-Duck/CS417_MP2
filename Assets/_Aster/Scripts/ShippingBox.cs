using Aster;
using Shared;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Aster
{
   //ships all meth in resource
    public class ShippingBox : MonoBehaviour
    {
        public List<Recipe> sellRecipes = new List<Recipe>();
        public int shipped;               
        public int goalForBar = 10;
        public StatBar bar;
        public UnityEvent onShipped, onNothingToShip;

        public void ShipAll()
        {
            var rm = ResourceManager.Instance;
            int sold = 0;
            foreach (var r in sellRecipes)
            {
                if (r.inputs.Count == 0) continue;
                int guard = 0;
                while (guard++ < 1000 && r.CanAffordFromResources())
                {
                    r.SpendFromResources();
                    rm.Add(r.output, r.outputAmount);
                    shipped++;
                    sold++;
                }
            }
            if (bar != null) bar.Set(Mathf.Min(shipped, goalForBar), goalForBar);
            if (sold > 0) onShipped?.Invoke(); else onNothingToShip?.Invoke();
        }
    }
}
