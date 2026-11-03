using UnityEngine;
using System.Collections;
using Shared;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Kyle
{
    public enum AutomationAction
    {
        Water,
        Fertilize
    }
    public class PlaceableAutomation : MonoBehaviour
    {
        public AutomationAction actionType;
        public int radius = 2;
        public float tickInterval = 60f;

        public Transform visualChild;
        [SerializeField] float miniScale = 0.1f;
        [SerializeField] float shrinkSpeed = 5f;
        public ParticleSystem triggerEffect;

        public static System.Collections.Generic.Dictionary<Vector2Int, PlaceableAutomation> placedAutomations = new System.Collections.Generic.Dictionary<Vector2Int, PlaceableAutomation>();
        public bool isPlacedOnGrid = false;
        public ErrorCatchingHint errorHint;

        private Vector3 originalScale;
        private Coroutine scaleRoutine;
        private Coroutine automationRoutine;

        private XRGrabInteractable grab;
        private Rigidbody rb;
        private bool isHeld = false;
        private bool isOverTable = false;

        private Vector3 startPosition;
        private Quaternion startRotation;
        private Vector2Int currentGridCell;

        private void Awake()
        {
            if (visualChild != null)
            {
                originalScale = visualChild.localScale;
            }

            grab = GetComponent<XRGrabInteractable>();
            rb = GetComponent<Rigidbody>();

            startPosition = transform.position;
            startRotation = transform.rotation;

            grab.selectEntered.AddListener(OnGrabbed);
            grab.selectExited.AddListener(OnDropped);
        }
        private void OnGrabbed(SelectEnterEventArgs args)
        {
            isHeld = true;

            if (rb != null) rb.isKinematic = false;

            if (automationRoutine != null)
            {
                StopCoroutine(automationRoutine);
                automationRoutine = null;
            }

            if (isPlacedOnGrid)
            {
                if (placedAutomations.ContainsKey(currentGridCell))
                {
                    placedAutomations.Remove(currentGridCell);
                }
                isPlacedOnGrid = false;
            }
        }
        private void OnDropped(SelectExitEventArgs args)
        {
            isHeld = false;
            Vector2Int hoverCell = HighlightSync.Instance != null ? HighlightSync.Instance.CurrentToolHoverCell : new Vector2Int(-1, -1);

            if (HighlightSync.Instance != null)
            {
                HighlightSync.Instance.ClearToolHover();
            }

            bool isCropEmpty = FieldActionManager.Instance.GetCropData(hoverCell).state == CellState.Empty;
            bool isAutomationEmpty = !placedAutomations.ContainsKey(hoverCell);

            if (isOverTable && hoverCell.x >= 0 && isCropEmpty && isAutomationEmpty)
            {
                currentGridCell = hoverCell;
                placedAutomations[currentGridCell] = this;
                isPlacedOnGrid = true;

                transform.position = GetTableWorldPosition(currentGridCell);
                transform.rotation = HighlightSync.Instance.tableOrigin.rotation;

                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                SetOverTable(true);

                startPosition = transform.position;
                startRotation = transform.rotation;

                automationRoutine = StartCoroutine(AutomationLoop());
            }
            else
            {
                if (!isPlacedOnGrid)
                {
                    if (errorHint != null)
                    {
                        errorHint.message = "You must place this on an empty farm cell first!";
                        errorHint.ShowError();
                    }

                    if (args.interactorObject != null)
                    {
                        StartCoroutine(ForceReGrab(args.interactorObject));
                    }

                    return;
                }

                transform.position = startPosition;
                transform.rotation = startRotation;

                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                SetOverTable(false);

                if (currentGridCell.x >= 0 && startPosition != transform.position)
                {
                    placedAutomations[currentGridCell] = this;
                    isPlacedOnGrid = true;
                    automationRoutine = StartCoroutine(AutomationLoop());
                }
            }
        }

        private IEnumerator ForceReGrab(IXRSelectInteractor interactor)
        {
            yield return null;

            if (grab != null && grab.interactionManager != null)
            {
                grab.interactionManager.SelectEnter(interactor, grab);
            }
        }

        public void SetOverTable(bool over)
        {
            if (isOverTable == over || visualChild == null) return;

            isOverTable = over;

            if (scaleRoutine != null) StopCoroutine(scaleRoutine);
            scaleRoutine = StartCoroutine(ScaleVisuals(over ? originalScale * miniScale : originalScale));
        }

        private IEnumerator ScaleVisuals(Vector3 target)
        {
            while (Vector3.Distance(visualChild.localScale, target) > 0.001f)
            {
                visualChild.localScale = Vector3.Lerp(visualChild.localScale, target, Time.deltaTime * shrinkSpeed);
                yield return null;
            }
            visualChild.localScale = target;
        }

        private void Update()
        {
            if (!isHeld || HighlightSync.Instance == null) return;

            Plane tablePlane = new Plane(HighlightSync.Instance.tableOrigin.up, HighlightSync.Instance.tableOrigin.position);
            Ray toolRay = new Ray(transform.position, Vector3.down);

            if (tablePlane.Raycast(toolRay, out float distanceToTable) && distanceToTable < 1.5f)
            {
                Vector3 hit = toolRay.GetPoint(distanceToTable);
                HighlightSync.Instance.UpdateToolHover(hit);
                if (HighlightSync.Instance.CurrentToolHoverCell.x >= 0)
                {
                    SetOverTable(true);
                }
                else
                {
                    SetOverTable(false);
                }
            }
            else
            {
                HighlightSync.Instance.ClearToolHover();
                SetOverTable(false);
            }
        }

        private void UpdateGridPosition()
        {
            if (HighlightSync.Instance != null)
            {
                Transform fieldOrigin = HighlightSync.Instance.fieldOrigin;
                float cellSize = HighlightSync.Instance.fieldCellSize;

                Vector3 localPos = Quaternion.Inverse(fieldOrigin.rotation) * (transform.position - fieldOrigin.position);

                int x = Mathf.RoundToInt(localPos.x / cellSize);
                int y = Mathf.RoundToInt(localPos.z / cellSize);

                currentGridCell = new Vector2Int(x, y);
            }
        }
        private Vector3 GetTableWorldPosition(Vector2Int cell)
        {
            Transform tableOrigin = HighlightSync.Instance.tableOrigin;
            float tSize = HighlightSync.Instance.tableCellSize;

            return tableOrigin.position
                + (tableOrigin.right * (cell.x * tSize))
                + (tableOrigin.forward * (cell.y * tSize));
        }

        private IEnumerator AutomationLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(tickInterval);

                ApplyAoEEffect();

                if (triggerEffect != null)
                {
                    triggerEffect.Play();
                }
            }
        }

        private void ApplyAoEEffect()
        {
            if (FieldActionManager.Instance == null) return;

            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    Vector2Int targetCell = new Vector2Int(currentGridCell.x + x, currentGridCell.y + y);

                    switch (actionType)
                    {
                        case AutomationAction.Water:
                            FieldActionManager.Instance.AutoWaterCell(targetCell);
                            break;

                        case AutomationAction.Fertilize:
                            FieldActionManager.Instance.AutoFertilizeCell(targetCell);
                            break;
                    }
                }
            }

            InventoryManager.Instance?.UpdateNums();
        }
    }

}