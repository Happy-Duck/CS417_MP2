using System.Collections;
using Shared;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Alyssa
{
    // One script for all four unlocked helpers. Grab it, drop it on a soil bed, and it snaps into
    // its slot and works every `interval` seconds. Grab it again to take it off the bed.
    // Set Kind, Slot and Slot Rotation on each prefab.
    [RequireComponent(typeof(XRGrabInteractable))]
    public class BedDevice : Statted
    {
        public enum Kind { Sprinkler, AutoWeeder, AutoFertilizer, AutoHarvester }

        public Kind kind;
        public float interval = 5f;          // seconds between actions
        public float waterPerPulse = 15f;    // Sprinkler only, taken from the barrel's Reservoir
        public Vector3 slot;                 // where it sits on the bed (bed-local), one corner per kind
        public Vector3 slotRotation;         // how it is rotated on the bed (bed-local euler angles)

        XRGrabInteractable grab;
        SoilBed bed;
        Transform originalParent;
        bool origKinematic, origGravity, pulledOff;
        Stat reservoir;

        void Awake()
        {
            grab = GetComponent<XRGrabInteractable>();
            originalParent = transform.parent;                   // where to return to when pulled off a bed
            var rb0 = GetComponent<Rigidbody>();
            if (rb0 != null) { origKinematic = rb0.isKinematic; origGravity = rb0.useGravity; }   // the prefab's own physics settings
            grab.selectEntered.AddListener(_ => OnGrabbed());
            grab.selectExited.AddListener(_ => OnReleased());
            if (Get("Cooldown") == null)
                stats.Add(new Stat { name = "Cooldown", value = interval, max = interval, ratePerSec = -1f });
        }

        protected override void Update()
        {
            base.Update();                                       // Euler integration: Cooldown counts down
            var cd = Get("Cooldown");
            if (bed == null) { cd.value = interval; return; }    // idle until it is on a bed
            if (cd.value > 0f) return;
            cd.value = interval;
            Act();
        }

        // needs a trigger collider on the prefab, same as the seed packets
        void OnTriggerStay(Collider other)
        {
            if (bed != null || grab.isSelected) return;          // already placed, or still in the hand
            var b = other.GetComponentInParent<SoilBed>();
            if (b == null || HasKind(b, kind)) return;
            Attach(b);
        }

        static bool HasKind(SoilBed b, Kind k)
        {
            foreach (var d in b.GetComponentsInChildren<BedDevice>())
                if (d.kind == k) return true;
            return false;
        }

        void Attach(SoilBed b)
        {
            bed = b;
            var rb = GetComponent<Rigidbody>();
            if (rb != null) { rb.isKinematic = true; rb.useGravity = false; }
            transform.SetParent(b.transform, false);             // moves with the bed, dies with the bed
            transform.localPosition = slot;
            transform.localRotation = Quaternion.Euler(slotRotation);
        }

        // grabbing a device that is on a bed pulls it off
        void OnGrabbed()
        {
            if (bed == null) return;
            pulledOff = true;
            bed = null;                                          // Cooldown resets and the animation stops
            transform.SetParent(originalParent, true);
        }

        // XRI restores the Rigidbody to what it was before the grab (kinematic while on a bed),
        // so after pulling a device off a bed put back the prefab's own settings a frame later
        void OnReleased()
        {
            if (!pulledOff) return;
            pulledOff = false;
            StartCoroutine(Unfreeze());
        }

        IEnumerator Unfreeze()
        {
            yield return null;
            var rb = GetComponent<Rigidbody>();
            if (rb != null && bed == null) { rb.isKinematic = origKinematic; rb.useGravity = origGravity; }
        }

        void Act()
        {
            switch (kind)
            {
                case Kind.Sprinkler:
                    if (bed.plant == null) return;
                    var res = Reservoir();
                    if (res == null || res.value < waterPerPulse)
                    {
                        Feedback.Hint("sprinkler-dry", "The barrel is low, so the sprinkler can't run. Give it time to refill.", transform.position);
                        return;
                    }
                    res.value -= waterPerPulse;
                    bed.plant.Water(waterPerPulse);
                    break;

                case Kind.AutoWeeder:
                    if (bed.weed != null && !bed.weed.Pulled)
                    {
                        Destroy(bed.weed.gameObject);
                        bed.weed = null;
                    }
                    break;

                case Kind.AutoFertilizer:
                    if (bed.plant == null) return;
                    var rm = ResourceManager.Instance;
                    if (rm.CanAfford("Fertilizer", 1) && FertilizerBoost.Apply(bed.plant))
                        rm.TrySpend("Fertilizer", 1);
                    break;

                case Kind.AutoHarvester:
                    Harvest();
                    break;
            }
        }

        void Harvest()
        {
            if (bed.plant == null) return;
            foreach (var f in bed.plant.GetComponentsInChildren<Fruit>())   // unpicked fruit is parented to the plant
            {
                ResourceManager.Instance.Add(f.resourceName, 1);
                GameEvents.RaiseHarvested(f.resourceName);
                Destroy(f.gameObject);
            }
        }

        // the rain barrel is whichever Statted object has a Reservoir stat
        Stat Reservoir()
        {
            if (reservoir == null)
                foreach (var s in FindObjectsByType<Statted>())
                {
                    var r = s.Get("Reservoir");
                    if (r != null) { reservoir = r; break; }
                }
            return reservoir;
        }
    }
}