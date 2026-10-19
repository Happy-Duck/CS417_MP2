using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

namespace Sharyq
{
    public class GeneralResourceAddedNotifier : MonoBehaviour
    {
        [SerializeField] TMP_Text label;
        [SerializeField] CanvasGroup group;
        [SerializeField] float fadeIn = 0.2f;
        [SerializeField] float hold = 1.5f;
        [SerializeField] float fadeOut = 0.5f;
        [SerializeField] bool deleteOnFinish = true;

        readonly StringBuilder sb = new();
        Coroutine routine;

        void Awake()
        {
            group.alpha = 0;
        }

        public void AddedResource(string ingredientName, int amount)
        {
            sb.AppendLine($"Gained {amount} {ingredientName}");
            label.text = sb.ToString().TrimEnd();

            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(Run());
        }

        public void ClearList()
        {
            sb.Clear();
            label.text = "";
        }

        IEnumerator Run()
        {
            yield return Fade(group.alpha, 1f, fadeIn);
            yield return new WaitForSeconds(hold);
            yield return Fade(1f, 0f, fadeOut);
            routine = null;
            
            if (deleteOnFinish)
                Destroy(gameObject);
        }

        IEnumerator Fade(float from, float to, float duration)
        {
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                group.alpha = Mathf.Lerp(from, to, t / duration);
                yield return null;
            }

            group.alpha = to;
        }
    }
}