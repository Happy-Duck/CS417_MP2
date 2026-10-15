using System.Text;
using UnityEngine;

namespace Alyssa
{
    public class StatLabel : MonoBehaviour
    {
        public Statted target;
        public Vector3 offset = new Vector3(0, 0.5f, 0);
        public float characterSize = 0.02f;
        TextMesh tm;
        float next;
        readonly StringBuilder sb = new();

        void Start()
        {
            if (target == null) target = GetComponentInParent<Statted>();
            var go = new GameObject("StatLabel");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = offset;
            tm = go.AddComponent<TextMesh>();
            tm.characterSize = characterSize;
            tm.fontSize = 48;
            tm.anchor = TextAnchor.LowerCenter;
            tm.alignment = TextAlignment.Center;
        }

        void LateUpdate()
        {
            if (tm == null || target == null) return;
            if (Time.time >= next)
            {
                next = Time.time + 0.25f;
                sb.Clear();
                foreach (var s in target.stats) sb.AppendLine($"{s.name} {s.value:F0}");
                tm.text = sb.ToString();
            }
            var cam = Camera.main;
            if (cam != null)
                tm.transform.rotation = Quaternion.LookRotation(tm.transform.position - cam.transform.position);
        }
    }
}