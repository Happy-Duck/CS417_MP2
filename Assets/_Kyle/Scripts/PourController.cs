using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;

namespace Kyle
{
    public class PourController : MonoBehaviour
    {
        [SerializeField] ParticleSystem fuelStream;
        [SerializeField] float minPourAngle = 50f;
        [SerializeField] float maxPourAngle = 135f;
        [SerializeField] float minPourRate = 10f;
        [SerializeField] float maxPourRate = 150f;

        private Vector3 originPosition;
        private Quaternion originRotation;

        private Rigidbody rb;
        private XRGrabInteractable grabInteractable;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            originPosition = transform.position;
            originRotation = transform.rotation;

            rb = GetComponent<Rigidbody>();
            grabInteractable = GetComponent<XRGrabInteractable>();

            grabInteractable.selectExited.AddListener(OnReleased);
        }
        // Update is called once per frame
        void Update()
        {
            float tiltAngle = Vector3.Angle(transform.up, Vector3.up);

            if (tiltAngle > minPourAngle)
            {
                if (!fuelStream.isPlaying)
                {
                    fuelStream.Play();
                }
                float pourRate = Mathf.InverseLerp(minPourAngle, maxPourAngle, tiltAngle);
                var emission = fuelStream.emission;
                emission.rateOverTime = Mathf.Lerp(minPourRate, maxPourRate, pourRate);
            }
            else
            {
                if (fuelStream.isPlaying)
                {
                    fuelStream.Stop();
                }
            }
        }
        private void OnReleased(SelectExitEventArgs args)
        {
            transform.position = originPosition;
            transform.rotation = originRotation;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        void OnDestroy()
        {
            if (grabInteractable != null)
            {
                grabInteractable.selectExited.RemoveListener(OnReleased);
            }
        }
    }
}
