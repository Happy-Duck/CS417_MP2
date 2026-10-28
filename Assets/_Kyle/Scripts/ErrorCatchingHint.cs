using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Kyle
{
    public class ErrorCatchingHint : MonoBehaviour
    {
        [TextArea] public string message;
        public float hideAfterSeconds = 3f;
        public Vector3 offset = new Vector3(0, 0.4f, 0);
        public float characterSize = 0.012f;
        //public Color textColor = Color.red;

        private GameObject label;
        private Camera cam;
        private float hideAt;

        private Camera Cam()
        {
            if (cam == null) cam = Camera.main;
            if (cam == null) cam = FindAnyObjectByType<Camera>();
            return cam;
        }

        private void Update()
        {
            if (label == null) return;

            var c = Cam();
            if (c != null)
            {
                label.transform.position = transform.position + offset;
                label.transform.rotation = Quaternion.LookRotation(label.transform.position - c.transform.position);
            }

            if (Time.time > hideAt)
            {
                Destroy(label);
                label = null;
            }
        }
        public void ShowError()
        {
            if (label != null)
            {
                hideAt = Time.time + hideAfterSeconds;
                return;
            }

            hideAt = Time.time + hideAfterSeconds;

            label = new GameObject("ErrorPopup_" + name);
            var tm = label.AddComponent<TextMesh>();

            tm.text = message;
            tm.characterSize = characterSize;
            tm.fontSize = 64;
            tm.anchor = TextAnchor.LowerCenter;
            tm.alignment = TextAlignment.Center;
            //tm.color = textColor;
        }

        private void OnDestroy()
        {
            if (label != null) Destroy(label);
        }
    }
}
