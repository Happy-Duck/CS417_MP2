using UnityEngine;

namespace Aster
{
    public class StatBar : MonoBehaviour
    {
        public Transform fill;
        public Color low = Color.red;
        public Color high = Color.green;

        Vector3 baseScale, basePos;
        Renderer r;
        bool init;

        void Init()
        {
            if (init || fill == null) return;
            init = true;
            baseScale = fill.localScale;
            basePos = fill.localPosition;
            r = fill.GetComponent<Renderer>();
        }

        public void Set(float value, float max)
        {
            Init();
            if (fill == null) return;
            float t = max > 0 ? Mathf.Clamp01(value / max) : 0f;
            var s = baseScale; s.x = baseScale.x * t;
            fill.localScale = s;
            var p = basePos; p.x = basePos.x - baseScale.x * (1f - t) * 0.5f;
            fill.localPosition = p;
            if (r != null) r.material.color = Color.Lerp(low, high, t);
        }
    }
}
