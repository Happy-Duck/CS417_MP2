using UnityEngine;
using UnityEngine.UI;

namespace Sharyq
{
    public class CookProgressBar : MonoBehaviour
    {
        [SerializeField] private ResourceProducingStat source;
        [SerializeField] private Image fillImage;
        [SerializeField] private GameObject visualRoot; // background + fill, a child of this object

        private void Awake()
        {
            if (source == null)
                source = GetComponentInParent<ResourceProducingStat>();
            SetVisible(false);
        }

        private void Update()
        {
            if (source == null) return;

            bool active = source.IsActive;
            SetVisible(active);

            if (active)
                fillImage.fillAmount = source.Progress;
        }

        private void SetVisible(bool visible)
        {
            if (visualRoot != null && visualRoot.activeSelf != visible)
                visualRoot.SetActive(visible);
        }
    }
}