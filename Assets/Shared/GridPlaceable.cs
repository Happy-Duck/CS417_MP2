using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables; // XRI 3.x, if you're on XRI 2.x delete this line

namespace Shared {

	// Lets the player pick something up and set it down on the grid
	// Needs an XR Grab Interactable and a GridOccupant on the same object (they get added automatically)
	// How it works:
	//		let go of it over the floor and it snaps to the nearest free spot
	//		the first time its placed it costs costAmount of costResource (leave empty for free)
	//		if the spot is taken or you can't afford it, it goes back where it was (or disappears if its new from a shelf)
	//		things you (as the dev) placed in the scene start on the grid and count as already paid for
	// Set placeableSurfaceLayers to only the object layer you want to be able to place objects onto, and keep this object OFF that layer
	[RequireComponent(typeof(XRGrabInteractable))]
	[RequireComponent(typeof(GridOccupant))]
	public class GridPlaceable : MonoBehaviour {
		// optional cost, leave the resource name empty for free
		[SerializeField] private string costResource = "";
		[SerializeField] private int costAmount = 0;

		// set this to only have the layers you want to place stuff on
		// 64 (2^6) is layer 6 (canPlaceOn), look into layer masks if curious, its basically a bitstring
		[SerializeField] private LayerMask placeableSurfaceLayers = 64;

		// how far below the object we look for the floor when its let go
		[SerializeField] private float maxDropHeight = 3f;
		// how far above the object the floor check starts, so it still finds the floor if the object is touching or slightly inside it
		[SerializeField] private float rayStartHeight = 0.5f;

		// fires the first time this is grabbed off a shelf (GridSupplyShelf uses it to restock)
		public event Action<GridPlaceable> TakenFromShelf;

		// optional hooks for sound effects, particles, etc (nothing needs to subscribe)
		// PickedUp: every time the player grabs it
		// Placed: it snapped onto the grid after being let go (not when it just goes back to where it was)
		// Purchased: the first time its placed after being taken from a shelf (paid for, if it had a cost)
		public event Action<GridPlaceable> PickedUp;
		public event Action<GridPlaceable> Placed;
		public event Action<GridPlaceable> Purchased;

		private XRGrabInteractable grab;
		private GridOccupant occupant;
		private Rigidbody body;

		private bool isShelfStock = false;
		private bool isPaidFor = false;
		private Vector3 lastPosition;
		private Quaternion lastRotation;

		private void Awake() {
			grab = GetComponent<XRGrabInteractable>();
			occupant = GetComponent<GridOccupant>();
			body = GetComponent<Rigidbody>();

			// this script decides when it goes on the grid, not GridOccupant
			occupant.RegisterOnStart = false;
			// it gets snapped into place on release, so don't let it get thrown
			grab.throwOnDetach = false;
			// kinematic so it stays where its put instead of falling over or rolling away
			if (body != null) body.isKinematic = true;
		}

		private void OnEnable() {
			grab.selectEntered.AddListener(OnGrabbed);
			grab.selectExited.AddListener(OnReleased);
		}
		private void OnDisable() {
			grab.selectEntered.RemoveListener(OnGrabbed);
			grab.selectExited.RemoveListener(OnReleased);
		}

		private void Start() {
			if (isShelfStock) return;

			// placed in the scene by hand (or spawned by a GridSpawner), so it starts on the grid and is already paid for
			isPaidFor = true;
			if (!occupant.IsPlaced && !occupant.TryPlaceAt(transform.position)) {
				Debug.LogWarning($"GridPlaceable: {name} overlaps something on the grid and wasn't placed", this);
			}
		}

		// GridSupplyShelf calls this right after spawning it, so it sits on the shelf instead of going on the grid
		public void MakeShelfStock() {
			isShelfStock = true;
		}

		private void OnGrabbed(SelectEnterEventArgs args) {
			if (PickedUp != null) PickedUp.Invoke(this);

			if (isShelfStock) {
				isShelfStock = false;
				if (TakenFromShelf != null) TakenFromShelf.Invoke(this);
			}

			// if it was on the grid, remember where so we can put it back, then free its cells
			if (occupant.IsPlaced) {
				lastPosition = transform.position;
				lastRotation = transform.rotation;
				occupant.RemoveFromGrid();
			}
		}

		private void OnReleased(SelectExitEventArgs args) {
			if (TryPlaceOnFloor()) return;

			// couldn't place it, so put it back where it was, or get rid of it if it was never placed
			if (isPaidFor) {
				SetRotation(lastRotation);
				if (!occupant.TryPlaceAt(lastPosition)) Destroy(gameObject);
			} else {
				Destroy(gameObject);
			}
		}

		// finds the floor under the object and places it on the nearest free spot there
		private bool TryPlaceOnFloor() {
			GridManager grid = GridManager.Instance;
			if (grid == null) return false;

			// 1. find the floor, starting the ray above the object so it can't start inside the floor
			Vector3 rayStart = transform.position + Vector3.up * rayStartHeight;
			bool hitGround = Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit,
				maxDropHeight + rayStartHeight, placeableSurfaceLayers, QueryTriggerInteraction.Ignore);
			if (!hitGround) return false;

			// 2. make sure the whole footprint is empty
			Vector2Int start = grid.GetFootprintStart(hit.point, occupant.Size);
			if (!grid.IsAreaFree(start, occupant.Size)) return false;

			// 3. pay for it the first time its placed (after the free check, so the player never pays for a failed placement)
			bool wasPaidFor = isPaidFor;
			if (!isPaidFor) {
				if (!string.IsNullOrEmpty(costResource)) {
					if (ResourceManager.Instance == null || !ResourceManager.Instance.TrySpend(costResource, costAmount)) return false;
				}
				isPaidFor = true;
			}

			// 4. stand it upright and snap it into place
			SetRotation(SnappedRotation());
			if (!occupant.TryPlaceAt(hit.point)) return false;

			if (Placed != null) Placed.Invoke(this);
			if (!wasPaidFor && Purchased != null) Purchased.Invoke(this);
			return true;
		}

		// square things can face any of the 4 directions, non-square things always face +Z so their footprint still matches
		private Quaternion SnappedRotation() {
			if (occupant.Size.x != occupant.Size.y) return Quaternion.identity;
			float yaw = Mathf.Round(transform.eulerAngles.y / 90f) * 90f;
			return Quaternion.Euler(0f, yaw, 0f);
		}

		// rotates the transform and the rigidbody together, otherwise a rigidbody (especially one with interpolation on)
		// can keep drawing the object at its old rotation
		private void SetRotation(Quaternion rotation) {
			transform.rotation = rotation;
			if (body != null) body.rotation = rotation;
		}
	}
}