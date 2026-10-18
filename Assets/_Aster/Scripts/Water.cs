using UnityEngine;
using Shared;

namespace Aster
{
  
    public class Water : MonoBehaviour
    {
        public float fullness;    
        public float fillTime = 3f;
        public int maxWater = 5;

        Vector3 baseScale;
        public bool IsFull => fullness >= 1f;

        void Start() { baseScale = transform.localScale; Apply(); }

        void Update()
        {
            if (fullness < 1f)
            {
                fullness = Mathf.Min(1f, fullness + Time.deltaTime / fillTime);
                Apply();
            }
        }

        public void Collect()
        {
            if (!IsFull) return;
            int space = maxWater - Mathf.RoundToInt(ResourceManager.Instance.Get("Water"));
            if (space <= 0) return;
            ResourceManager.Instance.Add("Water", space);
            Destroy(gameObject);
        }

        void Apply()
        {
            var s = baseScale;
            s.y = baseScale.y * Mathf.Max(0.05f, fullness);
            transform.localScale = s;
        }
    }
}
