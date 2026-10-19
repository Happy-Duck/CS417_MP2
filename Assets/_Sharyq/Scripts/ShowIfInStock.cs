using Mono.Cecil;
using Shared;
using UnityEngine;
using Rishi;

namespace Sharyq
{
    [RequireComponent(typeof(PurchaseShelf))]
    public class ShowIfInStock : MonoBehaviour
    {
        [SerializeField] private string neededResource;
        private PurchaseShelf _gridPurchaseShelf;
        [SerializeField] private bool enabled = false;
        private void Start()
        {
            if (!enabled) return;
            _gridPurchaseShelf = GetComponent<PurchaseShelf>();
            _gridPurchaseShelf.enabled = false;
        }
        
        private void Update()
        {
            if (!enabled) return;
            if (!_gridPurchaseShelf.enabled)
            {
                if (ResourceManager.Instance.Get(neededResource) != 0)
                {
                    _gridPurchaseShelf.enabled = true;
                }
            }
            else
            {
                if (ResourceManager.Instance.Get(neededResource) <= 0)
                {
                    _gridPurchaseShelf.enabled = false;
                }
            }
        }
    }

}
