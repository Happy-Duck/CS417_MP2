using Shared;
using System.Collections.Generic;
using UnityEngine;

namespace Aster
{
    public class Harvestable : MonoBehaviour
    {
        public string resourceName;
        public int amount = 1;
        public float lifetime = 0f;      // 0 = never expires
        public float remaining;          // sttat lifetime

        static readonly List<Harvestable> all = new List<Harvestable>();
        public static int CountOf(string n)
        {
            int c = 0;
            foreach (var h in all) if (h != null && h.resourceName == n) c++;
            return c;
        }

        Vector3 baseScale;
        bool done;

        void OnEnable() { all.Add(this); }
        void OnDisable() { all.Remove(this); }

        void Start() { baseScale = transform.localScale; remaining = lifetime; }

        void Update()
        {
            if (lifetime <= 0f || done) return;
            remaining -= Time.deltaTime;
            transform.localScale = baseScale * Mathf.Lerp(0.3f, 1f, Mathf.Clamp01(remaining / lifetime));
            if (remaining <= 0f) { done = true; Destroy(gameObject); }
        }

        public void Harvest()
        {
            if (done) return;
            done = true;
            ResourceManager.Instance.Add(resourceName, amount);
            Destroy(gameObject);
        }
    }
}
