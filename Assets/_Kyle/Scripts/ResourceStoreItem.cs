using UnityEngine;
using UnityEngine.UI;
using Shared;
using System.Collections.Generic;
using TMPro;
namespace Kyle
{
    public class ResourceStoreItem : StoreItem
    {
        public string itemResource;

        protected override void OnPurchaseSuccess()
        {
            ResourceManager.Instance.Add(itemResource, 1);
        }
    }
}
