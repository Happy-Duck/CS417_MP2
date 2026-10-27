using Shared;
using UnityEngine;

namespace Sharyq
{
    public class IsFirstWaffleSold : MonoBehaviour
    {
        private SellResource _sellResource;
        private void Awake()
        {
            if (!AchievementsManager.Instance.IsAchieved("Making Carbs into Cash"))
            {
                _sellResource = GetComponent<SellResource>();
                _sellResource.ItemSoldEvent.AddListener(FirstWaffleSold);
            }
        }

        private void FirstWaffleSold()
        {
            _sellResource.ItemSoldEvent.RemoveListener(FirstWaffleSold);
            AchievementsManager.Instance.CompleteAchievement("Making Carbs into Cash");
        }
    }
}

