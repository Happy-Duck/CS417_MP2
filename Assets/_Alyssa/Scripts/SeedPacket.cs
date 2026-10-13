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
        bool used;

        void OnTriggerEnter(Collider other)
        {
            if (used) return;
            var bed = other.GetComponentInParent<SoilBed>();
            if (bed == null || bed.plant != null) return;
            if (ResourceManager.Instance == null ||
                !ResourceManager.Instance.CanAfford(seedResource, 1)) return;
            if (bed.TryPlant(plantPrefab))
            {
                used = true;
                ResourceManager.Instance.TrySpend(seedResource, 1);
                Destroy(gameObject);
            }
        }
    }
}