using UnityEngine;
using TMPro;
using Shared;

namespace Kyle
{
    public class StoreManager : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI numDollars;
        [SerializeField] private string currencyKey = "Dollar";

        private void Start()
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnResourceChanged += HandleResourceChanged;
            }
            UpdateUI();
        }

        private void OnDestroy()
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnResourceChanged -= HandleResourceChanged;
            }
        }
        private void HandleResourceChanged(string resourceKey, int newAmount, int change)
        {
            if (resourceKey == currencyKey)
            {
                UpdateUI();
            }
        }
        private void UpdateUI()
        {
            if (ResourceManager.Instance != null && numDollars != null)
            {
                numDollars.text = ResourceManager.Instance.Get(currencyKey).ToString();
            }
        }
    }
}
