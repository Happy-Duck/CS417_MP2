using UnityEngine;

namespace Sharyq
{
    public class BuildingBought : MonoBehaviour
    {
        [SerializeField] private AudioClip paySound;
        [SerializeField] private GameObject payNotifPrefab;
        [SerializeField] private Transform location;
        
        public void PayResource(string costResource, int costAmount)
        {
            if (payNotifPrefab != null)
            {
                var prefab = Instantiate(payNotifPrefab, location.position, location.rotation).GetComponent<ResourceNotifier>();
                prefab.RemoveResource(costResource, costAmount);
            }
            
            if (paySound != null)
                AudioSource.PlayClipAtPoint(paySound, transform.position);
        }
    }
}
