using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Shared
{
    /// <summary>
    /// Displays a notification whenever AchievementsManager.onAchieved triggers.
    /// setup achievement queue incase stuff triggers in parrallel
    /// </summary>
    public class AchievementNotif : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] 
        private CanvasGroup canvasGroup;
        [SerializeField] 
        private TMP_Text nameText;
        [SerializeField] 
        private TMP_Text descriptionText;

        [Header("Timing")]
        [SerializeField] 
        private float fadeDuration = 0.45f;
        [SerializeField] 
        private float holdDuration = 3.5f;
        [SerializeField] 
        private float gapBetweenNotifs = 0.25f;

        private AudioSource _audioSource;

        private struct NotificationInfo
        {
            public string Name;
            public string Description;
        }

        private readonly Queue<NotificationInfo> _achievementQueue = new Queue<NotificationInfo>();
        private bool _isShowing;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            canvasGroup.alpha = 0f;
        }
        
        private void Start()
        {
            if (AchievementsManager.Instance == null)
            {
                Debug.LogWarning("Achievement Manager not Found");
                return;
            }
            
            AchievementsManager.Instance.onAchieved.AddListener(Enqueue);
        }

        private void OnDestroy()
        {
            if (AchievementsManager.Instance != null)
                AchievementsManager.Instance.onAchieved.RemoveListener(Enqueue);
        }

        private void Enqueue(string achievementName, string description)
        {
            _achievementQueue.Enqueue(new NotificationInfo { Name = achievementName, Description = description });
            if (!_isShowing) StartCoroutine(ProcessQueue());
        }

        private IEnumerator ProcessQueue()
        {
            _isShowing = true;

            while (_achievementQueue.Count > 0)
            {
                NotificationInfo notificationInfo = _achievementQueue.Dequeue();
                nameText.text = "Unlocked: " + notificationInfo.Name;
                descriptionText.text = notificationInfo.Description;

                if (_audioSource != null)
                    _audioSource.Play();

                yield return Fade(0f, 1f);
                yield return new WaitForSecondsRealtime(holdDuration);
                yield return Fade(1f, 0f);
                yield return new WaitForSecondsRealtime(gapBetweenNotifs);
            }

            _isShowing = false;
        }

        private IEnumerator Fade(float alphaFrom, float alphaTo)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(alphaFrom, alphaTo, Mathf.Clamp01(elapsed / fadeDuration));
                yield return null;
            }

            canvasGroup.alpha = alphaTo;
        }
    }
}