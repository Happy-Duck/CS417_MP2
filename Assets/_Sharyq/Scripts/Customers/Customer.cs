using System;
using System.Collections;
using Shared;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Sharyq
{
    public class Customer : MonoBehaviour
    {
        public static int ActiveCount { get; private set; }
        public static event Action<int> OnCountChanged;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            ActiveCount = 0;
            OnCountChanged = null;
        }

        //Fullness Stat
        [SerializeField] private float fullness = 0f;
        [SerializeField] private GameObject tipPrefab;
        [SerializeField] private GameObject leftoverPrefab;
        [SerializeField] private GameObject notifPrefab;

        private NavMeshAgent _agent;
        private Seat _seat;
        private Transform _exitPoint;
        private Animator _animator;

        [Header("Customer Stats")]
        public float hungerLevel;
        public float kindnessLevel;
        public float richnessLevel;
        public float speedLevel;

        public float Fullness => fullness;

        public void Awake()
        {
            ActiveCount++;
            if (ActiveCount == 20 && !AchievementsManager.Instance.IsAchieved("Lunch Rush"))
                AchievementsManager.Instance.CompleteAchievement("Lunch Rush");
            OnCountChanged?.Invoke(ActiveCount);

            // Init Stats related to how fast they eat,
            // and how likely they will give
            // Leftovers and tip
            hungerLevel = Random.Range(0f, 1.0f);
            kindnessLevel = Random.Range(0f, 1.0f);
            richnessLevel = Random.Range(0f, 1.0f);
            speedLevel = Random.Range(0f, 1.0f);

            // Nav Agent stuff
            _agent = GetComponent<NavMeshAgent>();
            _agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
            _agent.stoppingDistance = 0.3f;
            _animator = GetComponent<Animator>();
        }

        public void Init(Seat seat, Transform exitPoint)
        {
            _seat = seat;
            _exitPoint = exitPoint;
            StartCoroutine(StartBehavior());
        }

        private IEnumerator StartBehavior()
        {
            // Go To Table
            yield return WalkTo(_seat.transform.position);

            _agent.updateRotation = false;
            transform.rotation = _seat.transform.rotation;

            // Eating Logic
            yield return FillRoutine();

            // tipping
            Vector3 tipPos = _seat.GetRandomTablePoint();
            if (Random.value < kindnessLevel)
            {
                var tip = Instantiate(tipPrefab, tipPos, RandomYaw());
                var tipStat = tip.GetComponent<TipStat>();
                if (tipStat != null)
                    tipStat.TipAmount = (int)Math.Round(100 * richnessLevel);
            }

            if (Random.value < hungerLevel)
            {
                Vector3 leftoverPos = GetPointAwayFrom(tipPos, 0.15f);
                Instantiate(leftoverPrefab, leftoverPos, RandomYaw());
            }

            //Leave my establishment so i can serve more people lmao
            var notif = Instantiate(notifPrefab, tipPos, Quaternion.identity).GetComponent<FinishedNotifier>();
            notif.TriggerNotification();
            _seat.Release();
            _seat = null;
            _agent.updateRotation = true;

            yield return WalkTo(_exitPoint.position);
            Destroy(gameObject);
        }

        private IEnumerator FillRoutine()
        {
            float duration = 10f * speedLevel;
            fullness = 0f;

            if (duration <= 0f)
            {
                fullness = 1f;
                yield break;
            }

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                fullness = Mathf.Clamp01(t / duration);
                yield return null;
            }

            fullness = 1f;
        }

        private static Quaternion RandomYaw() =>
            Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        private Vector3 GetPointAwayFrom(Vector3? other, float minDistance, int attempts = 10)
        {
            Vector3 point = _seat.GetRandomTablePoint();
            if (other == null) return point;

            for (int i = 0; i < attempts; i++)
            {
                if (Vector3.Distance(point, other.Value) >= minDistance) break;
                point = _seat.GetRandomTablePoint();
            }
            return point;
        }

        private IEnumerator WalkTo(Vector3 destination)
        {
            _agent.SetDestination(destination);

            _animator.SetBool("IsRunning", true);

            while (_agent.pathPending) yield return null;

            while (_agent.remainingDistance > _agent.stoppingDistance + 0.05f)
            {
                if (_agent.pathStatus == NavMeshPathStatus.PathInvalid) yield break;
                yield return null;
            }
            _animator.SetBool("IsRunning", false);
        }

        private void OnDestroy()
        {
            if (_seat != null) _seat.Release();

            ActiveCount--;
            OnCountChanged?.Invoke(ActiveCount);
        }
    }
}