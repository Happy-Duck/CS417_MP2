
using Shared;
using UnityEngine;

namespace Aster
{
    public class GasolineShelfUnlock : MonoBehaviour
    {
        [SerializeField] private GameObject gasolineShelf;

        private const string AchievementName = "Mmm...";

        private void Start()
        {
            // Subscribe to future achievement unlocks.
            if (AchievementsManager.Instance != null)
            {
                AchievementsManager.Instance.onAchieved.AddListener(
                    OnAchievementUnlocked
                );

                // Handle a potion achievement earned before this script started.
                if (AchievementsManager.Instance.IsAchieved(AchievementName))
                {
                    UnlockShelf();
                }
                else if (gasolineShelf != null)
                {
                    gasolineShelf.SetActive(false);
                }
            }
        }

        private void OnDestroy()
        {
            if (AchievementsManager.Instance != null)
            {
                AchievementsManager.Instance.onAchieved.RemoveListener(
                    OnAchievementUnlocked
                );
            }
        }

        private void OnAchievementUnlocked(string name, string description)
        {
            if (name == AchievementName)
            {
                UnlockShelf();
            }
        }

        private void UnlockShelf()
        {
            if (gasolineShelf != null)
            {
                gasolineShelf.SetActive(true);
                Debug.Log("gasoline unlocked");
            }
        }
    }
}
