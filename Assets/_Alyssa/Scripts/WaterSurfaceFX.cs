using UnityEngine;

namespace Alyssa
{
    public class WaterSurfaceFX : MonoBehaviour
    {
        public Statted source;                    // the object that has WaterBarrel
        public string statName = "Reservoir";
        public float emptyY = 0.05f, fullY = 0.8f;      // local height range
        public float minIntensity = 0.2f, maxIntensity = 1f;
        public float minSpeed = 0.1f, maxSpeed = 0.8f;

        static readonly int IntensityId = Shader.PropertyToID("_Intensity");
        static readonly int SpeedId = Shader.PropertyToID("_Speed");
        Renderer rend;
        MaterialPropertyBlock mpb;

        void Awake()
        {
            rend = GetComponent<Renderer>();
            mpb = new MaterialPropertyBlock();
        }

        void Update()
        {
            if (source == null) return;
            var stat = source.Get(statName);
            if (stat == null) return;
            float t = Mathf.InverseLerp(stat.min, stat.max, stat.value);

            var p = transform.localPosition;
            p.y = Mathf.Lerp(emptyY, fullY, t);
            transform.localPosition = p;

            rend.GetPropertyBlock(mpb);
            mpb.SetFloat(IntensityId, Mathf.Lerp(minIntensity, maxIntensity, t));
            mpb.SetFloat(SpeedId, Mathf.Lerp(minSpeed, maxSpeed, t));
            rend.SetPropertyBlock(mpb);
        }
    }
}