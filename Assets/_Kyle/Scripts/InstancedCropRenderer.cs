using System.Collections.Generic;
using UnityEngine;

namespace Kyle
{
    [System.Serializable]
    public struct CropPart
    {
        public Mesh mesh;
        public int submeshIndex;
        public Material material;
    }
    public struct CropLayoutData
    {
        public float randomX;
        public float randomZ;
        public Quaternion randomRot;
    }

    // used AI to help write the code with the matrices and drawing instanced meshes with the graphics. I needed to draw
    // hundreds of individual corn/wheat/weed plants and it was impossible to run playably in VR if I used individual GameObjects.
    public class InstancedCropRenderer : MonoBehaviour
    {
        public static InstancedCropRenderer Instance { get; private set; }

        [Header("Corn")]
        public CropPart[] cornStalkParts;
        public CropPart[] cornHarvestParts;

        private List<Matrix4x4> tableCornStalkMatrices = new List<Matrix4x4>();
        private List<Matrix4x4> fieldCornStalkMatrices = new List<Matrix4x4>();
        private List<Matrix4x4> tableCornHarvestMatrices = new List<Matrix4x4>();
        private List<Matrix4x4> fieldCornHarvestMatrices = new List<Matrix4x4>();

        [Header("Wheat")]
        public CropPart[] wheatParts;

        private List<Matrix4x4> tableWheatMatrices = new List<Matrix4x4>();
        private List<Matrix4x4> fieldWheatMatrices = new List<Matrix4x4>();

        [Header("Weeds")]
        public CropPart[] weedParts;
        [SerializeField] private float weedBaseScale = 1f;
        private List<Matrix4x4> tableWeedMatrices = new List<Matrix4x4>();
        private List<Matrix4x4> fieldWeedMatrices = new List<Matrix4x4>();

        private Dictionary<Vector2Int, CropLayoutData[]> cellLayouts = new Dictionary<Vector2Int, CropLayoutData[]>();

        [Header("Ground Context")]
        public Mesh groundQuadMesh;
        public Material rockyMaterial;
        public Material soilMaterial;

        private List<Matrix4x4> rockyMatrices = new List<Matrix4x4>();
        private List<Matrix4x4> soilMatrices = new List<Matrix4x4>();
        private List<Matrix4x4> tableRockyMatrices = new List<Matrix4x4>();
        private List<Matrix4x4> tableSoilMatrices = new List<Matrix4x4>();

        [Header("Table Manual Heights (Corn)")]
        [SerializeField] private float tableStalkStartY = 0f;
        [SerializeField] private float tableStalkEndY = 0.025f;
        [SerializeField] private float tableHarvestY = 0.06f;

        [Header("Field Manual Heights (Corn)")]
        [SerializeField] private float fieldStalkStartY = 0f;
        [SerializeField] private float fieldStalkEndY = 1f;
        [SerializeField] private float fieldHarvestY = 3f;

        [Header("Table Manual Heights (Wheat)")]
        [SerializeField] private float tableWheatStartY = 0f;
        [SerializeField] private float tableWheatEndY = 0f;

        [Header("Field Manual Heights (Wheat)")]
        [SerializeField] private float fieldWheatStartY = 0f;
        [SerializeField] private float fieldWheatEndY = 0f;

