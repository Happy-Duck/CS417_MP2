using Shared;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Alyssa
{
    // A shelf spot that sells one item (a BedDevice prefab or the fertilizer prefab).
    // Same restocking idea as PacketDispenser, but it charges for the item.
    public class DeviceDispenser : MonoBehaviour
    {
        public GameObject itemPrefab;
        public Transform displayPoint;           // uses this object if empty
        public Transform spawnParent;            // Alyssa_Scene_Root
        public string costResource = "Compost";
        public int cost = 3;
        public float restockDelay = 1f;

        void Start() { Restock(); }

        void Restock()
        {
            if (itemPrefab == null) { Debug.LogError("DeviceDispenser: no item prefab", this); return; }
            Transform p = displayPoint != null ? displayPoint : transform;
            var item = Instantiate(itemPrefab, p.position, p.rotation, spawnParent);

            var grab = item.GetComponent<XRGrabInteractable>();
            UnityAction<SelectEnterEventArgs> onGrab = null;
            onGrab = _ =>
            {
                grab.selectEntered.RemoveListener(onGrab);
                var rm = ResourceManager.Instance;
                if (cost > 0 && !rm.CanAfford(costResource, cost))
                {
                    // Error Tolerant Lesson: recognise the attempt, explain why it failed
                    Feedback.Hint("cant-afford-" + itemPrefab.name,
                        "Not enough " + costResource + ". You need " + cost + " to buy this.", item.transform.position);
                    Destroy(item);
                }
                else if (cost > 0)
                {
                    rm.TrySpend(costResource, cost);
                }
                Invoke(nameof(Restock), restockDelay);
            };
            grab.selectEntered.AddListener(onGrab);
        }
    }
}
