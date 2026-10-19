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
                Debug.Log(prefab);
                prefab.RemoveResource(costResource, costAmount);
            }
            
            if (payNotifPrefab != null)
                AudioSource.PlayClipAtPoint(paySound, transform.position);
        }
    }
}