        [Header("Crop Scales")]
        [SerializeField] private float cornBaseScale = 1f;
        [SerializeField] private float wheatBaseScale = 2f;

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        private void Update()
        {
            if (tableCornStalkMatrices.Count > 0)
                for (int i = 0; i < cornStalkParts.Length; i++)
                    Graphics.DrawMeshInstanced(cornStalkParts[i].mesh, cornStalkParts[i].submeshIndex, cornStalkParts[i].material, tableCornStalkMatrices.ToArray());

            if (fieldCornStalkMatrices.Count > 0)
                for (int i = 0; i < cornStalkParts.Length; i++)
                    Graphics.DrawMeshInstanced(cornStalkParts[i].mesh, cornStalkParts[i].submeshIndex, cornStalkParts[i].material, fieldCornStalkMatrices.ToArray());

            if (tableCornHarvestMatrices.Count > 0)
                for (int i = 0; i < cornHarvestParts.Length; i++)
                    Graphics.DrawMeshInstanced(cornHarvestParts[i].mesh, cornHarvestParts[i].submeshIndex, cornHarvestParts[i].material, tableCornHarvestMatrices.ToArray());

            if (fieldCornHarvestMatrices.Count > 0)
                for (int i = 0; i < cornHarvestParts.Length; i++)
                    Graphics.DrawMeshInstanced(cornHarvestParts[i].mesh, cornHarvestParts[i].submeshIndex, cornHarvestParts[i].material, fieldCornHarvestMatrices.ToArray());

            if (tableWheatMatrices.Count > 0)
                for (int i = 0; i < wheatParts.Length; i++)
                    Graphics.DrawMeshInstanced(wheatParts[i].mesh, wheatParts[i].submeshIndex, wheatParts[i].material, tableWheatMatrices.ToArray());

            if (fieldWheatMatrices.Count > 0)
                for (int i = 0; i < wheatParts.Length; i++)
                    Graphics.DrawMeshInstanced(wheatParts[i].mesh, wheatParts[i].submeshIndex, wheatParts[i].material, fieldWheatMatrices.ToArray());

            if (rockyMatrices.Count > 0)
                Graphics.DrawMeshInstanced(groundQuadMesh, 0, rockyMaterial, rockyMatrices.ToArray());
            if (soilMatrices.Count > 0)
                Graphics.DrawMeshInstanced(groundQuadMesh, 0, soilMaterial, soilMatrices.ToArray());

            if (tableRockyMatrices.Count > 0)
                Graphics.DrawMeshInstanced(groundQuadMesh, 0, rockyMaterial, tableRockyMatrices.ToArray());
            if (tableSoilMatrices.Count > 0)
                Graphics.DrawMeshInstanced(groundQuadMesh, 0, soilMaterial, tableSoilMatrices.ToArray());

            if (tableWeedMatrices.Count > 0)
                for (int i = 0; i < weedParts.Length; i++)
                    Graphics.DrawMeshInstanced(weedParts[i].mesh, weedParts[i].submeshIndex, weedParts[i].material, tableWeedMatrices.ToArray());
            if (fieldWeedMatrices.Count > 0)
                for (int i = 0; i < weedParts.Length; i++)
                    Graphics.DrawMeshInstanced(weedParts[i].mesh, weedParts[i].submeshIndex, weedParts[i].material, fieldWeedMatrices.ToArray());
        }

        public void PlantCellCrops(Vector2Int cell, int stalksPerCell)
        {
            CropLayoutData[] layout = new CropLayoutData[stalksPerCell];
            for (int i = 0; i < stalksPerCell; i++)
            {
                layout[i] = new CropLayoutData
                {
                    randomX = Random.Range(-0.4f, 0.4f),
                    randomZ = Random.Range(-0.4f, 0.4f),
                    randomRot = Quaternion.Euler(0, Random.Range(0, 360), 0)
                };
            }
            cellLayouts[cell] = layout;
        }

