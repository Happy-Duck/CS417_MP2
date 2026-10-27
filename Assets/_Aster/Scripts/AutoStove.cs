
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using Shared;

namespace Aster
{
    public class AutoStove : MonoBehaviour
    {
        [Header("METH Recipe")]
        [SerializeField] private Recipe METHRecipe;

        [Header("UI Text")]
        [SerializeField] private TMP_Text ingredientsText;
        [SerializeField] private TMP_Text METHReadyText;

        [Header("Optional Buttons")]
        [SerializeField] private GameObject collect;
        private int pending;

        private readonly StringBuilder textBuilder = new StringBuilder();

        private void Start()
        {
            RefreshUI();
        }

        // Connect this to the Turn In button's OnClick event.
        public void TurnInIngredients()
        {
            if (METHRecipe == null || METHRecipe.inputs == null ||
                METHRecipe.inputs.Count == 0)
            {
                return;
            }

            // Combine duplicate ingredient entries so the calculation
            // also works if the recipe lists the same resource twice.
            Dictionary<string, int> required = GetCombinedRequirements();

            if (required.Count == 0)
            {
                RefreshUI();
                return;
            }

            // Find the maximum number of complete recipes possible.
            int possibleBatches = int.MaxValue;
            ResourceManager rm = ResourceManager.Instance;

            foreach (KeyValuePair<string, int> item in required)
            {
                if (item.Value <= 0)
                    continue;

                int available = rm.Get(item.Key);
                int batchesForIngredient = available / item.Value;

                possibleBatches = Mathf.Min(
                    possibleBatches, batchesForIngredient);
            }

            if (possibleBatches <= 0 || possibleBatches == int.MaxValue)
            {
                RefreshUI();
                return;
            }

            // Spend all ingredients needed for every possible batch.
            foreach (KeyValuePair<string, int> item in required)
            {
                int totalNeeded = item.Value * possibleBatches;

                if (!rm.TrySpend(item.Key, totalNeeded))
                {
                    // Normally shouldn't happen because we checked
                    // inventory above. See note below if inventory can
                    // change from other scripts at the same time.
                    Debug.LogWarning(
                        "AutoStove: Could not spend " + item.Key);
                    RefreshUI();
                    return;
                }
            }

            // Add all produced output to the uncollected amount.
            pending += possibleBatches * METHRecipe.outputAmount;

            RefreshUI();
        }

        // Connect this to the Collect METH button's OnClick event.
        public void CollectMETH()
        {
            if (pending <= 0)
                return;

            ResourceManager.Instance.Add(
                METHRecipe.output, pending);

            pending = 0;
            RefreshUI();
        }

        private Dictionary<string, int> GetCombinedRequirements()
        {
            Dictionary<string, int> requirements =
                new Dictionary<string, int>();

            foreach (var ingredient in METHRecipe.inputs)
            {
                if (ingredient.count <= 0)
                    continue;

                if (requirements.ContainsKey(ingredient.resource))
                    requirements[ingredient.resource] += ingredient.count;
                else
                    requirements.Add(ingredient.resource, ingredient.count);
            }

            return requirements;
        }

        private void RefreshUI()
        {
            if (METHRecipe == null || METHRecipe.inputs == null)
                return;

            textBuilder.Clear();
            textBuilder.AppendLine("<b>METH</b>");

            Dictionary<string, int> requirements = GetCombinedRequirements();

            foreach (KeyValuePair<string, int> item in requirements)
            {
                int available = ResourceManager.Instance.Get(item.Key);

                textBuilder.Append(item.Key)
                    .Append(": ")
                    .Append(available)
                    .Append("/")
                    .Append(item.Value);
            }

            int possibleBatches = int.MaxValue;

            foreach (KeyValuePair<string, int> item in requirements)
            {
                if (item.Value <= 0)
                    continue;

                int batches = ResourceManager.Instance.Get(item.Key)
                    / item.Value;

                possibleBatches = Mathf.Min(possibleBatches, batches);
            }

            if (possibleBatches == int.MaxValue)
                possibleBatches = 0;

            if (ingredientsText != null)
                ingredientsText.text = textBuilder.ToString();

            if (METHReadyText != null)
            {
                METHReadyText.text = METHRecipe.output
                    + " ready to collect: " + pending;
            }

            if (collect != null)
                collect.SetActive(pending > 0);
        }
    }
}
