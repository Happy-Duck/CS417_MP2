using UnityEngine;
using UnityEngine.Events;

namespace Sharyq
{
    public class ResourceProducingStat : MonoBehaviour
    {
        public UnityEvent finishedCooking = new UnityEvent();
        
        [SerializeField] private float timeTillDone = 5.0f;
        [SerializeField] private float increaseRate = 1.0f;
        private bool _active = false;
        public float currentTime = 0.0f;
        
        public bool IsActive => _active;
        public float Progress => timeTillDone > 0f ? Mathf.Clamp01(currentTime / timeTillDone) : 0f;

        private void Update()
        {
            if (_active)
            {
                currentTime += Time.deltaTime * increaseRate;
                if (currentTime >= timeTillDone) ProductionFinished();
            }
        }
 
        public void StartProduction()
        {
            _active = true;
            currentTime = 0.0f;
        }
        
        private void ProductionFinished()
        {
            _active = false;
            currentTime = 0.0f;
            finishedCooking.Invoke();
        }
    }
}

