using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Sharyq
{
    public class Customer : MonoBehaviour
    {
        [SerializeField] private GameObject tipPrefab;
        [SerializeField] private GameObject leftoverPrefab;
        
        private NavMeshAgent _agent;
        private Seat _seat;
        private Transform _exitPoint;
        
        [Header("Customer Stats")]
        public float hungerLevel;
        public float kindnessLevel;
        public float richnessLevel;
        public float speedLevel;

        public void Awake()
        {
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
            yield return new WaitForSeconds(10f * speedLevel);
            
            // tipping
            if (Random.value < kindnessLevel)
            {
                var tip = Instantiate(tipPrefab, _seat.tipDropPoint);
                var tipStat = tip.GetComponent<TipStat>();
                if (tipStat != null)
                    tipStat.TipAmount = (int) Math.Round(100 * richnessLevel);
            }
            
            if (Random.value < hungerLevel)
            {
                var leftover = Instantiate(leftoverPrefab, _seat.leftOverDropPoint);
            }
            
            //Leave my establishment so i can serve more people lmao
            _seat.Release();
            _seat = null;
            _agent.updateRotation = true;
 
            yield return WalkTo(_exitPoint.position);
            Destroy(gameObject);

        }

        private IEnumerator WalkTo(Vector3 destination)
        {
            _agent.SetDestination(destination);
            
            while (_agent.pathPending) yield return null;
 
            while (_agent.remainingDistance > _agent.stoppingDistance + 0.05f)
            {
                if (_agent.pathStatus == NavMeshPathStatus.PathInvalid) yield break;
                yield return null;
            }
        }
        
        private void OnDestroy()
        {
            if (_seat != null) _seat.Release();
        }

    }
}
