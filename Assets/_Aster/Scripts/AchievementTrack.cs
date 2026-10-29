using System.Collections.Generic;
using System.Text;
using Shared;
using TMPro;
using UnityEngine;

namespace Aster
{
    public class AchievementTrack : MonoBehaviour
    {
        public TMP_Text text;
        public string title = "LAB ACHIEVEMENTS";
        public string[] achievements = { };
        public float flashSeconds = 1.5f;
        public string flashColor = "#FFD54F";

        readonly List<string> unlocked = new List<string>();
        readonly Dictionary<string, float> flashUntil = new Dictionary<string, float>();
        readonly StringBuilder sb = new StringBuilder();
        bool dirty = true, wasFlashing;

        void Start()
        {

            var m = AchievementsManager.Instance;
            foreach (var a in achievements) if (m.IsAchieved(a)) unlocked.Add(a);
            m.onAchieved.AddListener(OnAchieved);

        }

        void OnDestroy()
        {
            if (AchievementsManager.Instance != null)
                AchievementsManager.Instance.onAchieved.RemoveListener(OnAchieved);
        }

        void OnAchieved(string achievementName, string description)
        {
            if (!unlocked.Contains(achievementName)) unlocked.Add(achievementName);
            flashUntil[achievementName] = Time.time + flashSeconds;
            dirty = true;
        }

        void Update()
        {
            if (text == null) return;
            bool flashing = false;
            foreach (var kv in flashUntil) if (Time.time < kv.Value) { flashing = true; break; }
            if (flashing || wasFlashing) dirty = true;
            wasFlashing = flashing;

            if (dirty) { Redraw(); dirty = false; }
        }

        void Redraw()
        {
            sb.Clear();
            sb.Append("<b>").Append(title).Append(": ").Append(unlocked.Count).Append('/').Append(achievements.Length).Append("</b>\n");
            foreach (var a in unlocked)
            {
                bool flash = flashUntil.TryGetValue(a, out float t) && Time.time < t;
                if (flash) sb.Append("<color=").Append(flashColor).Append('>');
                sb.Append("- ").Append(a);
                if (flash) sb.Append("</color>");
                sb.Append('\n');
            }
            text.text = sb.ToString();
        }
    }
}
