using UnityEngine;
using System.Collections.Generic;
using Shared;
using TMPro;

namespace Kyle
{
    public class ShoppingCartScript : MonoBehaviour
    {
        [System.Serializable]
        public struct ItemPrice
        {
            public string resourceKey;
            public int sellPrice;
        }

        [SerializeField] private string currencyKey = "Dollar";
        [SerializeField] private List<ItemPrice> priceList;
        [SerializeField] AudioClip sellSound;
        [SerializeField] TextMeshProUGUI sellText;
        [SerializeField] ErrorCatchingHint errorCatchingHint;

        public int sellNum = 1;

        private Dictionary<string, int> prices = new Dictionary<string, int>();

        private void Awake()
        {
            // convert list into dictionary for speed
            foreach (var item in priceList)
            {
                prices[item.resourceKey] = item.sellPrice;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            InventoryItem item = other.GetComponentInParent<InventoryItem>();

            if (item != null)
            {
                if (prices.TryGetValue(item.ResourceKey, out int price))
                {
                    if (ResourceManager.Instance != null)
                    {
                        if (ResourceManager.Instance.TrySpend(item.ResourceKey, sellNum))
                        {
                            ResourceManager.Instance.Add(currencyKey, price * sellNum);
                            Destroy(item.gameObject);

                            if (sellSound != null)
                            {
                                AudioSource.PlayClipAtPoint(sellSound, transform.position);
                            }
                        }
                        else
                        {
                            int numItems = ResourceManager.Instance.Get(item.ResourceKey);
                            ResourceManager.Instance.TrySpend(item.ResourceKey, numItems);
                            errorCatchingHint.message = $"Not enough {item.ResourceKey}! Spent {numItems} instead.";
                            errorCatchingHint.ShowError();
                        }
                    }
                }
                else
                {
                    Debug.Log($"cannot sell {item.ResourceName}");
                }
            }
        }

        public void IncreaseNum()
        {
            sellNum++;
            sellText.text = sellNum.ToString();
        }

        public void DecreaseNum()
        {
            if (sellNum > 1)
            {
                sellNum--;
            }
            sellText.text = sellNum.ToString();
        }
    }
}