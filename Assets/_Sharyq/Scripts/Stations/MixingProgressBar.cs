using UnityEngine;
using UnityEngine.UI;

namespace Sharyq
{
    public class MixProgressBar : MonoBehaviour
    {
        [SerializeField] private MixingStation source;
        [SerializeField] private Image fillImage;
        [SerializeField] private GameObject visualRoot;

        private void Awake()
        {
            if (source == null)
                source = GetComponentInParent<MixingStation>();
            SetVisible(false);
        }

        private void Update()
        {
            if (source == null) return;

            bool active = source.IsMixing;
            SetVisible(active);

            if (active && fillImage != null)
                fillImage.fillAmount = source.Progress;
        }

        private void SetVisible(bool visible)
        {
            if (visualRoot != null && visualRoot.activeSelf != visible)
                visualRoot.SetActive(visible);
        }
    }
}