        public void RebuildMatricesFromState(Dictionary<Vector2Int, CropData> currentStates)
        {
            tableCornStalkMatrices.Clear();
            fieldCornStalkMatrices.Clear();
            tableCornHarvestMatrices.Clear();
            fieldCornHarvestMatrices.Clear();
            tableWheatMatrices.Clear();
            fieldWheatMatrices.Clear();
            tableWeedMatrices.Clear();
            fieldWeedMatrices.Clear();

            float tSize = HighlightSync.Instance.tableCellSize;
            float fSize = HighlightSync.Instance.fieldCellSize;

            Transform tableOrigin = HighlightSync.Instance.tableOrigin;
            Transform fieldOrigin = HighlightSync.Instance.fieldOrigin;

            foreach (var kvp in currentStates)
            {
                Vector2Int cell = kvp.Key;
                CropData crop = kvp.Value;

                if (crop.state == CellState.Empty || !cellLayouts.ContainsKey(cell))
                    continue;

                float growthPercent = crop.growth / 100f;
                float currentYScale = Mathf.Lerp(0.1f, 1.0f, growthPercent);

                if (crop.state == CellState.CornReadyForHarvest || crop.state == CellState.WheatReadyForHarvest)
                    currentYScale = 1.0f;

                float activeBaseScale = cornBaseScale;
                if (crop.state == CellState.WheatPlanted || crop.state == CellState.WheatReadyForHarvest)
                    activeBaseScale = wheatBaseScale;
                else if (crop.state == CellState.Weedy)
                    activeBaseScale = weedBaseScale;

                float fieldScaleMultiplier = activeBaseScale;
                float tableScaleMultiplier = 2 * fieldScaleMultiplier * (tSize / fSize);

                Vector3 tableScale = new Vector3(tableScaleMultiplier, tableScaleMultiplier * currentYScale, tableScaleMultiplier);
                Vector3 fieldScale = new Vector3(fieldScaleMultiplier, fieldScaleMultiplier * currentYScale, fieldScaleMultiplier);

                float tablePivotShift = 0f;
                float fieldPivotShift = 0f;

                if (crop.state == CellState.CornPlanted || crop.state == CellState.CornReadyForHarvest)
                {
                    if (crop.state == CellState.CornReadyForHarvest)
                    {
                        tablePivotShift = tableHarvestY;
                        fieldPivotShift = fieldHarvestY;
                    }
                    else
                    {
                        tablePivotShift = Mathf.Lerp(tableStalkStartY, tableStalkEndY, growthPercent);
                        fieldPivotShift = Mathf.Lerp(fieldStalkStartY, fieldStalkEndY, growthPercent);
                    }

                    Vector3 tableCenter = tableOrigin.position + (tableOrigin.right * cell.x * tSize) + (tableOrigin.forward * cell.y * tSize) + (tableOrigin.up * tablePivotShift);
                    Vector3 fieldCenter = fieldOrigin.position + (fieldOrigin.right * cell.x * fSize) + (fieldOrigin.forward * cell.y * fSize) + (fieldOrigin.up * fieldPivotShift);

                    foreach (CropLayoutData layout in cellLayouts[cell])
                    {
                        Vector3 tablePos = tableCenter + (tableOrigin.right * layout.randomX * tSize) + (tableOrigin.forward * layout.randomZ * tSize);
                        Matrix4x4 tableMatrix = Matrix4x4.TRS(tablePos, layout.randomRot, tableScale);

                        Vector3 fieldPos = fieldCenter + (fieldOrigin.right * layout.randomX * fSize) + (fieldOrigin.forward * layout.randomZ * fSize);
                        Matrix4x4 fieldMatrix = Matrix4x4.TRS(fieldPos, layout.randomRot, fieldScale);

                        if (crop.state == CellState.CornReadyForHarvest)
                        {
                            tableCornHarvestMatrices.Add(tableMatrix);
                            fieldCornHarvestMatrices.Add(fieldMatrix);
                        }
                        else
                        {
                            tableCornStalkMatrices.Add(tableMatrix);
                            fieldCornStalkMatrices.Add(fieldMatrix);
                        }
                    }
                }
                else if (crop.state == CellState.WheatPlanted || crop.state == CellState.WheatReadyForHarvest)
                {
                    tablePivotShift = Mathf.Lerp(tableWheatStartY, tableWheatEndY, growthPercent);
                    fieldPivotShift = Mathf.Lerp(fieldWheatStartY, fieldWheatEndY, growthPercent);

                    Vector3 tableCenter = tableOrigin.position + (tableOrigin.right * cell.x * tSize) + (tableOrigin.forward * cell.y * tSize) + (tableOrigin.up * tablePivotShift);
                    Vector3 fieldCenter = fieldOrigin.position + (fieldOrigin.right * cell.x * fSize) + (fieldOrigin.forward * cell.y * fSize) + (fieldOrigin.up * fieldPivotShift);

                    foreach (CropLayoutData layout in cellLayouts[cell])
                    {
                        Vector3 tablePos = tableCenter + (tableOrigin.right * layout.randomX * tSize) + (tableOrigin.forward * layout.randomZ * tSize);
                        Matrix4x4 tableMatrix = Matrix4x4.TRS(tablePos, layout.randomRot, tableScale);

                        Vector3 fieldPos = fieldCenter + (fieldOrigin.right * layout.randomX * fSize) + (fieldOrigin.forward * layout.randomZ * fSize);
                        Matrix4x4 fieldMatrix = Matrix4x4.TRS(fieldPos, layout.randomRot, fieldScale);

                        tableWheatMatrices.Add(tableMatrix);
                        fieldWheatMatrices.Add(fieldMatrix);
                    }
                }
                else if (crop.state == CellState.Weedy)
                {
                    Vector3 tableCenter = tableOrigin.position + (tableOrigin.right * cell.x * tSize) + (tableOrigin.forward * cell.y * tSize);
                    Vector3 fieldCenter = fieldOrigin.position + (fieldOrigin.right * cell.x * fSize) + (fieldOrigin.forward * cell.y * fSize) + (fieldOrigin.up * 0.01f);

                    Vector3 weedTableScale = new Vector3(tableScaleMultiplier, tableScaleMultiplier, tableScaleMultiplier);
                    Vector3 weedFieldScale = new Vector3(fieldScaleMultiplier, fieldScaleMultiplier, fieldScaleMultiplier);

                    foreach (CropLayoutData layout in cellLayouts[cell])
                    {
                        Vector3 tablePos = tableCenter + (tableOrigin.right * layout.randomX * tSize) + (tableOrigin.forward * layout.randomZ * tSize);
                        Matrix4x4 tableMatrix = Matrix4x4.TRS(tablePos, layout.randomRot, weedTableScale);

                        Vector3 fieldPos = fieldCenter + (fieldOrigin.right * layout.randomX * fSize) + (fieldOrigin.forward * layout.randomZ * fSize);
                        Matrix4x4 fieldMatrix = Matrix4x4.TRS(fieldPos, layout.randomRot, weedFieldScale);

                        tableWeedMatrices.Add(tableMatrix);
                        fieldWeedMatrices.Add(fieldMatrix);
                    }
                }
            }
        }

