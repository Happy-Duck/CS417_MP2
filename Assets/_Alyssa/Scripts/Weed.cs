using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Alyssa
{
    public class Weed : Statted
    {
        public Transform visual;             // uses this object if empty
        public float spreadPerSec = 2f;
        public bool Pulled { get; private set; }

        void Awake()
        {
            if (visual == null) visual = transform;
            if (Get("Spread") == null)
                stats.Add(new Stat { name = "Spread", value = 0, ratePerSec = spreadPerSec });

            var grab = GetComponent<XRGrabInteractable>();
            if (grab != null) grab.selectEntered.AddListener(_ => Pull());
        }

        void Pull()
        {
            Pulled = true;
            Get("Spread").ratePerSec = 0;         // stops growing once picked
            transform.SetParent(null, true);      
        }

        protected override void Update()
        {
            base.Update();                    // Euler integration
            float s = Get("Spread").value / 100f;
            visual.localScale = Vector3.one * Mathf.Lerp(0.4f, 1f, s);
        }
    }
}