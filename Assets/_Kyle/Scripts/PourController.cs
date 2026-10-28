using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;
using Alyssa;
using System.IO;
using System.Runtime.InteropServices.ComTypes;

namespace Kyle
{
    [RequireComponent(typeof(XRGrabInteractable))]
    [RequireComponent(typeof(Rigidbody))]
    public class PourController : Statted
    {
        [SerializeField] ParticleSystem fuelStream;
        [SerializeField] float minPourAngle = 30f;
        [SerializeField] float maxPourAngle = 135f;
        [SerializeField] float minPourRate = 10f;
        [SerializeField] float maxPourRate = 150f;

        [SerializeField] float waterPerSec = 25f;

        private Vector3 originPosition;
        private Quaternion originRotation;

        private Rigidbody rb;
        private XRGrabInteractable grabInteractable;
        private bool isPouring;

        void Awake()
        {
            if (stats.Count == 0)
                stats.Add(new Stat { name = "WaterLevel", value = 100 });

            originPosition = transform.position;
            originRotation = transform.rotation;

            rb = GetComponent<Rigidbody>();
            grabInteractable = GetComponent<XRGrabInteractable>();

            grabInteractable.selectExited.AddListener(OnReleased);

            if (fuelStream != null)
                fuelStream.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        //Update is called once per frame
        protected override void Update()
        {
            base.Update();

            float tiltAngle = Vector3.Angle(transform.up, Vector3.up);
            bool hasWater = Get("WaterLevel").value > 0;

            if (tiltAngle > minPourAngle && hasWater)
            {
                isPouring = true;
                if (!fuelStream.isPlaying)
                {
                    fuelStream.Play();
                }
                float pourRate = Mathf.InverseLerp(minPourAngle, maxPourAngle, tiltAngle);
                var emission = fuelStream.emission;
                emission.rateOverTime = Mathf.Lerp(minPourRate, maxPourRate, pourRate);
                Get("WaterLevel").value -= (waterPerSec * 0.2f) * Time.deltaTime;
            }
            else
            {
                isPouring = false;

                if (fuelStream.isPlaying)
                {
                    fuelStream.Stop(true, ParticleSystemStopBehavior.StopEmitting);
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
