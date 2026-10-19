using System.Collections;
using Shared;
using UnityEngine;

namespace Sharyq
{
    public class RentStat : MonoBehaviour
    {
        [Header("Rent Settings")]
        [SerializeField] private int rentAmount;
        [SerializeField] private float timeTillRentIsDue = 120f;
        private float _currentTime = 0f;

        public int RentAmount => rentAmount;
        public float TimeRemaining => Mathf.Max(0f, timeTillRentIsDue - _currentTime);

        private void Update()
        {
            _currentTime += Time.deltaTime;

            if (_currentTime >= timeTillRentIsDue)
            {
                GameOver();
            }
        }

        private void GameOver()
        {
            Debug.Log("Game Over: rent was not paid in time.");

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
        }

        public void PayRent()
        {
            if (!ResourceManager.Instance.CanAfford("Dollar", RentAmount))
                return;
            
            ResourceManager.Instance.TrySpend("Dollar", RentAmount);
            _currentTime = 0f;
            rentAmount = (int)Mathf.Floor(rentAmount * 1.5f);
        }
    }
}