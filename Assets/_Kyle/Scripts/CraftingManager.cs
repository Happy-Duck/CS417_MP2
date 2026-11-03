using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using Shared;
using System.Collections.Generic;
using TMPro;

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
        private ErrorCatchingHint error;
        [SerializeField] private List<CraftingRecipe> availableRecipes;
        [SerializeField] AudioClip craftSound;
        [SerializeField] TextMeshProUGUI craftText;

        [Header("Socket References")]
        [SerializeField] private XRSocketInteractor[] craftingSockets;

        public int craftNum = 1;

        private void Start()
        {
            error = GetComponent<ErrorCatchingHint>();
        }

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
                        if (!currentItems.ContainsKey(item.ResourceKey))
                            currentItems[item.ResourceKey] = 0;

                        currentItems[item.ResourceKey]++;
                        totalItemsInSockets++;
                    }
                }
            }

            CraftingRecipe matchedRecipe = null;
            foreach (CraftingRecipe recipe in availableRecipes)
            {
                if (IsRecipeMatch(recipe, currentItems, totalItemsInSockets))
                {
                    matchedRecipe = recipe;
                    break;
                }
            }

            if (matchedRecipe == null)
            {
                error.message = "Items in sockets don't match any base recipe!";
                error.ShowError();
                return;
            }

            int actualCrafts = craftNum - 1;
            if (actualCrafts > 0)
            {
                foreach (ResourceAmount req in matchedRecipe.ingredients)
                {
                    int availableInInventory = ResourceManager.Instance.Get(req.resourceName);
                    int maxBatchesForThisIngredient = availableInInventory / req.amount;

                    if (maxBatchesForThisIngredient < actualCrafts)
                    {
                        actualCrafts = maxBatchesForThisIngredient;
                    }
                }
            }

            int totalActualCrafts = actualCrafts + 1;
            if (totalActualCrafts > 0)
            {
                ExecuteRecipe(matchedRecipe, totalActualCrafts);

                if (totalActualCrafts < craftNum)
                {
                    error.message = $"Not enough resources for {craftNum}! Crafted {actualCrafts} instead.";
                    error.ShowError();
                }
            }
            else
            {
                error.message = "You don't have enough resources in your inventory!";
                error.ShowError();
            }
        }

        private bool IsRecipeMatch(CraftingRecipe recipe, Dictionary<string, int> socketItems, int totalSocketItems)
        {
            int totalRecipeItems = 0;
            foreach (ResourceAmount req in recipe.ingredients)
            {
                if (!socketItems.ContainsKey(req.resourceName) || socketItems[req.resourceName] != req.amount)
                {
                    return false;
                }
                totalRecipeItems += req.amount;
            }

            return totalSocketItems == totalRecipeItems;
        }

        private void ExecuteRecipe(CraftingRecipe recipe, int amountToCraft)
        {
            int numToDeduct = amountToCraft - 1;

            foreach (ResourceAmount req in recipe.ingredients)
            {
                ResourceManager.Instance.TrySpend(req.resourceName, req.amount * numToDeduct);
            }

            foreach (var socket in craftingSockets)
            {
                if (socket.interactablesSelected.Count > 0)
                {
                    GameObject itemObj = socket.interactablesSelected[0].transform.gameObject;
                    InventoryItem tracker = itemObj.GetComponent<InventoryItem>();

                    if (tracker != null)
                    {
                        tracker.MarkAsCrafted();
                    }

                    socket.enabled = false;
                    socket.enabled = true;
                    Destroy(itemObj);
                }
            }

            foreach (ResourceAmount res in recipe.results)
            {
                ResourceManager.Instance.Add(res.resourceName, res.amount * amountToCraft);
            }

            if (craftSound != null)
            {
                AudioSource.PlayClipAtPoint(craftSound, transform.position);
            }
        }

        public void IncreaseNum()
        {
            craftNum++;
            craftText.text = craftNum.ToString();
        }

        public void DecreaseNum()
        {
            if (craftNum > 1)
            {
                craftNum--;
            }
            craftText.text = craftNum.ToString();
        }
    }
}