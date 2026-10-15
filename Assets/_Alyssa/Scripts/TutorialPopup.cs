using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Alyssa
{
    public class TutorialPopup : MonoBehaviour
    {
        public enum Trigger { Proximity, OnGrab }

        [TextArea] public string message = "Do the thing.";
        public string id = "";                         // unique per popup; defaults to the object name
        public Trigger triggerMode = Trigger.Proximity;
        public float showDistance = 1.5f;              // Proximity only
        public float hideAfterSeconds = 6f;            // OnGrab only
        public Vector3 offset = new Vector3(0, 0.4f, 0);   // world meters, not affected by parent scale
        public float characterSize = 0.012f;
        public bool onlyWhenPlaced = false;            // for grid items like the soil bed

        static readonly HashSet<string> seen = new();  // each popup only appears once per Play
        GameObject label;
        Shared.GridOccupant occ;
        Camera cam;
        float hideAt;

        void Start()
        {
            if (string.IsNullOrEmpty(id)) id = name;
            occ = GetComponent<Shared.GridOccupant>();
            if (triggerMode == Trigger.OnGrab)
            {
                var grab = GetComponent<XRGrabInteractable>();
                if (grab != null) grab.selectEntered.AddListener(_ => Show());
                else Debug.LogWarning("TutorialPopup (OnGrab) needs an XRGrabInteractable on " + name, this);
            }
        }

        Camera Cam()
        {
            if (cam == null) cam = Camera.main;
            if (cam == null) cam = FindAnyObjectByType<Camera>();
            return cam;
        }

        void Update()
        {
            var c = Cam();
            if (c == null) return;

            if (label == null)
            {
                if (triggerMode != Trigger.Proximity || seen.Contains(id)) return;
                if (onlyWhenPlaced && occ != null && !occ.IsPlaced) return;
                if (Vector3.Distance(c.transform.position, transform.position) < showDistance) Show();
                return;
            }

            label.transform.position = transform.position + offset;      // follows the object
            label.transform.rotation = Quaternion.LookRotation(label.transform.position - c.transform.position);

            bool hide = triggerMode == Trigger.OnGrab
                ? Time.time > hideAt
                : Vector3.Distance(c.transform.position, transform.position) > showDistance + 1.5f;
            if (hide) { Destroy(label); label = null; }
        }

        void Show()
        {
            if (label != null || seen.Contains(id)) return;
            seen.Add(id);
            hideAt = Time.time + hideAfterSeconds;
            label = new GameObject("Popup_" + id);                       // not parented, so no inherited scale
            var tm = label.AddComponent<TextMesh>();
            tm.text = message;
            tm.characterSize = characterSize;
            tm.fontSize = 64;
            tm.anchor = TextAnchor.LowerCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = Color.yellow;
        }

        void OnDestroy() { if (label != null) Destroy(label); }
    }
}