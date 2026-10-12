using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Shared;

namespace Alyssa
{
    [RequireComponent(typeof(XRGrabInteractable))]
    public class SeedPacket : MonoBehaviour
    {
        public Plant plantPrefab;
        public string seedResource = "StrawberrySeed";

        void OnTriggerEnter(Collider other)
        {   // put a Trigger collider on this packet
            var bed = other.GetComponentInParent<SoilBed>();
            if (bed == null || bed.plant != null) return;
            if (!ResourceManager.Instance.CanAfford(seedResource, 1)) return;
            if (bed.TryPlant(plantPrefab))
                ResourceManager.Instance.TrySpend(seedResource, 1);
        }
    }
}