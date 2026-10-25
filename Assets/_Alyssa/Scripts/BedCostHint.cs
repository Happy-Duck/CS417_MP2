using Shared;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Alyssa
{
    // Add to the SoilBed prefab. Warns when you pick a new bed off the shelf with no Compost.
    [RequireComponent(typeof(XRGrabInteractable))]
    public class BedCostHint : MonoBehaviour
    {
        public string costResource = "Compost";      // match GridPlaceable's Cost Resource
        public int cost = 1;                         // match GridPlaceable's Cost Amount
        public string tip = "Pull weeds to earn more.";   // how to get more of it
        public float shelfRadius = 1.5f;             // only warn while the bed is still near the shelf

        Transform shelf;

        void Start()
        {
            var s = GameObject.Find("BedShelf");
            if (s != null) shelf = s.transform;
            GetComponent<XRGrabInteractable>().selectEntered.AddListener(_ => Check());
        }

        void Check()
        {
            if (shelf == null || Vector3.Distance(transform.position, shelf.position) > shelfRadius) return;
            var rm = ResourceManager.Instance;
            if (rm != null && !rm.CanAfford(costResource, cost))
                Feedback.Hint("bed-no-compost", "Beds cost " + cost + " " + costResource + ". " + tip, transform.position);
        }
    }
}