using UnityEngine;
using System.Collections.Generic;
using Shared; // Needed for ResourceManager

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
                        ResourceManager.Instance.Add(currencyKey, price);
                    }

                    Destroy(item.gameObject);

                    if (sellSound != null)
                    {
                        AudioSource.PlayClipAtPoint(sellSound, transform.position);
                    }
                }
                else
                {
                    Debug.Log($"cannot sell {item.ResourceName}");
                }
            }
        }
    }
}