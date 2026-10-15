using UnityEngine;
using System.Collections;
using Shared;
using TMPro;

namespace Kyle
{
    public class InventoryManager : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI numWheatSeeds;
        [SerializeField] TextMeshProUGUI numCornSeeds;
        [SerializeField] TextMeshProUGUI numFertilizer;
        [SerializeField] TextMeshProUGUI numCorn;
        [SerializeField] TextMeshProUGUI numWheat;
        [SerializeField] TextMeshProUGUI numWeeds;

        [SerializeField] GameObject cornPrefab;
        [SerializeField] private Transform cornSpawn;
        [SerializeField] GameObject wheatPrefab;
        [SerializeField] private Transform wheatSpawn;
        [SerializeField] GameObject weedsPrefab;
        [SerializeField] private Transform weedsSpawn;

        public static InventoryManager Instance { get; private set; }

        // Track the active item sitting on each shelf spawn point
        private GameObject shelfCorn;
        private GameObject shelfWheat;
        private GameObject shelfWeeds;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            UpdateNums();
            StartCoroutine(ResourcePollRoutine());
        }

        private IEnumerator ResourcePollRoutine()
        {
            WaitForSeconds wait = new WaitForSeconds(0.2f);

            while (true)
            {
                EnsureShelfItems();
                UpdateNums();
                yield return wait;
            }
        }
        private void EnsureShelfItems()
        {
            if (shelfCorn == null && ResourceManager.Instance.Get("HarvestedCorn") > 0)
            {
                shelfCorn = SpawnPhysicalItem(cornPrefab, cornSpawn, "Corn", "HarvestedCorn");
            }

            if (shelfWheat == null && ResourceManager.Instance.Get("HarvestedWheat") > 0)
            {
                shelfWheat = SpawnPhysicalItem(wheatPrefab, wheatSpawn, "Wheat", "HarvestedWheat");
            }

            if (shelfWeeds == null && ResourceManager.Instance.Get("Weeds") > 0)
            {
                shelfWeeds = SpawnPhysicalItem(weedsPrefab, weedsSpawn, "Weeds", "Weeds");
            }
        }

        public void ClearShelfReference(GameObject grabbedObj)
        {
            if (grabbedObj == shelfCorn) shelfCorn = null;
            else if (grabbedObj == shelfWheat) shelfWheat = null;
            else if (grabbedObj == shelfWeeds) shelfWeeds = null;
        }

        private GameObject SpawnPhysicalItem(GameObject prefab, Transform spawnPoint, string resourceName, string resourceKey)
        {
            if (prefab != null && spawnPoint != null)
            {
                GameObject spawnedObj = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

                InventoryItem tracker = spawnedObj.GetComponent<InventoryItem>();
                if (tracker != null)
                {
                    tracker.Initialize(resourceName, resourceKey, spawnPoint);
                }
                return spawnedObj;
            }
            return null;
        }

        public void UpdateNums()
        {
            numWheatSeeds.text = ResourceManager.Instance.Get("WheatSeed").ToString();
            numCornSeeds.text = ResourceManager.Instance.Get("CornSeed").ToString();
            numFertilizer.text = ResourceManager.Instance.Get("Fertilizer").ToString();

            numCorn.text = ResourceManager.Instance.Get("HarvestedCorn").ToString();
            numWheat.text = ResourceManager.Instance.Get("HarvestedWheat").ToString();
            numWeeds.text = ResourceManager.Instance.Get("Weeds").ToString();
        }
    }
}