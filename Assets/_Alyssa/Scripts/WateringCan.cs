using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Alyssa
{
    [RequireComponent(typeof(XRGrabInteractable))]
    public class WateringCan : Statted
    {
        public ParticleSystem stream;
        public float waterPerSec = 25f;
        bool pouring;

        void Awake()
        {
            if (stats.Count == 0)
                stats.Add(new Stat { name = "WaterLevel", value = 100 });
            var grab = GetComponent<XRGrabInteractable>();
            grab.activated.AddListener(_ => SetPouring(true));
            grab.deactivated.AddListener(_ => SetPouring(false));
        }

        void SetPouring(bool p)
        {
            pouring = p;
            if (stream) { if (p) stream.Play(); else stream.Stop(); }
        }

        void OnTriggerStay(Collider other)
        {   // spout Trigger collider on a child
            if (!pouring || Get("WaterLevel").value <= 0) return;
            var plant = other.GetComponentInParent<Plant>();
            if (plant == null) return;
            float amt = waterPerSec * Time.deltaTime;
            plant.Water(amt);
            Get("WaterLevel").value -= amt * 0.2f;
        }
    }
}