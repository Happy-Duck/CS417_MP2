using UnityEngine;
using TMPro;
using Shared;
using System.Collections.Generic;

namespace Kyle
{
    public class StoreManager : MonoBehaviour
    {
        [System.Serializable]
        public struct CycleItem
        {
            public string displayName;
            public string resourceKey;
            public int cost;
            public GameObject visualPrefab;
        }

        public List<CycleItem> cycleItems;

        public TextMeshProUGUI cycleItemNameText;
        public TextMeshProUGUI cycleItemPriceText;

        [SerializeField] TextMeshProUGUI numDollars;
        [SerializeField] private string currencyKey = "Dollar";

        private int currentIndex = 0;

        private void Start()
        {
            if (cycleItems.Count > 0)
            {
                UpdateCycleDisplay();
            }

            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnResourceChanged += HandleResourceChanged;
            }
            UpdateNumDollars();
        }

        public void NextItem()
        {
            if (cycleItems.Count == 0)
            {
                return;
            }

            currentIndex++;

            if (currentIndex >= cycleItems.Count)
            {
                currentIndex = 0;
            }

            UpdateCycleDisplay();
        }

        public void PreviousItem()
        {
            if (cycleItems.Count == 0)
            {
                return;
            }

            currentIndex--;

            if (currentIndex < 0)
            {
                currentIndex = cycleItems.Count - 1;
            }

            UpdateCycleDisplay();
        }

        private void UpdateCycleDisplay()
        {
            for (int i = 0; i < cycleItems.Count; i++)
            {
                cycleItems[i].visualPrefab.SetActive(i == currentIndex);
            }
            CycleItem currentItem = cycleItems[currentIndex];
            if (cycleItemNameText != null) cycleItemNameText.text = currentItem.displayName;
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
                UpdateNumDollars();
            }
        }
        private void UpdateNumDollars()
        {
            if (ResourceManager.Instance != null && numDollars != null)
            {
                numDollars.text = ResourceManager.Instance.Get(currencyKey).ToString();
            }
        }
    }
}
