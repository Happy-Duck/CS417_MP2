using Shared;
using UnityEngine;

namespace Sharyq
{
    public class UnlockEmployee : MonoBehaviour, IAchievementUnlockable
    {
        private GridPurchaseShelf _gridPurchaseShelf;

        private void Awake()
        {
            _gridPurchaseShelf = GetComponent<GridPurchaseShelf>();
            _gridPurchaseShelf.enabled = false;
            AchievementsManager.Instance.onAchieved.AddListener(Unlock);
        }

        public void Unlock(string achievementName, string achievementDescription)
        {
            if (achievementName == "This is Getting Repetitive")
                _gridPurchaseShelf.enabled = true;
        }
    }
}

