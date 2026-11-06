using UnityEngine;
using UnityEngine.UI;
using Shared;
using System.Collections.Generic;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;
namespace Kyle
{
    public class ResourceStoreItem : StoreItem
    {
        public string itemResource;

        protected override void OnPurchaseSuccess(SelectEnterEventArgs args)
        {
            ResourceManager.Instance.Add(itemResource, 1);
        }
    }
}
