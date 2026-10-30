using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Shared;

namespace Kyle
{
    //[RequireComponent(typeof(XRGrabInteractable))]
    public class TractorAutomator : MonoBehaviour
    {
        public string upgradeKey = "TractorUpgrade";

        public GameObject miniTractor;
        public GameObject bigTractor;
        public float moveSpeed = 0.5f; // seconds it takes to move 1 cell
        public float loopCooldown = 180f;
        public int gridColumns = 10;
        public int gridRows = 10;

        [Header("Audio & Animation")]
        public AudioSource engineAudio;
        public float bobSpeed = 15f;
        public float miniBobHeight = 0.1f;
        public float bigBobHeight = 0.001f;

        public float maxLeanAngle = 15f;

        //private XRGrabInteractable grabInteractable;
        private Coroutine automationRoutine;
        private List<Vector2Int> snakingPath;

        private Vector3 initialMiniLocalPos;
        private Quaternion initialMiniLocalRot;

        private float currentBobMultiplier = 0f;
        private float currentLeanAngle = 0f;

        private void Awake()
        {
            //grabInteractable = GetComponent<XRGrabInteractable>();
            snakingPath = GenerateSnakingPath(gridColumns, gridRows);
            SetVisualsActive(false);

            if (miniTractor != null)
            {
                initialMiniLocalPos = miniTractor.transform.localPosition;
                initialMiniLocalRot = miniTractor.transform.localRotation;
            }
        }
        private void Start()
        {
            if (ResourceManager.Instance != null && ResourceManager.Instance.Get(upgradeKey) > 0)
            {
                ActivateTractor();
            }
        }

        private void Update()
        {
            if (miniTractor != null)
            {
                miniTractor.transform.localPosition = initialMiniLocalPos + new Vector3(0, currentBobMultiplier * miniBobHeight, 0);
                miniTractor.transform.localRotation = initialMiniLocalRot * Quaternion.Euler(0, 0, currentLeanAngle);
            }

            if (bigTractor != null && bigTractor.activeSelf && HighlightSync.Instance != null)
            {
                Transform tableOrigin = HighlightSync.Instance.tableOrigin;
                Transform fieldOrigin = HighlightSync.Instance.fieldOrigin;

                float scaleFactor = HighlightSync.Instance.fieldCellSize / HighlightSync.Instance.tableCellSize;

                Vector3 offsetFromTable = transform.position - tableOrigin.position;
                Vector3 localOffset = Quaternion.Inverse(tableOrigin.rotation) * offsetFromTable;
                Vector3 scaledOffset = localOffset * scaleFactor;

                Vector3 basePos = fieldOrigin.position + (fieldOrigin.rotation * scaledOffset);
                bigTractor.transform.position = basePos + (fieldOrigin.up * (currentBobMultiplier * bigBobHeight));

                Quaternion localRot = Quaternion.Inverse(tableOrigin.rotation) * transform.rotation;
                bigTractor.transform.rotation = fieldOrigin.rotation * localRot;

                Quaternion baseLocalRot = Quaternion.Inverse(tableOrigin.rotation) * transform.rotation;
                bigTractor.transform.rotation = fieldOrigin.rotation * baseLocalRot * Quaternion.Euler(0, 0, currentLeanAngle);
            }
        }

        public void ActivateTractor()
        {
            if (automationRoutine == null)
            {
                automationRoutine = StartCoroutine(AutomationSequence());
            }
        }
        public void StopTractor()
        {
            if (automationRoutine != null)
            {
                StopCoroutine(automationRoutine);
                automationRoutine = null;
            }
            SetVisualsActive(false);
        }
        private void SetVisualsActive(bool isActive)
        {
            if (miniTractor != null) miniTractor.SetActive(isActive);
            if (bigTractor != null) bigTractor.SetActive(isActive);

            if (engineAudio != null)
            {
                if (isActive && !engineAudio.isPlaying) engineAudio.Play();
                else if (!isActive && engineAudio.isPlaying) engineAudio.Stop();
            }
        }

        private IEnumerator AutomationSequence()
        {
            while (true)
            {
                transform.position = GetTableWorldPosition(snakingPath[0]);
                SetVisualsActive(true);

                for (int i = 0; i < snakingPath.Count; i++)
                {
                    Vector2Int currentCell = snakingPath[i];
                    Vector3 targetPos = GetTableWorldPosition(currentCell);

                    if (Vector3.Distance(transform.position, targetPos) > 0.01f)
                    {
                        yield return StartCoroutine(DriveTo(targetPos));
                    }

                    FieldActionManager.Instance.AutoCollectCell(currentCell);
                }

                SetVisualsActive(false);
                transform.position = transform.position + (Vector3.down * 10f);
                yield return new WaitForSeconds(loopCooldown);
            }
        }
        private IEnumerator DriveTo(Vector3 targetPos)
        {
            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;

            Vector3 direction = (targetPos - startPos).normalized;
            Quaternion targetRot = startRot;
            float turnDir = 0f;

            if (direction != Vector3.zero && HighlightSync.Instance != null)
            {
                targetRot = Quaternion.LookRotation(-direction, HighlightSync.Instance.tableOrigin.up);

                turnDir = Vector3.SignedAngle(startRot * Vector3.forward, targetRot * Vector3.forward, HighlightSync.Instance.tableOrigin.up);
            }

            float elapsedTime = 0f;
            while (elapsedTime < moveSpeed)
            {
                float progress = elapsedTime / moveSpeed;

                currentBobMultiplier = Mathf.Abs(Mathf.Sin(elapsedTime * bobSpeed));

                float turnProgress = Mathf.Clamp01(progress * 1.5f);

                float leanCurve = Mathf.Sin(turnProgress * Mathf.PI);

                currentLeanAngle = (turnDir / 90f) * maxLeanAngle * leanCurve;

                transform.position = Vector3.Lerp(startPos, targetPos, progress);
                transform.rotation = Quaternion.Slerp(startRot, targetRot, Mathf.Clamp01(progress * 1.5f));

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.position = targetPos;
            transform.rotation = targetRot;
            currentBobMultiplier = 0f;
            currentLeanAngle = 0f;
        }

        private List<Vector2Int> GenerateSnakingPath(int cols, int rows)
        {
            List<Vector2Int> path = new List<Vector2Int>();
            for (int z = 0; z < rows; z++)
            {
                // right on even rows, left on odd
                if (z % 2 == 0)
                {
                    for (int x = 0; x < cols; x++) path.Add(new Vector2Int(x, z));
                }
                else
                {
                    for (int x = cols - 1; x >= 0; x--) path.Add(new Vector2Int(x, z));
                }
            }
            return path;
        }

        private Vector3 GetTableWorldPosition(Vector2Int cell)
        {
            Transform tableOrigin = HighlightSync.Instance.tableOrigin;
            float tSize = HighlightSync.Instance.tableCellSize;

            return tableOrigin.position
                + (tableOrigin.right * (cell.x * tSize))
                + (tableOrigin.forward * (cell.y * tSize));
        }
    }
}

