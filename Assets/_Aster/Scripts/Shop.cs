using UnityEngine;
using UnityEngine.Events;
using Shared;

namespace Aster
{
    
    public class ItemShop : MonoBehaviour
    {
        public GameObject itemPrefab;
        public Transform spawnPoint;
        public string costResource = "Dollar";
        public int cost = 10;
        public bool giveFreeOnStart = true;
        public bool onlyOneAtATime = true;
        public UnityEvent onBought, onFailed;

        GameObject current;

        void Start() { if (giveFreeOnStart) Spawn(); }

        public void Buy()
        {
            if (onlyOneAtATime && current != null) { onFailed?.Invoke(); return; }
            if (!ResourceManager.Instance.TrySpend(costResource, cost)) { onFailed?.Invoke(); return; }
            Spawn();
            onBought?.Invoke();
        }

        void Spawn()
        {
            current = Instantiate(itemPrefab, spawnPoint.position, spawnPoint.rotation, transform.root);
        }
    }
}
