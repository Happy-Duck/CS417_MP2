using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Sharyq
{
    public class CustomerSpawner : MonoBehaviour
    {
        [SerializeField] private Customer customerPrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform exitPoint;
        [SerializeField] private List<Seat> seats = new List<Seat>();

        public void Awake()
        {
            seats.AddRange(FindObjectsByType<Seat>());
        }

        public bool HasFreeSeat
        {
            get
            {
                foreach (Seat s in seats)
                    if (!s.Occupied) return true;
                return false;
            }
        }
 
        // disabled seat occupied logic, not enough time to worry about right now
        public bool TrySpawnCustomer()
        {
            if (seats.Count == 0) return false;

            Seat s = seats[Random.Range(0, seats.Count)];
            Customer c = Instantiate(customerPrefab, spawnPoint.position, spawnPoint.rotation);
            c.Init(s, exitPoint);
            return true;
        }
    }

}
