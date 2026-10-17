using UnityEngine;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Unity.GraphToolkit.Editor;

namespace Kyle
{

    public class FarmingToolScript : MonoBehaviour
    {
        public FarmingTool toolAction;
        public Transform visualChild;

        [SerializeField] float miniScale = 0.1f;
        [SerializeField] float shrinkSpeed = 5f;

        private Vector3 originalScale;
        private Coroutine scaleRoutine;
        private XRGrabInteractable grab;
        private Rigidbody rb;
        private bool isHeld = false;

        public bool isOverTable { get; private set; }

        private Vector3 startPosition;
        private Quaternion startRotation;

        private void Awake()
        {
            originalScale = visualChild.localScale;
            grab = GetComponent<XRGrabInteractable>();
            rb = GetComponent<Rigidbody>();

            startPosition = transform.position;
            startRotation = transform.rotation;

            grab.selectEntered.AddListener(args => isHeld = true);
            grab.selectExited.AddListener(OnDropped);
        }

        public void SetOverTable(bool over)
        {
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
            if (!isHeld || HighlightSync.Instance == null) { return; }

            Plane tablePlane = new Plane(HighlightSync.Instance.tableOrigin.up, HighlightSync.Instance.tableOrigin.position);
            Ray toolRay = new Ray(transform.position, Vector3.down);

            Debug.DrawRay(transform.position, Vector3.down * 1.5f, Color.red);

            if (tablePlane.Raycast(toolRay, out float distanceToTable) && distanceToTable < 1.5f)
            {
                Vector3 hit = toolRay.GetPoint(distanceToTable);
                HighlightSync.Instance.UpdateToolHover(hit);
            }
            else
            {
                HighlightSync.Instance.ClearToolHover();
            }

            //bool hitPlane = tablePlane.Raycast(toolRay, out float distanceToTable);

            //if (hitPlane)
            //{
            //    Vector3 hit = toolRay.GetPoint(distanceToTable);
            //    Debug.Log($"[SeedBag Debug] Hit plane at {hit}. Distance: {distanceToTable}");

            //    if (distanceToTable < 5.0f)
            //    {
            //        HighlightSync.Instance.UpdateToolHover(hit);
            //    }
            //    else
            //    {
            //        Debug.LogWarning("[SeedBag Debug] Ray hit the plane, but distance was > 5.0f!");
            //        HighlightSync.Instance.ClearToolHover();
            //    }
            //}
            //else
            //{
            //    Debug.LogWarning("[SeedBag Debug] Raycast completely missed the mathematical plane!");
            //    HighlightSync.Instance.ClearToolHover();
            //}
        }

        private void OnDropped(SelectExitEventArgs args)
        {
            isHeld = false;

            if (HighlightSync.Instance.CurrentToolHoverCell.x >= 0 || HighlightSync.Instance.GetSelectedCells().Count > 0)
            {
                FieldActionManager.Instance.ExecuteToolDrop(this);
            }

            HighlightSync.Instance.ClearToolHover();

            // teleport back
            transform.position = startPosition;
            transform.rotation = startRotation;
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            SetOverTable(false);
        }
    }
}
