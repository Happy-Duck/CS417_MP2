using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sharyq
{
    public class Employee : MonoBehaviour
    {
        [SerializeField] private float range = 2.5f;
        [SerializeField] private float actionInterval = 1f;
        [SerializeField] private LayerMask stationMask = ~0;

        private readonly List<IAutomatable> _stations = new();
        private Coroutine _loop;
        
        public void OnPlaced()
        {
            if (_loop == null) _loop = StartCoroutine(WorkLoop());
        }

        private IEnumerator WorkLoop()
        {
            var wait = new WaitForSeconds(actionInterval);
            while (true)
            {
                ScanStations();
                foreach (var s in _stations)
                    if (s.TryAutomate()) break;
                yield return wait;
            }
        }

        private void ScanStations()
        {
            _stations.Clear();
            foreach (var hit in Physics.OverlapSphere(transform.position, range, stationMask))
            {
                var s = hit.GetComponentInParent<IAutomatable>();
                if (s != null && !_stations.Contains(s)) _stations.Add(s);
            }
        }

        // demo so i can see range in engine
        private void OnDrawGizmosSelected() => Gizmos.DrawWireSphere(transform.position, range);
    }
}