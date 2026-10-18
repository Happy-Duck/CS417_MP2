using Shared;
using System.Collections.Generic;
using UnityEngine;

namespace Aster
{
    [System.Serializable]
    public class Amount
    {
        public string resource;
        public int count = 1;
    }

    [System.Serializable]
    public class Recipe
    {
        public string recipeName;
        public List<Amount> inputs = new List<Amount>();
        public string output;
        public int outputAmount = 1;
        public Color liquidColor = Color.blue;

        public bool CanAffordFromResources()
        {
            foreach (var a in inputs) if (!ResourceManager.Instance.CanAfford(a.resource, a.count)) return false;
            return true;
        }

        public void SpendFromResources()
        {
            foreach (var a in inputs) ResourceManager.Instance.TrySpend(a.resource, a.count);
        }
    }
}
