using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Alyssa
{
    // World-space text that faces the player.
    //   Hint = small message at a spot (error-tolerant feedback)
    //   Card = bigger message floating in front of the player (unlock reveals)
    public class Feedback : MonoBehaviour
    {
        const float HintSize = 0.015f;   // tune these if the text looks too big or small in VR
        const float CardSize = 0.02f;
        const int WrapChars = 30;

        static readonly Dictionary<string, float> lastShown = new Dictionary<string, float>();

        float dieAt, riseSpeed;

        // key stops the same message from spamming; cooldown is in seconds
        public static void Hint(string key, string message, Vector3 position, float seconds = 4f, float cooldown = 8f)
        {
            float t;
            if (lastShown.TryGetValue(key, out t) && Time.time >= t && Time.time - t < cooldown) return;
            lastShown[key] = Time.time;
            Spawn(message, position + Vector3.up * 0.4f, HintSize, new Color(1f, 0.85f, 0.3f), seconds, 0.03f);
        }

        public static void Card(string message, float seconds = 6f)
        {
            var cam = Camera.main;
            if (cam == null) return;
            Vector3 pos = cam.transform.position + cam.transform.forward * 1.5f;
            Spawn(message, pos, CardSize, Color.white, seconds, 0f);
        }

        static void Spawn(string message, Vector3 pos, float size, Color color, float seconds, float rise)
        {
            var go = new GameObject("Feedback");
            go.transform.position = pos;
            var tm = go.AddComponent<TextMesh>();
            tm.text = Wrap(message);
            tm.characterSize = size;
            tm.fontSize = 48;
            tm.color = color;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            var fb = go.AddComponent<Feedback>();
            fb.dieAt = Time.time + seconds;
            fb.riseSpeed = rise;
        }

        static string Wrap(string text)
        {
            var sb = new StringBuilder();
            foreach (var para in text.Split('\n'))
            {
                int line = 0;
                foreach (var w in para.Split(' '))
                {
                    if (line > 0 && line + 1 + w.Length > WrapChars) { sb.Append('\n'); line = 0; }
                    else if (line > 0) { sb.Append(' '); line++; }
                    sb.Append(w);
                    line += w.Length;
                }
                sb.Append('\n');
            }
            return sb.ToString().TrimEnd('\n');
        }

        void LateUpdate()
        {
            if (Time.time >= dieAt) { Destroy(gameObject); return; }
            transform.position += Vector3.up * riseSpeed * Time.deltaTime;
            var cam = Camera.main;
            if (cam != null)
                transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);
        }
    }
}