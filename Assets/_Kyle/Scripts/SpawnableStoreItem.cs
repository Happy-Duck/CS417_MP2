using UnityEngine;

namespace Kyle
{
    public class SpawnableStoreItem : StoreItem
    {
        public GameObject prefabToSpawn;
        public Transform spawnPoint;
        public string spawnPopupMessage = "Grab the item and place it on the table!";

        protected override void OnPurchaseSuccess()
        {
            if (prefabToSpawn != null && spawnPoint != null)
            {
                Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);
            }

            errorCatchingHint.message = spawnPopupMessage;
            errorCatchingHint.ShowError();
        }
    }
}