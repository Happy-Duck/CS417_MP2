using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

namespace Sharyq
{
    public class FinishedNotifier : MonoBehaviour
    {
        [SerializeField] TMP_Text label;
        [SerializeField] CanvasGroup group;
        [SerializeField] float fadeIn = 0.2f;
        [SerializeField] float hold = 1.5f;
        [SerializeField] float fadeOut = 0.5f;
        [SerializeField] bool deleteOnFinish = true;

        [Header("Facing")]
        [SerializeField] bool faceOnLoad = true;
        [SerializeField] bool faceOnEachAdd = true;
        [SerializeField] bool keepUpright = true;
        
        Coroutine routine;
        Transform _cam;

        void Awake()
        {
            group.alpha = 0;
        }

        void Start()
        {
            if (faceOnLoad)
                FacePlayer();
        }

        public void TriggerNotification()
        {
            if (faceOnEachAdd)
                FacePlayer();

            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(Run());
        }
        

        void FacePlayer()
        {
            if (_cam == null)
            {
                var main = Camera.main;
                if (main == null) return;
                _cam = main.transform;
            }

            // World-space canvases render facing +Z, so point +Z away from the camera
            Vector3 dir = transform.position - _cam.position;
            if (keepUpright)
                dir.y = 0f;

            if (dir.sqrMagnitude < 0.0001f) return;

            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }

        IEnumerator Run()
        {
            yield return Fade(group.alpha, 1f, fadeIn);
            yield return new WaitForSeconds(hold);
            yield return Fade(1f, 0f, fadeOut);
            routine = null;

            if (deleteOnFinish)
                Destroy(gameObject);
        }

        IEnumerator Fade(float from, float to, float duration)
        {
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                group.alpha = Mathf.Lerp(from, to, t / duration);
                yield return null;
            }

            group.alpha = to;
        }
    }
}