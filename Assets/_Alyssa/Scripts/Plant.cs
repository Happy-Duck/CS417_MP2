using System.Collections.Generic;
using UnityEngine;

namespace Alyssa
{
    public class Plant : Statted
    {
        public GameObject[] stages;          // 0 = seedling ... last = fully grown
        public Transform visual;             // only used if Stages is empty
        public float growthPerSec = 3f;
        public Transform[] fruitPoints;    
        public GameObject fruitPrefab;      
        public string signatureStat = "";     

        [Header("Dryness tint")]
        public bool tintWhenDry = true;      
        public Color dryTint = new Color(0.7f, 0.6f, 0.4f);   // brownish when thirsty

        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly int ColorId = Shader.PropertyToID("_Color");

        Renderer[] rends;
        Color[] originalColors;
        int[] colorIds;
        MaterialPropertyBlock mpb;

        readonly List<GameObject> fruits = new();
        readonly List<Transform> homes = new();
        bool hasFruit;
        int shownStage = -1;

        void Awake()
        {
            if (visual == null) visual = transform;
            mpb = new MaterialPropertyBlock();
            CacheOriginalColors();
            EnsureStat("Thirst", 70, -1f);
            EnsureStat("Growth", 0, 0f);
            EnsureStat("Ripeness", 0, 0f);
        }

        void EnsureStat(string n, float v, float rate)
        {
            if (Get(n) == null) stats.Add(new Stat { name = n, value = v, ratePerSec = rate });
        }

        // remember each renderer's real material color so tinting multiplies it instead of replacing it
        void CacheOriginalColors()
        {
            rends = GetComponentsInChildren<Renderer>(true);   // includes inactive stages
            originalColors = new Color[rends.Length];
            colorIds = new int[rends.Length];
            for (int i = 0; i < rends.Length; i++)
            {
                originalColors[i] = Color.white;
                colorIds[i] = BaseColorId;
                var mat = rends[i].sharedMaterial;
                if (mat == null) continue;
                if (mat.HasProperty(BaseColorId)) { colorIds[i] = BaseColorId; originalColors[i] = mat.GetColor(BaseColorId); }
                else if (mat.HasProperty(ColorId)) { colorIds[i] = ColorId; originalColors[i] = mat.GetColor(ColorId); }
            }
        }

        void OnDestroy()
        {
            // fruit still tracked here was never picked, so remove it with the plant
            foreach (var f in fruits)
                if (f != null) Destroy(f);
        }

        protected override void Update()
        {
            base.Update();
            var thirst = Get("Thirst"); var growth = Get("Growth");

            if (thirst.value > 0 && !hasFruit)
                growth.value = Mathf.Clamp(growth.value + growthPerSec * Time.deltaTime, 0, 100);
            Get("Ripeness").value = growth.value >= 100 ? 100 : 0;

            ShowStage(growth.value);
            ApplyTint(thirst.value / 100f);
            UpdateFruit(growth);

            if (thirst.value <= 0) Destroy(gameObject);         // Death
        }

        void ShowStage(float g)
        {
            if (stages == null || stages.Length == 0)
            {          // fallback: scale
                visual.localScale = Vector3.one * Mathf.Lerp(0.2f, 1f, g / 100f);
                return;
            }
            int idx = Mathf.Min(stages.Length - 1, (int)(g / 100f * stages.Length));
            if (idx == shownStage) return;
            shownStage = idx;
            for (int i = 0; i < stages.Length; i++) stages[i].SetActive(i == idx);
        }

        void ApplyTint(float wetness)
        {
            if (!tintWhenDry) return;
            // fully wet = original look, dry = dryTint multiplied on top
            Color tint = Color.Lerp(dryTint, Color.white, Mathf.Clamp01(wetness * 1.5f));
            for (int i = 0; i < rends.Length; i++)
            {
                if (rends[i] == null) continue;
                rends[i].GetPropertyBlock(mpb);
                mpb.SetColor(colorIds[i], originalColors[i] * tint);
                rends[i].SetPropertyBlock(mpb);
            }
        }

        void UpdateFruit(Stat growth)
        {
            if (fruitPrefab == null || fruitPoints == null || fruitPoints.Length == 0) return;

            if (!hasFruit && growth.value >= 100)
            {
                var sig = string.IsNullOrEmpty(signatureStat) ? null : Get(signatureStat);
                float fruitScale = sig == null ? 1f : Mathf.Lerp(0.7f, 1.3f, sig.value / 100f);
                foreach (var p in fruitPoints)
                {
                    var f = Instantiate(fruitPrefab, p.position, p.rotation, p);
                    f.transform.localScale *= fruitScale;      // signature stat changes fruit size
                    fruits.Add(f);
                    homes.Add(p);
                }

                hasFruit = true;
            }
            if (!hasFruit) return;

            // a fruit counts as picked once it's destroyed or carried away from its spot
            for (int i = fruits.Count - 1; i >= 0; i--)
            {
                if (fruits[i] == null ||
                    Vector3.Distance(fruits[i].transform.position, homes[i].position) > 0.2f)
                {
                    fruits.RemoveAt(i); homes.RemoveAt(i);
                }
            }
            if (fruits.Count == 0)
            {
                hasFruit = false;
                growth.value = 60;
            }
        }

        public void Water(float amount) =>
            Get("Thirst").value = Mathf.Min(100, Get("Thirst").value + amount);   // Caretaking
    }
}