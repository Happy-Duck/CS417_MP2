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
            if (stream) stream.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        protected override void Update()
        {
            base.Update();
            if (pouring && Get("WaterLevel").value <= 0) SetPouring(false);   // empty can stops pouring
        }

        void SetPouring(bool p)
        {
            Debug.Log("Pouring: " + p);          // delete once it works
            pouring = p;
            if (!stream) return;
            if (p) stream.Play();
            else stream.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        void OnTriggerStay(Collider other)
        {
            if (pouring && Time.frameCount % 30 == 0)
                Debug.Log("Spout touching: " + other.name, other);

            if (!pouring || Get("WaterLevel").value <= 0) return;
            var plant = other.GetComponentInParent<Plant>();
            if (plant == null) return;
            float amt = waterPerSec * Time.deltaTime;
            plant.Water(amt);
            Get("WaterLevel").value -= amt * 0.2f;
        }
    }
}