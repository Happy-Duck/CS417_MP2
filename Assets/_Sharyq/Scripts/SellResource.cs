using System;
using UnityEngine;
using Shared;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Sharyq
{
    public class SellResource : MonoBehaviour
    {
        public UnityEvent ItemSoldEvent;
        [SerializeField] private string sellResource;
        [SerializeField] private int sellAmount = 1;
        [SerializeField] private string buyResource;
        [SerializeField] private int buyAmount = 10;
        [SerializeField] private CustomerSpawner spawner;
        
        private Button _sellButton;
        
        public string Resource
        {
            get => sellResource;
            set => sellResource = value;
        }
        public string BuyingResource
        {
            get => buyResource;
            set => buyResource = value;
        }

        private void Awake()
        {
            _sellButton = GetComponent<Button>();
        }

        private void OnEnable() => _sellButton.onClick.AddListener(SellButtonPressed);

        private void OnDisable() => _sellButton.onClick.RemoveListener(SellButtonPressed);
        
        public int BuyingResourceAmount
        {
            get => buyAmount;
            set => buyAmount = value;
        }

        public void SellButtonPressed()
        {
            var rm = ResourceManager.Instance;
            if (rm == null || sellAmount <= 0 || !spawner.HasFreeSeat) return;

            if (rm.TrySpend(sellResource, sellAmount))
            {
                rm.Add(buyResource, buyAmount);
                spawner.TrySpawnCustomer();
                ItemSoldEvent.Invoke();
            }
        }
    }
}

