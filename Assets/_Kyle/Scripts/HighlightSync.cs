using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Kyle
{
    [RequireComponent(typeof(XRSimpleInteractable))]
    public class HighlightSync : MonoBehaviour, IXRHoverFilter
    {
        public static HighlightSync Instance { get; private set; }

        public Transform tableOrigin;
        public float tableCellSize;

        public Transform fieldOrigin;
        public float fieldCellSize;

        [SerializeField] Transform tableHighlightMesh;
        [SerializeField] Transform fieldBeaconMesh;

        public int gridColumns;
        public int gridRows;

        [SerializeField] float maxGrabDistance = 2f;

        private XRSimpleInteractable interactable;

        public bool canProcess => true;

        [Header("Highlight Materials")]
        [SerializeField] private Material normalFieldHighlightMaterial;
        [SerializeField] private Material normalTableHighlightMaterial;
        [SerializeField] private Material redFieldHighlightMaterial;
        [SerializeField] private Material redTableHighlightMaterial;

        [SerializeField] private MeshRenderer tableHighlightRenderer;
        [SerializeField] private MeshRenderer fieldBeaconRenderer;

        // for multiple highlights
        [SerializeField] GameObject highlightPrefab;
        [SerializeField] Transform highlightParent;

        private readonly Dictionary<Vector2Int, GameObject> selectedHighlights = new Dictionary<Vector2Int, GameObject>();
        private Vector2Int currentHoverCell = new Vector2Int(-1, -1);

        public Vector2Int CurrentToolHoverCell { get; private set; } = new Vector2Int(-1, -1);

        private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
        [SerializeField] private float hitCooldown = 0.02f;
        private float nextAllowedHitTime = 0f;
        private float particlePersistTimer = 0f;
        private Vector2Int waterHoverCell = new Vector2Int(-1, -1);
        private float waterHoverTimer = 0f;

        public InputActionReference leftTrigger;
        public InputActionReference rightTrigger;

        // dragging stuff
        private bool isDragging = false;
        private bool isAdding = true; // true if painting highlights, false if erasing
        private Vector2Int lastProcessedCell = new Vector2Int(-1, -1);

        private void Awake()
        {
            if (Instance == null) { Instance = this; }
            interactable = GetComponent<XRSimpleInteractable>();
            interactable.selectMode = InteractableSelectMode.Multiple;
            interactable.hoverFilters.Add(this);
        }

        private void OnEnable()
        {
            leftTrigger.action.Enable();
            rightTrigger.action.Enable();

            leftTrigger.action.started += OnTriggerPressed;
            rightTrigger.action.started += OnTriggerPressed;

            leftTrigger.action.canceled += OnTriggerReleased;
            rightTrigger.action.canceled += OnTriggerReleased;
            //interactable.selectEntered.AddListener(OnTableClicked);
        }
        private void OnDisable()
        {
            leftTrigger.action.Disable();
            rightTrigger.action.Disable();

            leftTrigger.action.started -= OnTriggerPressed;
            rightTrigger.action.started -= OnTriggerPressed;
            leftTrigger.action.canceled -= OnTriggerReleased;
            rightTrigger.action.canceled -= OnTriggerReleased;
        }
        private void OnTriggerPressed(InputAction.CallbackContext context)
        {
            isDragging = true;

            Vector2Int activeCell = currentHoverCell.x >= 0 ? currentHoverCell : CurrentToolHoverCell;

            if (activeCell.x >= 0)
            {
                isAdding = !selectedHighlights.ContainsKey(activeCell);
                ProcessDragCell(activeCell);
            }
        }

        private void OnTriggerReleased(InputAction.CallbackContext context)
        {
            isDragging = false;
            lastProcessedCell = new Vector2Int(-1, -1);
        }
        private bool CellNeedsAttention(Vector2Int cell)
        {
            if (cell.x < 0) return false;

            CropData crop = FieldActionManager.Instance.GetCropData(cell);

            if (crop.state == CellState.Weedy) { return false; }

            bool needsWater = (crop.state == CellState.CornPlanted || crop.state == CellState.WheatPlanted) && crop.water <= 0f;

            bool needsFertilizer = crop.ground == GroundState.Rocky;

            return needsWater || needsFertilizer;
        }

        private void ProcessDragCell(Vector2Int cell)
        {
            if (cell == lastProcessedCell || cell.x < 0) return;

            if (isAdding)
            {
                if (!selectedHighlights.ContainsKey(cell))
                {
                    Vector3 tablePos = tableOrigin.position
                        + (tableOrigin.right * cell.x * tableCellSize)
                        + (tableOrigin.forward * cell.y * tableCellSize)
                        + (tableOrigin.up * 0.004f);

                    GameObject instance = Instantiate(highlightPrefab, tablePos + new Vector3(0, 0.074f, 0), tableOrigin.rotation, highlightParent);
                    selectedHighlights.Add(cell, instance);
                }
            }
            else
            {
                if (selectedHighlights.TryGetValue(cell, out GameObject existing))
                {
                    Destroy(existing);
                    selectedHighlights.Remove(cell);
                }
            }

            lastProcessedCell = cell;
        }

        //private void TogglePersistentHighlight(Vector2Int cell)
        //{
        //    // delete highlight
        //    if (selectedHighlights.TryGetValue(cell, out GameObject existing))
        //    {
        //        Destroy(existing);
        //        selectedHighlights.Remove(cell);
        //        return;
        //    }

        //    Vector3 tablePos = tableOrigin.position 
        //        + (tableOrigin.right * cell.x * tableCellSize)
        //        + (tableOrigin.forward * cell.y * tableCellSize)
        //        + (tableOrigin.up * 0.002f);

        //    GameObject instance = Instantiate(highlightPrefab, tablePos + new Vector3(0, 0.074f, 0), tableOrigin.rotation, highlightParent);
        //    selectedHighlights.Add(cell, instance);
        //}

        public IReadOnlyCollection<Vector2Int> GetSelectedCells()
        {
            return selectedHighlights.Keys;
        }

        public void ClearAllHighlights()
        {
            foreach (var highlight in selectedHighlights.Values)
            {
                Destroy(highlight);
            }
            selectedHighlights.Clear();
        }

        public void UpdateToolHover(Vector3 hit)
        {
            if (selectedHighlights.Count > 0)
            {
                CurrentToolHoverCell = new Vector2Int(-1, -1);
                return;
            }

            Vector3 offset = hit - tableOrigin.position;
            int x = Mathf.FloorToInt((Vector3.Dot(offset, tableOrigin.right) / tableCellSize) + 0.5f);
            int z = Mathf.FloorToInt((Vector3.Dot(offset, tableOrigin.forward) / tableCellSize) + 0.5f);

            if (x >= 0 && x < gridColumns && z >= 0 && z < gridRows)
            {
                CurrentToolHoverCell = new Vector2Int(x, z);

                if (isDragging) ProcessDragCell(CurrentToolHoverCell);

                Vector3 tablePos = tableOrigin.position + (tableOrigin.right * x * tableCellSize) + (tableOrigin.forward * z * tableCellSize);
                //tableHighlightMesh.position = tablePos + (tableOrigin.up * 0.003f) + new Vector3(0, 0.074f, 0);
                //tableHighlightMesh.gameObject.SetActive(true);

                //Vector3 fieldPos = fieldOrigin.position + (fieldOrigin.right * x * fieldCellSize) + (fieldOrigin.forward * z * fieldCellSize);
                //fieldBeaconMesh.position = fieldPos + (fieldOrigin.up * 0.02f) + new Vector3(0, 4.5307f, 0);
                //fieldBeaconMesh.gameObject.SetActive(true);
            }
            else
            {
                ClearToolHover();
            }
        }

        public void ClearToolHover()
        {
            CurrentToolHoverCell = new Vector2Int(-1, -1);
            if (selectedHighlights.Count == 0 && currentHoverCell.x < 0)
            {
                tableHighlightMesh.gameObject.SetActive(false);
                fieldBeaconMesh.gameObject.SetActive(false);
            }
        }

        public bool Process(IXRHoverInteractor interactor, IXRHoverInteractable interactable)
        {
            float distance = Vector3.Distance(interactor.transform.position, transform.position);
            return distance <= maxGrabDistance;
        }

        private void Update()
        {
            if (waterHoverTimer > 0)
            {
                waterHoverTimer -= Time.deltaTime;
                if (waterHoverTimer <= 0) waterHoverCell = new Vector2Int(-1, -1);
            }

            if (interactable.isHovered && interactable.interactorsHovering.Count > 0)
            {
                var interactor = interactable.interactorsHovering[0];
                if (Vector3.Distance(interactor.transform.position, transform.position) > maxGrabDistance)
                {
                    TurnOffHighlights();
                }
                else
                {
                    Vector3 targetPoint = Vector3.zero;
                    bool pointFound = false;

                    Ray pointerRay = new Ray(interactor.transform.position, interactor.transform.forward);
                    Collider tableCollider = GetComponent<Collider>();

                    if (tableCollider.Raycast(pointerRay, out RaycastHit manualHit, 10f))
                    {
                        targetPoint = manualHit.point;
                        pointFound = true;
                    }
                    else
                    {
                        Plane tablePlane = new Plane(tableOrigin.up, tableOrigin.position);

                        if (tablePlane.Raycast(pointerRay, out float enter))
                        {
                            Vector3 planePoint = pointerRay.GetPoint(enter);
                            targetPoint = tableCollider.ClosestPoint(planePoint);
                            pointFound = true;
                        }
                        else
                        {
                            targetPoint = tableCollider.ClosestPoint(interactor.transform.position);
                            pointFound = true;
                        }
                    }

                    if (pointFound)
                    {
                        UpdateHighlights(targetPoint);
                    }
                    else
                    {
                        TurnOffHighlights();
                    }
                }
            }
            else
            {
                TurnOffHighlights();
            }

            DrawHighlightMeshes(GetCurrentHoverCell());
        }

        private void UpdateHighlights(Vector3 hit)
        {
            Vector3 offset = hit - tableOrigin.position;
            float distanceX = Vector3.Dot(offset, tableOrigin.right);
            float distanceZ = Vector3.Dot(offset, tableOrigin.forward);

            int x = Mathf.FloorToInt((distanceX / tableCellSize) + 0.5f);
            int z = Mathf.FloorToInt((distanceZ / tableCellSize) + 0.5f);

            if (x < 0 || x >= gridColumns || z < 0 || z >= gridRows)
            {
                TurnOffHighlights();
                return;
            }

            currentHoverCell = new Vector2Int(x, z);
            if (isDragging) ProcessDragCell(currentHoverCell);
        }

        private void TurnOffHighlights()
        {
            currentHoverCell = new Vector2Int(-1, -1);
        }

        public Vector2Int GetCurrentHoverCell()
        {
            if (CurrentToolHoverCell.x >= 0) return CurrentToolHoverCell;
            if (currentHoverCell.x >= 0) return currentHoverCell;
            return waterHoverCell;
        }

        private void DrawHighlightMeshes(Vector2Int cell)
        {
            if (cell.x < 0)
            {
                tableHighlightMesh.gameObject.SetActive(false);
                fieldBeaconMesh.gameObject.SetActive(false);
                return;
            }

            Vector3 tablePos = tableOrigin.position + (tableOrigin.right * cell.x * tableCellSize) + (tableOrigin.forward * cell.y * tableCellSize);
            Vector3 fieldPos = fieldOrigin.position + (fieldOrigin.right * cell.x * fieldCellSize) + (fieldOrigin.forward * cell.y * fieldCellSize);

            tableHighlightMesh.position = tablePos + (tableOrigin.up * 0.003f) + new Vector3(0, 0.074f, 0);
            fieldBeaconMesh.position = fieldPos + (fieldOrigin.up * 0.02f) + new Vector3(0, 4.5307f, 0);

            tableHighlightMesh.gameObject.SetActive(true);
            fieldBeaconMesh.gameObject.SetActive(true);

            bool needsAttention = CellNeedsAttention(cell);
            Material tableMaterial = needsAttention
                ? redTableHighlightMaterial
                : normalTableHighlightMaterial;

            Material fieldMaterial = needsAttention
                ? redFieldHighlightMaterial
                : normalFieldHighlightMaterial;

            if (tableHighlightRenderer != null && tableMaterial != null)
            {
                tableHighlightRenderer.material = tableMaterial;
            }
            if (fieldBeaconRenderer != null && fieldMaterial != null)
            {
                fieldBeaconRenderer.material = fieldMaterial;
            }
        }

        private void OnParticleCollision(GameObject other)
        {
            if (Time.time < nextAllowedHitTime) return;

            ParticleSystem ps = other.GetComponent<ParticleSystem>();
            if (ps != null && ps.GetCollisionEvents(this.gameObject, collisionEvents) > 0)
            {
                Vector3 hitPoint = collisionEvents[0].intersection;

                Vector3 offset = hitPoint - tableOrigin.position;
                int x = Mathf.FloorToInt((Vector3.Dot(offset, tableOrigin.right) / tableCellSize) + 0.5f);
                int z = Mathf.FloorToInt((Vector3.Dot(offset, tableOrigin.forward) / tableCellSize) + 0.5f);

                if (x >= 0 && x < gridColumns && z >= 0 && z < gridRows)
                {
                    nextAllowedHitTime = Time.time + hitCooldown;
                    Vector2Int hitCell = new Vector2Int(x, z);

                    waterHoverCell = hitCell;
                    waterHoverTimer = 0.2f;

                    FieldActionManager.Instance.ExecuteActionOnCell(hitCell, FarmingTool.Water);
                }
            }
        }

        public Vector2Int GetCellForUI()
        {
            // hide ui if multiselect
            if (selectedHighlights.Count > 1)
            {
                return new Vector2Int(-1, -1);
            }

            // persist ui if one cell selected
            if (selectedHighlights.Count == 1)
            {
                foreach (Vector2Int cell in selectedHighlights.Keys)
                {
                    return cell;
                }
            }

            // prio tool location
            if (CurrentToolHoverCell.x >= 0)
            {
                return CurrentToolHoverCell;
            }

            // follow controller pointer
            return GetCurrentHoverCell();
        }

        private void Start()
        {
            GenerateCellColliders();
        }

        public void GenerateCellColliders()
        {
            GameObject colliderHolder = new GameObject("TableCellCollidersContainer");
            colliderHolder.transform.SetParent(tableOrigin);
            colliderHolder.transform.localPosition = Vector3.zero;
            colliderHolder.transform.localRotation = Quaternion.identity;

            for (int x = 0; x < gridColumns; x++)
            {
                for (int z = 0; z < gridRows; z++)
                {
                    GameObject cellObj = new GameObject($"Cell_Collider_{x}_{z}");
                    cellObj.transform.SetParent(colliderHolder.transform);

                    Vector3 localPos = new Vector3((x * tableCellSize), 0.02f, (z * tableCellSize));
                    cellObj.transform.localPosition = localPos;
                    cellObj.transform.localRotation = Quaternion.identity;

                    BoxCollider box = cellObj.AddComponent<BoxCollider>();
                    box.size = new Vector3(tableCellSize * 0.9f, 0.1f, tableCellSize * 0.9f);
                    box.isTrigger = true;

                    FieldCellCollider cellCollider = cellObj.AddComponent<FieldCellCollider>();
                    cellCollider.Initialize(new Vector2Int(x, z));
                }
            }
        }
    }
}