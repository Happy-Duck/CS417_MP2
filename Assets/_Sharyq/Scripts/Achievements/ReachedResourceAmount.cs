using Shared;
using UnityEngine;

namespace Sharyq
{
    public class ReachedResourceAmount : MonoBehaviour
    {
        [SerializeField] 
        private int amount;
        [SerializeField]
        private string resourceName;
        [SerializeField]
        private string achievementName;

        private void Start()
        {
            ResourceManager.Instance.OnResourceChanged += CheckAmount;
        }

        private void CheckAmount(string resource, int newAmount, int change)
        {
            if (resource != resourceName)
                return;

            if (newAmount >= amount)
            {
                AchievementsManager.Instance.CompleteAchievement(achievementName);
                ResourceManager.Instance.OnResourceChanged -= CheckAmount;
            }
        }

        private void OnDestroy()
        {
            ResourceManager.Instance.OnResourceChanged -= CheckAmount;
        }
    }
}