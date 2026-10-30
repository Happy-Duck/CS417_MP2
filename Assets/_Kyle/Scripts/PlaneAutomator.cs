using System.Collections;
using UnityEngine;
using Shared;

namespace Kyle
{
    public class PlaneAutomator : MonoBehaviour
    {
        public string upgradeKey = "PlaneUpgrade";

        public GameObject miniPlane;
        public GameObject bigPlane;
        public GameObject bigPlaneVisual;

        public float flyDuration = 5.0f;
        public float loopCooldown = 90f;
        public float miniFlightHeight = 0.4f;
        public float bigFlightHeight = 10f;
        public float bigPlaneZOffset = -5f;

        public AudioSource planeEngineAudio;
        public AudioSource sprayAudio;
        public ParticleSystem sprayMistLeft;
        public ParticleSystem sprayMistRight;

        public ParticleSystem bigSprayMistLeft;
        public ParticleSystem bigSprayMistRight;

        public float turbulenceSpeed = 1.5f;
        public float maxBankAngle = 10f;
        public float maxPitchAngle = 3f;

        private Coroutine flightRoutine;

        private void Awake()
        {
            SetVisualsActive(false);
        }

        private void Start()
        {
            if (ResourceManager.Instance != null && ResourceManager.Instance.Get(upgradeKey) > 0)
            {
                ActivatePlane();
            }
        }
        public void ActivatePlane()
        {
            if (flightRoutine == null)
            {
                flightRoutine = StartCoroutine(FlightSequence());
            }
        }

        private void Update()
        {
            if (bigPlane != null && bigPlane.activeSelf && HighlightSync.Instance != null)
            {
                Transform tableOrigin = HighlightSync.Instance.tableOrigin;
                Transform fieldOrigin = HighlightSync.Instance.fieldOrigin;

                float scaleFactor = HighlightSync.Instance.fieldCellSize / HighlightSync.Instance.tableCellSize;

                Quaternion localRot = Quaternion.Inverse(tableOrigin.rotation) * transform.rotation;
                bigPlane.transform.rotation = fieldOrigin.rotation * localRot;

                Vector3 offsetFromTable = transform.position - tableOrigin.position;
                Vector3 localOffset = Quaternion.Inverse(tableOrigin.rotation) * offsetFromTable;
                Vector3 scaledOffset = localOffset * scaleFactor;
                Vector3 basePos = fieldOrigin.position + (fieldOrigin.rotation * scaledOffset);

                bigPlane.transform.position = basePos + (fieldOrigin.up * bigFlightHeight) + (bigPlane.transform.forward * bigPlaneZOffset);
            }
        }

        private IEnumerator FlightSequence()
        {
            while (true)
            {
                //Vector3 startPos = GetFlightPosition(4.5f, -3f, miniFlightHeight);
                Vector3 startPos = GetFlightPosition(-3f, 4.5f, miniFlightHeight);
                //Vector3 endPos = GetFlightPosition(4.5f, 12f, miniFlightHeight);
                Vector3 endPos = GetFlightPosition(20f, 4.5f, miniFlightHeight);

                transform.position = startPos;

                Quaternion baseForwardRot = Quaternion.identity;

                if (HighlightSync.Instance != null)
                {
                    baseForwardRot = Quaternion.LookRotation((endPos - startPos).normalized, HighlightSync.Instance.tableOrigin.up);
                    transform.rotation = Quaternion.LookRotation((endPos - startPos).normalized, HighlightSync.Instance.tableOrigin.up);
                }

                SetVisualsActive(true);

                float elapsedTime = 0f;
                bool weedsKilled = false;
                bool isSpraying = false;
                bool playSpraySound = false;

                float flightNoise = Random.Range(0f, 100f);

                while (elapsedTime < flyDuration)
                {
                    float progress = elapsedTime / flyDuration;
                    transform.position = Vector3.Lerp(startPos, endPos, progress);

                    float rollNoise = Mathf.PerlinNoise(Time.time * turbulenceSpeed + flightNoise, 0f) * 2f - 1f;
                    float pitchNoise = Mathf.PerlinNoise(0f, Time.time * turbulenceSpeed + flightNoise) * 2f - 1f;

                    float currentBank = rollNoise * maxBankAngle;
                    float currentPitch = pitchNoise * maxPitchAngle;

                    Quaternion turbulenceRot = Quaternion.Euler(currentPitch, 0, currentBank);
                    transform.rotation = baseForwardRot * turbulenceRot;

                    if (progress >= 0.3f && progress < 0.5f && !playSpraySound)
                    {
                        sprayAudio.Play();
                        playSpraySound = true;
                    }

                    if (progress >= 0.4f && progress < 0.85f && !isSpraying)
                    {
                        sprayMistLeft.Play();
                        sprayMistRight.Play();
                        bigSprayMistLeft.Play();
                        bigSprayMistRight.Play();
                        isSpraying = true;
                        //sprayAudio.Play();
                    }
                    else if (progress >= 0.85f && isSpraying)
                    {
                        sprayMistLeft.Stop();
                        sprayMistRight.Stop();
                        bigSprayMistLeft.Stop();
                        bigSprayMistRight.Stop();
                        isSpraying = false;
                        //sprayAudio.Stop();
                    }

                    if (progress >= 0.6f && !weedsKilled)
                    {
                        FieldActionManager.Instance.AutoKillAllWeeds();
                        weedsKilled = true;
                    }

                    elapsedTime += Time.deltaTime;
                    yield return null;
                }

                SetVisualsActive(false);

                //yield return new WaitForSeconds(2f);
                transform.position = transform.position + (Vector3.down * 10f);

                yield return new WaitForSeconds(loopCooldown);
            }
        }
        private void SetVisualsActive(bool isActive)
        {
            if (miniPlane != null) miniPlane.SetActive(isActive);
            if (bigPlane != null) bigPlaneVisual.SetActive(isActive);

            if (planeEngineAudio != null)
            {
                if (isActive && !planeEngineAudio.isPlaying) planeEngineAudio.Play();
                else if (!isActive && planeEngineAudio.isPlaying) planeEngineAudio.Stop();
            }

            //if (sprayMist != null)
            //{
            //    if (isActive) sprayMist.Play();
            //    else sprayMist.Stop();
            //}
        }

        private Vector3 GetFlightPosition(float x, float y, float heightOffset)
        {
            Transform tableOrigin = HighlightSync.Instance.tableOrigin;
            float tSize = HighlightSync.Instance.tableCellSize;

            return tableOrigin.position
                + (tableOrigin.right * (x * tSize))
                + (tableOrigin.forward * (y * tSize))
                + (tableOrigin.up * heightOffset);
        }
    }
}