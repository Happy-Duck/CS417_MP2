using System;
using Shared;
using UnityEngine;

namespace Sharyq
{
    public class TrackTotalResourceSold : MonoBehaviour
    {
        [SerializeField]
        private string achievementName;
        [SerializeField]
        private string resourceName = "Waffle";
        [SerializeField]
        private int totalWafflesSold = 0;
        [SerializeField]
        private int thresholdWafflesSold = 30;
        private int _prevWaffles = 0;
        

        private void Awake()
        {
            if (!AchievementsManager.Instance.IsAchieved(achievementName))
                ResourceManager.Instance.OnResourceChanged += CheckRequirement;
        }

        private void Start()
        {
            _prevWaffles = ResourceManager.Instance.Get(resourceName);
        }

        private void CheckRequirement(string resource, int newAmount, int changedAmount)
        {
            if (AchievementsManager.Instance.IsAchieved(achievementName))
            {
                ResourceManager.Instance.OnResourceChanged -= CheckRequirement;
                return;
            }
            if (resource == resourceName && newAmount < _prevWaffles)
            {
                totalWafflesSold += 1;
                if (totalWafflesSold > newAmount)
                {
                    achievementName = "This is Getting Repetitive";
                    AchievementsManager.Instance.CompleteAchievement(achievementName);
                    ResourceManager.Instance.OnResourceChanged -= CheckRequirement;
                    return;
                }
                _prevWaffles = newAmount;
            }
        }

        private void OnDestroy()
        {
            if (ResourceManager.Instance != null) 
                ResourceManager.Instance.OnResourceChanged -= CheckRequirement;
        }
    }
}