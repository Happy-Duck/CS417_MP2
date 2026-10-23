using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Aster {
    public class PopupTutorial : MonoBehaviour
    {
        static readonly HashSet<string> seen = new HashSet<string>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetSeen() { seen.Clear(); }

        public string id;
        [TextArea] public string message;
        public TMP_Text label;
        public GameObject panel;
        public float showDistance = 1.3f;
        public float duration = 8f;

        float timeLeft;
        bool showing;

        void Start()
        {
            if (label != null) label.text = message;
            if (panel != null) panel.SetActive(false);
        }

        void Update()
        {
            var cam = Camera.main;
            if (cam == null || panel == null) return;

            if (!showing && !seen.Contains(id) &&
                Vector3.Distance(cam.transform.position, transform.position) < showDistance)
            {
                seen.Add(id);
                showing = true;
                timeLeft = duration;
                panel.SetActive(true);
            }

            if (showing)
            {
                panel.transform.rotation = Quaternion.LookRotation(panel.transform.position - cam.transform.position);
                timeLeft -= Time.deltaTime;
                if (timeLeft <= 0f) Dismiss();
            }
        }

        public void Dismiss()
        {
            showing = false;
            if (panel != null) panel.SetActive(false);
        }
    }
}
