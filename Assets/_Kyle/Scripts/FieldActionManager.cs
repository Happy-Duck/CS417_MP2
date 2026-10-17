using System.Collections.Generic;
using UnityEngine;
using Shared;

namespace Kyle
{
    public enum FarmingTool
    {
        None,
        PlantCorn,
        PlantWheat,
        Fertilize,
        Harvest,
        Water,
        Hoe
    }
    public enum CellState
    {
        Empty,
        Fertilized,
        CornPlanted,
        CornReadyForHarvest,
        WheatPlanted,
        WheatReadyForHarvest,
        Weedy
    }
    public enum GroundState
    {
        Rocky,
        Soil
    }
    public class CropData
    {
        public CellState state = CellState.Empty;
        public GroundState ground = GroundState.Rocky;
        public float growth = 0f;
        public float water = 0f;
        public float lastWateredTime = 0f;
        public float fertilization = 0f;
    }

    public class FieldActionManager : MonoBehaviour
    {
        public static FieldActionManager Instance { get; private set; }
        private readonly Dictionary<Vector2Int, CropData> activeCrops = new Dictionary<Vector2Int, CropData>();

        //[SerializeField] private CropVisuals cornPrefab;
        //[SerializeField] private CropVisuals wheatPrefab;

        //private readonly Dictionary<Vector2Int, GameObject> activeCrops = new Dictionary<Vector2Int, GameObject>();

        private float weedSpawnTimer = 0f;
        private float nextWeedSpawnInterval = 20f;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }
        private void Start()
        {
            ResourceManager.Instance.Add("CornSeed", 10);
            ResourceManager.Instance.Add("WheatSeed", 10);
            ResourceManager.Instance.Add("Fertilizer", 20);

            InventoryManager.Instance.UpdateNums();

            SpawnInitialWeeds(5);

            InstancedCropRenderer.Instance.RebuildGroundMatrices(activeCrops);
            nextWeedSpawnInterval = Random.Range(20f, 60f);
        }

        private void Update()
        {
            bool needsVisualUpdate = false;
            bool needsGroundUpdate = false;

            // random weed spawner
            weedSpawnTimer += Time.deltaTime;
            if (weedSpawnTimer >= nextWeedSpawnInterval)
            {
                weedSpawnTimer = 0f;
                nextWeedSpawnInterval = Random.Range(20f, 60f);
                TrySpawnRandomWeed();
                needsVisualUpdate = true;
            }

            HashSet<Vector2Int> affectedByWeeds = new HashSet<Vector2Int>();
            foreach (var kvp in activeCrops)
            {
                if (kvp.Value.state == CellState.Weedy)
                {
                    Vector2Int weedCell = kvp.Key;
                    for (int x = weedCell.x - 2; x <= weedCell.x + 2; x++)
                    {
                        for (int z = weedCell.y - 2; z <= weedCell.y + 2; z++)
                        {
                            affectedByWeeds.Add(new Vector2Int(x, z));
                        }
                    }
                }
            }

            foreach (var kvp in activeCrops)
            {
                Vector2Int cell = kvp.Key;
                CropData crop = kvp.Value;

                // drain water
                if (crop.water > 0 && Time.time > crop.lastWateredTime + 0.5f)
                {
                    crop.water -= (100f / 120f) * Time.deltaTime;
                    if (crop.water < 0) crop.water = 0;
                }

                bool isActivelyGrowing = (crop.state == CellState.CornPlanted || crop.state == CellState.WheatPlanted) && crop.growth < 100f;
                if (isActivelyGrowing)
                {
                    // drain fertilizer
                    if (crop.ground == GroundState.Soil)
                    {
                        float drainRate = 100f / 180f;
                        if (affectedByWeeds.Contains(cell))
                        {
                            drainRate += 2f;
                        }

                        crop.fertilization -= drainRate * Time.deltaTime;

                        if (crop.fertilization <= 0)
                        {
                            crop.fertilization = 0f;
                            crop.ground = GroundState.Rocky;
                            crop.water = 0f;
                            needsGroundUpdate = true;
                        }
                    }
                    // only grow with water
                    if (crop.water > 0)
                    {
                        float growthRate = 100f / 120f;
                        if (affectedByWeeds.Contains(cell))
                        {
                            growthRate *= 0.5f;
                        }

                        crop.growth += growthRate * Time.deltaTime;
                        needsVisualUpdate = true;

                        if (crop.growth >= 100f)
                        {
                            crop.growth = 100f;

                            if (crop.state == CellState.CornPlanted)
                                crop.state = CellState.CornReadyForHarvest;
                            else if (crop.state == CellState.WheatPlanted)
                                crop.state = CellState.WheatReadyForHarvest;
                        }
                    }
                }
            }

            if (needsVisualUpdate)
            {
                InstancedCropRenderer.Instance.RebuildMatricesFromState(activeCrops);
            }
            if (needsGroundUpdate) 
            {
                InstancedCropRenderer.Instance.RebuildGroundMatrices(activeCrops); 
            }
        }

