using UnityEngine;
using UnityEngine.UI;

namespace Sharyq
{
    public class FullnessBar : MonoBehaviour
    {
        [SerializeField] private Customer source;
        [SerializeField] private Image fillImage;
        [SerializeField] private bool hideWhenEmpty = false;
        [SerializeField] private CanvasGroup group;

        private void Update()
        {
            if (source == null || fillImage == null) return;

            float value = source.Fullness;
            fillImage.fillAmount = value;

            if (hideWhenEmpty && group != null)
                group.alpha = value > 0f ? 1f : 0f;
        }
    }
}