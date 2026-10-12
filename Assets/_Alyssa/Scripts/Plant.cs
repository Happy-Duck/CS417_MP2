using UnityEngine;

namespace Alyssa
{
    public class Plant : Statted
    {
        public Transform visual;
        public float growthPerSec = 3f;
        Renderer[] rends;

        void Awake()
        {
            if (visual == null) visual = transform;      // fallback so it never crashes
            rends = GetComponentsInChildren<Renderer>(); // cached once
            if (stats.Count == 0)
            {
                stats.Add(new Stat { name = "Thirst", value = 70, ratePerSec = -1f });  // slower drain
                stats.Add(new Stat { name = "Growth", value = 0 });
                stats.Add(new Stat { name = "Ripeness", value = 0 });
            }
        }

        protected override void Update()
        {
            base.Update();
            var thirst = Get("Thirst"); var growth = Get("Growth");
            if (thirst.value > 0)
                growth.value = Mathf.Clamp(growth.value + growthPerSec * Time.deltaTime, 0, 100);
            if (growth.value >= 100) Get("Ripeness").value = 100;

            visual.localScale = Vector3.one * Mathf.Lerp(0.2f, 1f, growth.value / 100f);
            Color c = Color.Lerp(Color.gray, Color.white, thirst.value / 100f);
            foreach (var r in rends) r.material.color = c;

            if (thirst.value <= 0) Destroy(gameObject);   // Death
        }

        public void Water(float amount) =>
            Get("Thirst").value = Mathf.Min(100, Get("Thirst").value + amount);   // Caretaking
    }
}