        public void ExecuteToolDrop(FarmingToolScript droppedTool)
        {
            var selectedCells = HighlightSync.Instance.GetSelectedCells();

            if (selectedCells.Count > 0)
            {
                foreach (Vector2Int cell in selectedCells)
                {
                    ExecuteActionOnCell(cell, droppedTool.toolAction);
                }
                HighlightSync.Instance.ClearAllHighlights();
            }
            else
            {
                Vector2Int hoverCell = HighlightSync.Instance.CurrentToolHoverCell;
                if (hoverCell.x >= 0)
                {
                    ExecuteActionOnCell(hoverCell, droppedTool.toolAction);
                }
            }
            //Destroy(droppedTool.gameObject);
        }

        public void ExecuteActionOnCell(Vector2Int cell, FarmingTool action)
        {
            CropData crop = GetCropData(cell);

            // restrict weedy cells
            if (crop.state == CellState.Weedy && action != FarmingTool.Hoe)
            {
                return;
            }

            switch (action)
            {
                case FarmingTool.PlantCorn:
                    if (crop.ground == GroundState.Soil && crop.state == CellState.Empty && ResourceManager.Instance.TrySpend("CornSeed", 1))
                    {
                        crop.state = CellState.CornPlanted;
                        crop.growth = 0f;
                        //crop.water = 0f;

                        if (!activeCrops.ContainsKey(cell)) activeCrops.Add(cell, crop);

                        InstancedCropRenderer.Instance.PlantCellCrops(cell, 30);
                        InstancedCropRenderer.Instance.RebuildMatricesFromState(activeCrops);
                    }
                    break;

                case FarmingTool.Water:
                    if (crop.state != CellState.Empty)
                    {
                        crop.water += 10f;
                        if (crop.water > 100f) crop.water = 100f;
                        crop.lastWateredTime = Time.time;
                    }
                    break;

                case FarmingTool.PlantWheat:
                    if (crop.ground == GroundState.Soil && crop.state == CellState.Empty && ResourceManager.Instance.TrySpend("WheatSeed", 1))
                    {
                        crop.state = CellState.WheatPlanted;
                        crop.growth = 0f;
                        //crop.water = 0f;

                        if (!activeCrops.ContainsKey(cell)) activeCrops.Add(cell, crop);

                        InstancedCropRenderer.Instance.PlantCellCrops(cell, 30);
                        InstancedCropRenderer.Instance.RebuildMatricesFromState(activeCrops);
                    }
                    break;

                case FarmingTool.Fertilize:
                    if (crop.fertilization < 100 && ResourceManager.Instance.TrySpend("Fertilizer", 1))
                    {
                        crop.ground = GroundState.Soil;
                        crop.fertilization = 100f;

                        if (!activeCrops.ContainsKey(cell))
                        {
                            activeCrops.Add(cell, crop);
                        }

                        InstancedCropRenderer.Instance.RebuildGroundMatrices(activeCrops);
                    }
                    break;

                case FarmingTool.Hoe:
                    if (crop.state == CellState.Weedy)
                    {
                        crop.state = CellState.Empty;
                        crop.ground = GroundState.Rocky;
                        crop.growth = 0f;
                        crop.water = 0f;
                        crop.fertilization = 0f;

                        ResourceManager.Instance.Add("Weeds", 4);

                        InventoryManager.Instance.UpdateNums();

                        InstancedCropRenderer.Instance.RebuildMatricesFromState(activeCrops);
                        InstancedCropRenderer.Instance.RebuildGroundMatrices(activeCrops);
                    }
                    break;

                case FarmingTool.Harvest:
                    if (crop.state == CellState.CornReadyForHarvest)
                    {
                        ResourceManager.Instance.Add("HarvestedCorn", 3);
                        InventoryManager.Instance.UpdateNums();
                        crop.state = CellState.Empty;
                        crop.growth = 0f;
                        InstancedCropRenderer.Instance.RebuildMatricesFromState(activeCrops);
                    }
                    else if (crop.state == CellState.WheatReadyForHarvest)
                    {
                        ResourceManager.Instance.Add("HarvestedWheat", 3);
                        InventoryManager.Instance.UpdateNums();

                        crop.state = CellState.Empty;
                        crop.growth = 0f;

                        InstancedCropRenderer.Instance.RebuildMatricesFromState(activeCrops);
                    }
                    break;
            }
            InventoryManager.Instance?.UpdateNums();
            //PlayActionAnim(cell, action);
        }

