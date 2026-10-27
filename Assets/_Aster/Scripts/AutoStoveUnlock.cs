
using Shared;
using UnityEngine;

namespace Aster
{
    public class AutoStoveUnlock : MonoBehaviour
    {
        
        [SerializeField] private GameObject upgradeButton;
        [SerializeField] private GameObject[] toDisable; // old beaker and stove
        [SerializeField] private GameObject autoStove;

        private bool alreadyUpgraded;

        private void Start()
        {
            if (upgradeButton != null)
                upgradeButton.SetActive(false);

            if (autoStove != null)
                autoStove.SetActive(false);
        }
        // first shipping goal reached
        public void OnGoalReached()
        {
            if (alreadyUpgraded)
                return;

            if (upgradeButton != null)
                upgradeButton.SetActive(true);
        }

        public void Upgrade()
        {
            if (alreadyUpgraded)
                return;

            alreadyUpgraded = true;
            if (upgradeButton != null)
                upgradeButton.SetActive(false);

            foreach (GameObject oldObject in toDisable)
            {
                if (oldObject != null)
                    oldObject.SetActive(false);
            }

            if (autoStove != null)
                autoStove.SetActive(true);

            if (AchievementsManager.Instance != null)
            {
                AchievementsManager.Instance
                    .CompleteAchievement("Quantity over Quality");
            }
        }

    }
}
