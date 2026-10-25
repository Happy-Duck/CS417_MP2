using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Alyssa
{
    public class SoilBed : MonoBehaviour
    {
        public Plant plant;

        public bool Placed { get; private set; }
        [HideInInspector] public Weed weed;                       // set by WeedSpawner at runtime
        public Vector3 plantOffset = new Vector3(0, 0.25f, 0);   // soil surface height

        void Awake() { weed = null; }                             

        public bool HasWeed => weed != null && !weed.Pulled;

        public bool TryPlant(Plant prefab)
        {
            if (plant != null || HasWeed) return false;
            plant = Instantiate(prefab, transform.position + plantOffset,
                                Quaternion.identity, transform);
            return true;
        }

        void Start()
        {
            var grab = GetComponent<XRGrabInteractable>();
            if (grab != null) grab.selectExited.AddListener(_ => Placed = true);
        }
    }
}