        //private void SpawnCropVisuals(Vector2Int cell, CropVisuals prefab)
        //{
        //    CropVisuals newCrop = Instantiate(prefab, Vector3.zero, Quaternion.identity);

        //    if (newCrop.Plant(cell))
        //    {
        //        activeCrops.Add(cell, newCrop.gameObject);
        //    }
        //    else
        //    {
        //        Destroy(newCrop.gameObject);
        //    }
        //}

        public CropData GetCropData(Vector2Int cell)
        {
            if (activeCrops.TryGetValue(cell, out CropData data)) return data;
            return new CropData { state = CellState.Empty };
        }

        private void SpawnInitialWeeds(int count)
        {
            int spawned = 0;
            int cols = HighlightSync.Instance.gridColumns;
            int rows = HighlightSync.Instance.gridRows;

            while (spawned < count)
            {
                int rx = Random.Range(0, cols);
                int rz = Random.Range(0, rows);
                Vector2Int cell = new Vector2Int(rx, rz);

                if (!activeCrops.ContainsKey(cell) || activeCrops[cell].state == CellState.Empty)
                {
                    CropData crop = GetCropData(cell);
                    crop.state = CellState.Weedy;
                    if (!activeCrops.ContainsKey(cell)) activeCrops.Add(cell, crop);

                    InstancedCropRenderer.Instance.PlantCellCrops(cell, 15);
                    spawned++;
                }
            }
            InstancedCropRenderer.Instance.RebuildMatricesFromState(activeCrops);
        }

        private void TrySpawnRandomWeed()
        {
            int cols = HighlightSync.Instance.gridColumns;
            int rows = HighlightSync.Instance.gridRows;
            List<Vector2Int> emptyCells = new List<Vector2Int>();

            for (int x = 0; x < cols; x++)
            {
                for (int z = 0; z < rows; z++)
                {
                    Vector2Int cell = new Vector2Int(x, z);
                    if (!activeCrops.ContainsKey(cell) || activeCrops[cell].state == CellState.Empty)
                    {
                        emptyCells.Add(cell);
                    }
                }
            }

            if (emptyCells.Count > 0)
            {
                Vector2Int target = emptyCells[Random.Range(0, emptyCells.Count)];
                CropData crop = GetCropData(target);
                crop.state = CellState.Weedy;
                if (!activeCrops.ContainsKey(target)) activeCrops.Add(target, crop);

                InstancedCropRenderer.Instance.PlantCellCrops(target, 15);
                InstancedCropRenderer.Instance.RebuildMatricesFromState(activeCrops);
            }
        }
    }
}