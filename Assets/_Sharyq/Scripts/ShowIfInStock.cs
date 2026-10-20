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
        [SerializeField] private bool scriptEnabled = false;
        private void Start()
        {
            if (!scriptEnabled) return;
            _gridPurchaseShelf = GetComponent<PurchaseShelf>();
            _gridPurchaseShelf.enabled = false;
        }
        
        private void Update()
        {
            if (!scriptEnabled) return;
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
