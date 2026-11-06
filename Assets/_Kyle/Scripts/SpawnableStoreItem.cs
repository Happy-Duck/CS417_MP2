using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Kyle
{
    public class SpawnableStoreItem : StoreItem
    {
        public GameObject prefabToSpawn;
        public Transform spawnPoint;
        public string spawnPopupMessage = "Place the item on an empty cell!";

        private PlaceableAutomation currentSpawnedItem;
        protected override bool IsSoldOut()
        {
            if (currentSpawnedItem != null && !currentSpawnedItem.isPlacedOnGrid)
            {
                return true;
            }
            return false;
        }

        protected override void OnPurchaseSuccess(SelectEnterEventArgs args)
        {
            if (prefabToSpawn != null && spawnPoint != null)
            {
                GameObject spawnedObj = Instantiate(prefabToSpawn, transform.position, transform.rotation);
                currentSpawnedItem = spawnedObj.GetComponent<PlaceableAutomation>();

                var newInteractable = spawnedObj.GetComponent<XRGrabInteractable>();
                if (newInteractable != null && args != null)
                {
                    args.manager.SelectEnter(args.interactorObject, newInteractable);
                }
            }

            errorCatchingHint.message = spawnPopupMessage;
            errorCatchingHint.ShowError();

            CheckLockState();
        }

        private void Update()
        {
            if (currentSpawnedItem != null && currentSpawnedItem.isPlacedOnGrid)
            {
                currentSpawnedItem = null;
                CheckLockState();
            }
        }
    }
}