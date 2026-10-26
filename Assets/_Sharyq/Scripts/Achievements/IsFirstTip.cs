using Shared;
using UnityEngine;

namespace Sharyq
{
    public class IsFirstTip : MonoBehaviour
    {
        private TipStat _tipStat;
        private void Awake()
        {
            if (!AchievementsManager.Instance.IsAchieved("Supporting Workers"))
            {
                _tipStat = GetComponent<TipStat>();
                _tipStat.onResourcePickup.AddListener(TipCollected);
            }
        }

        private void TipCollected()
        {
            _tipStat.onResourcePickup.RemoveListener(TipCollected);
            AchievementsManager.Instance.CompleteAchievement("Supporting Workers");
        }
    }
}