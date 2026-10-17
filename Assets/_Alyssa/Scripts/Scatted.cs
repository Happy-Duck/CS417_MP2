using System.Collections.Generic;
using UnityEngine;

namespace Alyssa
{
    [System.Serializable]
    public class Stat { public string name; public float value = 50, min = 0, max = 100, ratePerSec; }

    public class Statted : MonoBehaviour
    {
        public List<Stat> stats = new();
        public Stat Get(string n) => stats.Find(s => s.name == n);
        protected virtual void Update()
        {
            foreach (var s in stats)   // Euler integration
                s.value = Mathf.Clamp(s.value + s.ratePerSec * Time.deltaTime, s.min, s.max);
        }
    }
}