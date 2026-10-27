using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using Shared;
using System.Collections.Generic;

namespace Kyle
{
    [System.Serializable]
    public struct ResourceAmount
    {
        public string resourceName;
        public int amount;
    }
    // crafting recipe class for ease of editing in the inspector
    [System.Serializable]
    public class CraftingRecipe
    {
        [Tooltip("ex. Corn Seed Recipe")]
        public string recipeName = "New Recipe";

        [Tooltip("Items needed to craft recipe")]
        public List<ResourceAmount> ingredients;

        [Tooltip("Resource gained when crafting")]
        public List<ResourceAmount> results;
    }
    public class CraftingManager : MonoBehaviour
    {
        [SerializeField] private List<CraftingRecipe> availableRecipes;

        [Header("Socket References")]
        [SerializeField] private XRSocketInteractor[] craftingSockets;

        public void TryCraft()
        {
            Dictionary<string, int> currentItems = new Dictionary<string, int>();
            int totalItemsInSockets = 0;

            foreach (var socket in craftingSockets)
            {
                if (socket.interactablesSelected.Count > 0)
                {
                    InventoryItem item = socket.interactablesSelected[0].transform.GetComponent<InventoryItem>();
                    if (item != null)
                    {
                        if (!currentItems.ContainsKey(item.ResourceName))
                            currentItems[item.ResourceName] = 0;

                        currentItems[item.ResourceName]++;
                        totalItemsInSockets++;
                    }
                }
            }

            // looking for  recipe
            foreach (CraftingRecipe recipe in availableRecipes)
            {
                if (CanCraftRecipe(recipe, currentItems, totalItemsInSockets))
                {
                    ExecuteRecipe(recipe);
                    return;
                }
            }
        }

        private bool CanCraftRecipe(CraftingRecipe recipe, Dictionary<string, int> currentItems, int totalItems)
        {
            int totalRecipeItems = 0;

            foreach (ResourceAmount req in recipe.ingredients)
            {
                if (!currentItems.ContainsKey(req.resourceName) || currentItems[req.resourceName] < req.amount)
                {
                    return false;
                }
                totalRecipeItems += req.amount;
            }

            if (totalItems != totalRecipeItems)
            {
                return false;
            }

            return true;
        }
        private void ExecuteRecipe(CraftingRecipe recipe)
        {
            foreach (ResourceAmount req in recipe.ingredients)
            {
                ConsumeItems(req.amount, req.resourceName);
            }

            foreach (ResourceAmount res in recipe.results)
            {
                ResourceManager.Instance.Add(res.resourceName, res.amount);
            }
        }

        private void ConsumeItems(int amountNeeded, string targetResourceName)
        {
            int destroyed = 0;
            foreach (var socket in craftingSockets)
            {
                if (socket.interactablesSelected.Count > 0 && destroyed < amountNeeded)
                {
                    GameObject item = socket.interactablesSelected[0].transform.gameObject;
                    InventoryItem tracker = item.GetComponent<InventoryItem>();

                    if (tracker != null && tracker.ResourceName == targetResourceName)
                    {
                        tracker.MarkAsCrafted();

                        socket.enabled = false;
                        socket.enabled = true;
                        Destroy(item);
                        destroyed++;
                    }
                }
            }
        }
    }
}