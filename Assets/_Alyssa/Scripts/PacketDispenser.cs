using Shared;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Alyssa
{
    public class PacketDispenser : MonoBehaviour
    {
        public GameObject packetPrefab;      // the StrawberrySeedPacket prefab
        public Transform displayPoint;       // where the packet sits; uses this object if empty
        public Transform spawnParent;        // Alyssa_Scene_Root
        public string seedResource = "StrawberrySeed";
        public int seedsPerPacket = 1;
        public float restockDelay = 1f;

        void Start() { Restock(); }

        void Restock()
        {
            if (packetPrefab == null) { Debug.LogError("PacketDispenser: no packet prefab", this); return; }
            Transform p = displayPoint != null ? displayPoint : transform;
            var packet = Instantiate(packetPrefab, p.position, p.rotation, spawnParent);

            var grab = packet.GetComponent<XRGrabInteractable>();
            UnityAction<SelectEnterEventArgs> onGrab = null;
            onGrab = _ => {
                grab.selectEntered.RemoveListener(onGrab);
                // free for now; later swap this line for ResourceManager.Instance.TrySpend(currency, price)
                ResourceManager.Instance.Add(seedResource, seedsPerPacket);
                Invoke(nameof(Restock), restockDelay);
            };
            grab.selectEntered.AddListener(onGrab);
        }
    }
}