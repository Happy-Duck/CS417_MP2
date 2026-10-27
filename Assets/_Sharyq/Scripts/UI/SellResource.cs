using System;
using UnityEngine;
using Shared;
using UnityEngine.Events;
using UnityEngine.Playables;
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
        [SerializeField] private GameObject notifier;
        [SerializeField] private Transform sellNotifLocation;
        [SerializeField] private Transform buyNotifLocation;
        
        [SerializeField] private AudioClip paySound;
        
        private Button _sellButton;
        [SerializeField] private AudioClip failSound;

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
            if (rm == null || sellAmount <= 0 || !spawner.HasFreeSeat)
            {
                var notif =  Instantiate(notifier, sellNotifLocation.position, sellNotifLocation.rotation).GetComponent<ResourceNotifier>();
                notif.CantSellWaffle();
                AudioSource.PlayClipAtPoint(failSound, transform.position);
            };

            if (rm.TrySpend(sellResource, sellAmount))
            {
                rm.Add(buyResource, buyAmount);
                spawner.TrySpawnCustomer();
                var notif =  Instantiate(notifier, sellNotifLocation.position, sellNotifLocation.rotation).GetComponent<ResourceNotifier>();
                var notif2 =  Instantiate(notifier, buyNotifLocation.position, buyNotifLocation.rotation).GetComponent<ResourceNotifier>();
                notif.RemoveResource(sellResource, sellAmount);
                notif2.AddedResource(buyResource, buyAmount);
                AudioSource.PlayClipAtPoint(paySound, transform.position);
                ItemSoldEvent.Invoke();
            }
        }
    }
}

