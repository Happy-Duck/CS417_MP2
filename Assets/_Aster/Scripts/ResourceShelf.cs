using Aster;
using Shared;
using UnityEngine;

namespace Aster
{
    public class ResourceShelf : MonoBehaviour
    {
        public string resourceName;
        public GameObject[] slots;          // visual items,, one per unit of stock
        public string costResource = "Gold";
        public int restockCost = 5;
        public GameObject restockButton;    // only shown when the shelf is empty
        public int stock;                   // stat stock

        private ResourceAchievement resourceAchievement;

        void Start()
        {
            stock = slots.Length;
            Refresh();
        }

        public void Interact()
        {
            if (stock <= 0) return;
            stock--;
            ResourceManager.Instance.Add(resourceName, 1);
            AchievementsManager.Instance.CompleteAchievement("What groceries u been into lately");

            resourceAchievement = FindFirstObjectByType<ResourceAchievement>();
            Refresh();
        }

        public void Restock()
        {
            if (stock > 0) return;
            if (!ResourceManager.Instance.TrySpend(costResource, restockCost)) return;
            AchievementsManager.Instance.CompleteAchievement("First Restock");
            stock = slots.Length;
            Refresh();
        }

        void Refresh()
        {
            for (int i = 0; i < slots.Length; i++)
                if (slots[i] != null) slots[i].SetActive(i < stock);
            if (restockButton != null) restockButton.SetActive(stock == 0);
        }
    }
}
