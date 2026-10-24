using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Alyssa
{
    // Add to the watering can.
    [RequireComponent(typeof(WateringCan))]
    public class CanHints : MonoBehaviour
    {
        WateringCan can;
        bool wasEmpty;

        void Awake()
        {
            can = GetComponent<WateringCan>();
            GetComponent<XRGrabInteractable>().activated.AddListener(_ => { if (Empty()) Tell(); });
        }

        void Start() { wasEmpty = Empty(); }

        void Update()
        {
            bool e = Empty();
            if (e && !wasEmpty) Tell();      // ran dry mid-pour
            wasEmpty = e;
        }

        bool Empty()
        {
            var level = can.Get("WaterLevel");
            return level != null && level.value <= 0f;
        }

        void Tell()
        {
            Feedback.Hint("can-empty", "The can is empty. Dip it in the rain barrel to refill.", transform.position);
        }
    }
}
