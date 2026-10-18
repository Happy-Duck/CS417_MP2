using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using Shared;

namespace Aster
{
    public class ResourceTrack : MonoBehaviour
    {
        public TMP_Text text;
        public string title = "INVENTORY";
        //public string[] resources = {
        //    "Water","Gasoline","Medicine","Paint Thinner","Matches","Acetone","Dye",
        //    "Batteries","Fertilizer","Salt","Mystery Juice","METH","GOLD"
        //};
        public string[] resources = {};
        public bool hideZeroRows = false;
        public float flashSeconds = 1.5f;
        public string flashColor = "#FFD54F";

        readonly Dictionary<string, float> flashUntil = new Dictionary<string, float>();
        readonly StringBuilder sb = new StringBuilder();
        bool flag = true, wasFlashing;

        void Start()
        {
            ResourceManager.Instance.OnResourceChanged += HandleChange;
        }

        void OnDestroy()
        {
            if (ResourceManager.Instance != null)
                ResourceManager.Instance.OnResourceChanged -= HandleChange;
        }

        void HandleChange(string resource, int newAmount, int change)
        {
            flashUntil[resource] = Time.time + flashSeconds;
            flag = true;
        }

        void Update()
        {
            if (text == null || ResourceManager.Instance == null) return;
            bool flashing = false;
            foreach (var kv in flashUntil) if (Time.time < kv.Value) { flashing = true; break; }
            if (flashing || wasFlashing) flag = true;
            wasFlashing = flashing;

            if (flag) { Redraw(); flag = false; }
        }

        void Redraw()
        {
            sb.Clear();
            sb.Append("<b>").Append(title).Append("</b>\n");
            foreach (var r in resources)
            {
                int v = ResourceManager.Instance.Get(r);
                if (hideZeroRows && v <= 0) continue;
                bool flash = flashUntil.TryGetValue(r, out float t) && Time.time < t;
                if (flash) sb.Append("<color=").Append(flashColor).Append('>');
                sb.Append(r).Append(": ").Append(v);
                if (flash) sb.Append("</color>");
                sb.Append('\n');
            }
            text.text = sb.ToString();
        }
    }
}
