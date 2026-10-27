using Aster;
using Shared;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Aster {
    
    public class Beaker : MonoBehaviour
    {
        public List<Recipe> recipes = new List<Recipe>();
        public TMP_Text statusText;       // world-space text above the beaker showing needs vs. turned in
        public Renderer liquidRenderer;
        public StatBar bar;               // progress of the last recipe
        public UnityEvent onDeposit, onCompleted, onFailed;

        int[][] progress;                
        int active;
        Vector3 liquidBase;
        readonly StringBuilder sb = new StringBuilder();

        void Start()
        {
            progress = new int[recipes.Count][];
            for (int i = 0; i < recipes.Count; i++) progress[i] = new int[recipes[i].inputs.Count];
            Refresh();
        }

        public void Deposit(int index)
        {
            if (index < 0 || index >= recipes.Count) return;
            var r = recipes[index];
            var rm = ResourceManager.Instance;
            active = index;

            bool gaveAny = false;
            for (int i = 0; i < r.inputs.Count; i++)
            {
                var a = r.inputs[i];
                int need = a.count - progress[index][i];
                if (need <= 0) continue;
                int give = Mathf.Min(need, rm.Get(a.resource));
                if (give > 0 && rm.TrySpend(a.resource, give))
                {
                    progress[index][i] += give;
                    gaveAny = true;
                }
            }

            if (!gaveAny) { onFailed?.Invoke(); Refresh(); return; }

            if (IsComplete(index))
            {
                rm.Add(r.output, r.outputAmount);
                for (int i = 0; i < progress[index].Length; i++) progress[index][i] = 0;
                if (liquidRenderer != null) liquidRenderer.material.color = r.liquidColor;
                onCompleted?.Invoke();
            }
            else onDeposit?.Invoke();

            Refresh();
        }

        bool IsComplete(int index)
        {
            var r = recipes[index];
            for (int i = 0; i < r.inputs.Count; i++)
                if (progress[index][i] < r.inputs[i].count) return false;
            AchievementsManager.Instance.CompleteAchievement("Mmm...");
            return true;
        }

        float Fraction(int index)
        {
            var r = recipes[index];
            int have = 0, need = 0;
            for (int i = 0; i < r.inputs.Count; i++)
            {
                have += Mathf.Min(progress[index][i], r.inputs[i].count);
                need += r.inputs[i].count;
            }
            return need > 0 ? (float)have / need : 0f;
        }

        void Refresh()
        {
            if (progress == null || recipes.Count == 0) return;

            if (statusText != null)
            {
                sb.Clear();
                for (int i = 0; i < recipes.Count; i++)
                {
                    var r = recipes[i];
                    sb.Append("<b>").Append(r.recipeName).Append("</b>\n");
                    for (int j = 0; j < r.inputs.Count; j++)
                    {
                        bool done = progress[i][j] >= r.inputs[j].count;
                        if (done) sb.Append("<color=#7CFC00>");
                        sb.Append(r.inputs[j].resource).Append(' ')
                          .Append(progress[i][j]).Append('/').Append(r.inputs[j].count);
                        if (done) sb.Append("</color>");
                        sb.Append('\n');
                    }
                    sb.Append('\n');
                }
                statusText.text = sb.ToString();
            }

            float f = Fraction(active);
            if (bar != null) bar.Set(f, 1f);
        }
    }
}