        public void RebuildGroundMatrices(Dictionary<Vector2Int, CropData> currentStates)
        {
            rockyMatrices.Clear();
            soilMatrices.Clear();
            tableRockyMatrices.Clear();
            tableSoilMatrices.Clear();

            int cols = HighlightSync.Instance.gridColumns;
            int rows = HighlightSync.Instance.gridRows;
            float fSize = HighlightSync.Instance.fieldCellSize;
            Transform fieldOrigin = HighlightSync.Instance.fieldOrigin;

            float tSize = HighlightSync.Instance.tableCellSize;
            Transform tableOrigin = HighlightSync.Instance.tableOrigin;

            Quaternion flatRotation = fieldOrigin.rotation * Quaternion.Euler(90, 0, 0);
            Vector3 quadScale = new Vector3(fSize, fSize, 1f);

            Quaternion tableFlatRot = tableOrigin.rotation * Quaternion.Euler(90, 0, 0);
            Vector3 tableQuadScale = new Vector3(tSize, tSize, 1f);

            for (int x = 0; x < cols; x++)
            {
                for (int z = 0; z < rows; z++)
                {
                    Vector2Int cell = new Vector2Int(x, z);

                    GroundState gState = GroundState.Rocky;
                    if (currentStates.TryGetValue(cell, out CropData crop))
                    {
                        gState = crop.ground;
                    }

                    Vector3 cellPos = fieldOrigin.position
                        + (fieldOrigin.right * x * fSize)
                        + (fieldOrigin.forward * z * fSize)
                        + (fieldOrigin.up * 0.01f);
                    Matrix4x4 matrix = Matrix4x4.TRS(cellPos, flatRotation, quadScale);

                    Vector3 tablePos = tableOrigin.position
                        + (tableOrigin.right * x * tSize)
                        + (tableOrigin.forward * z * tSize)
                        + (tableOrigin.up * 0.001f);
                    Matrix4x4 tableMatrix = Matrix4x4.TRS(tablePos, tableFlatRot, tableQuadScale);

                    if (gState == GroundState.Rocky)
                    {
                        rockyMatrices.Add(matrix);
                        tableRockyMatrices.Add(tableMatrix);
                    }
                    else
                    {
                        soilMatrices.Add(matrix);
                        tableSoilMatrices.Add(tableMatrix);
                    }
                }
            }
        }
    }
}