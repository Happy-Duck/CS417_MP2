using UnityEngine;

namespace Alyssa
{
    // Add to a BedDevice prefab (hoe, scythe, fertilizer bag). Animates the model so it looks like it is working.
    // It follows the BedDevice's Cooldown stat, so the downward strike lands exactly when the device acts.
    [RequireComponent(typeof(BedDevice))]
    public class DeviceSwing : MonoBehaviour
    {
        public Transform visual;                     // the model child to animate (first child if empty)
        public float dropDistance = 0.08f;           // how far it moves down on the strike
        public float tiltDegrees = 25f;              // how far it tips on the strike (0 = straight up and down)
        public Vector3 tiltAxis = Vector3.right;     // try X, Y or Z until it looks right for your model

        BedDevice device;
        Vector3 basePos;
        Quaternion baseRot;

        void Awake()
        {
            device = GetComponent<BedDevice>();
            if (visual == null && transform.childCount > 0) visual = transform.GetChild(0);
            if (visual != null) { basePos = visual.localPosition; baseRot = visual.localRotation; }
        }

        void LateUpdate()
        {
            if (visual == null) return;
            float v = 0f;                                          // 0 = rest, -1 = bottom of the strike
            if (GetComponentInParent<SoilBed>() != null)           // only animates once placed on a bed
            {
                var cd = device.Get("Cooldown");
                float p = 1f - Mathf.Clamp01(cd.value / device.interval);
                v = Pose(p);
            }
            visual.localPosition = basePos + Vector3.up * (v * dropDistance);
            visual.localRotation = baseRot * Quaternion.AngleAxis(v * tiltDegrees, tiltAxis);
        }

        // 0.00-0.15 recover, 0.15-0.75 rest, 0.75-0.90 wind up, 0.90-1.00 strike down
        static float Pose(float p)
        {
            if (p < 0.15f) return Mathf.Lerp(-1f, 0f, p / 0.15f);
            if (p < 0.75f) return 0f;
            if (p < 0.90f) return Mathf.Lerp(0f, 0.5f, (p - 0.75f) / 0.15f);
            float t = (p - 0.90f) / 0.10f;
            return Mathf.Lerp(0.5f, -1f, t * t);                   // speeds up into the strike
        }
    }
}