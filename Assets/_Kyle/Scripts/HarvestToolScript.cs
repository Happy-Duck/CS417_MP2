using UnityEngine;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Unity.GraphToolkit.Editor;

namespace Kyle
{

    public class HarvestToolScript : MonoBehaviour
    {
        public FarmingTool toolAction;
        public Transform visualChild;

        private XRGrabInteractable grab;
        private Rigidbody rb;
        private bool isHeld = false;

        private Vector3 startPosition;
        private Quaternion startRotation;

        private void Awake()
        {
            grab = GetComponent<XRGrabInteractable>();
            rb = GetComponent<Rigidbody>();

            startPosition = transform.position;
            startRotation = transform.rotation;

            grab.selectEntered.AddListener(args => isHeld = true);
            grab.selectExited.AddListener(OnDropped);
        }

        private void Update()
        {
            if (!isHeld || HighlightSync.Instance == null) { return; }

            Plane tablePlane = new Plane(HighlightSync.Instance.tableOrigin.up, HighlightSync.Instance.tableOrigin.position);
            Ray toolRay = new Ray(transform.position, Vector3.down);

            if (tablePlane.Raycast(toolRay, out float distanceToTable) && distanceToTable < 1.5f)
            {
                Vector3 hit = toolRay.GetPoint(distanceToTable);
                HighlightSync.Instance.UpdateToolHover(hit);
            }
            else
            {
                HighlightSync.Instance.ClearToolHover();
            }
        }
        public void ProcessBladeHit(Collider other)
        {
            if (!isHeld) { return; }

            FieldCellCollider cellCollider = other.GetComponent<FieldCellCollider>();
            if (cellCollider != null && toolAction == FarmingTool.Harvest || toolAction == FarmingTool.Hoe)
            {
                FieldActionManager.Instance.TrySwipeHarvest(cellCollider.CellCoordinates, toolAction);
            }
        }

        private void OnDropped(SelectExitEventArgs args)
        {
            isHeld = false;
            HighlightSync.Instance.ClearToolHover();

            transform.position = startPosition;
            transform.rotation = startRotation;
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
}
