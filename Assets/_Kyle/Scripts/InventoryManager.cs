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

        // second shelf
        [SerializeField] TextMeshProUGUI numChickenPoop;
        [SerializeField] TextMeshProUGUI numBurntWaffle;
        [SerializeField] TextMeshProUGUI numLeftover;

        [SerializeField] GameObject chickenPoopPrefab;
        [SerializeField] private Transform chickenPoopSpawn;
        [SerializeField] GameObject burntWafflePrefab;
        [SerializeField] private Transform burntWaffleSpawn;
        [SerializeField] GameObject leftoverPrefab;
        [SerializeField] private Transform leftoverSpawn;

        // bottom shelf
        [SerializeField] TextMeshProUGUI numChickenFeed;

        public static InventoryManager Instance { get; private set; }

        private GameObject shelfCorn;
        private GameObject shelfWheat;
        private GameObject shelfWeeds;
        private GameObject shelfChickenPoop;
        private GameObject shelfBurntWaffle;
        private GameObject shelfLeftover;

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
            if (ResourceManager.Instance != null)
                ResourceManager.Instance.OnResourceChanged -= HandleResourceChanged;
        }

        private void Start()
        {
            ResourceManager.Instance.OnResourceChanged += HandleResourceChanged;
            EnsureShelfItems();
            UpdateNums();
            //StartCoroutine(ResourcePollRoutine());
        }

        private void HandleResourceChanged(string resourceKey, int newAmount, int change)
        {
            EnsureShelfItems();
            UpdateNums();
        }

        //private IEnumerator ResourcePollRoutine()
        //{
        //    WaitForSeconds wait = new WaitForSeconds(0.2f);

        //    while (true)
        //    {
        //        EnsureShelfItems();
        //        UpdateNums();
        //        yield return wait;
        //    }
        //}
        private void EnsureShelfItems()
        {
            if (shelfCorn == null)
            {
                shelfCorn = SpawnPhysicalItem(cornPrefab, cornSpawn, "Corn", "HarvestedCorn");
            }
            if (shelfWheat == null)
            {
                shelfWheat = SpawnPhysicalItem(wheatPrefab, wheatSpawn, "Wheat", "HarvestedWheat");
            }
            if (shelfWeeds == null)
            {
                shelfWeeds = SpawnPhysicalItem(weedsPrefab, weedsSpawn, "Weeds", "Weeds");
            }

            if (shelfChickenPoop == null)
            {
                shelfChickenPoop = SpawnPhysicalItem(chickenPoopPrefab, chickenPoopSpawn, "ChickenPoop", "ChickenPoop");
            }

            if (shelfBurntWaffle == null)
            {
                shelfBurntWaffle = SpawnPhysicalItem(burntWafflePrefab, burntWaffleSpawn, "BurntWaffle", "BurntWaffle");
            }
                
            if (shelfLeftover == null)
            {
                shelfLeftover = SpawnPhysicalItem(leftoverPrefab, leftoverSpawn, "Leftover", "Leftover");
            }
        }

        public void ClearShelfReference(GameObject grabbedObj)
        {
            if (grabbedObj == shelfCorn) shelfCorn = null;
            else if (grabbedObj == shelfWheat) shelfWheat = null;
            else if (grabbedObj == shelfWeeds) shelfWeeds = null;
            else if (grabbedObj == shelfChickenPoop) shelfChickenPoop = null;
            else if (grabbedObj == shelfBurntWaffle) shelfBurntWaffle = null;
            else if (grabbedObj == shelfLeftover) shelfLeftover = null;
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

            numChickenPoop.text = ResourceManager.Instance.Get("Chicken Poop").ToString();
            numBurntWaffle.text = ResourceManager.Instance.Get("Burnt Waffle").ToString();
            numLeftover.text = ResourceManager.Instance.Get("Leftover").ToString();
            numChickenFeed.text = ResourceManager.Instance.Get("Chicken Feed").ToString();
        }
    }
}