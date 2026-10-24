using System;
using System.Collections.Generic;
using Shared;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Sharyq
{
    public class ToppingStation : MonoBehaviour
    {
        [Serializable]
        public struct ToppingRecipe
        {
            public IngredientType ingredient;
            public string resultResource;
        }

        [SerializeField]
        private XRSocketInteractor socket;
        [SerializeField]
        private string baseResource = "Waffle";
        [SerializeField]
        private List<ToppingRecipe> recipes = new List<ToppingRecipe>();
        [SerializeField]
        private GameObject notif;
        [SerializeField]
        private Vector3 notifOffset = new Vector3(0f, 1.5f, 0f);
        [SerializeField]
        private UnityEvent<string> onToppingAdded;
        [SerializeField]
        private UnityEvent onFailed;

        private Dictionary<IngredientType, string> lookup;

        private void Reset()
        {
            socket = GetComponentInChildren<XRSocketInteractor>();

            recipes = new List<ToppingRecipe>
            {
                new ToppingRecipe { ingredient = IngredientType.Apple, resultResource = "Apple Waffle" },
                new ToppingRecipe { ingredient = IngredientType.Banana, resultResource = "Banana Waffle" },
                new ToppingRecipe { ingredient = IngredientType.Peach, resultResource = "Peach Waffle" },
                new ToppingRecipe { ingredient = IngredientType.Cherry, resultResource = "Cherry Waffle" },
                new ToppingRecipe { ingredient = IngredientType.Strawberry, resultResource = "Strawberry Waffle" },
                new ToppingRecipe { ingredient = IngredientType.Chicken, resultResource = "Chicken Waffle" },
                new ToppingRecipe { ingredient = IngredientType.Corn, resultResource = "Corn Waffle" },
                new ToppingRecipe { ingredient = IngredientType.Meth, resultResource = "Energizing Waffle" },
            };
        }

        private void Awake()
        {
            lookup = new Dictionary<IngredientType, string>();
            foreach (var r in recipes)
                lookup[r.ingredient] = r.resultResource;
        }

        public void OnButtonPressed()
        {
            Debug.Log("[Topping] button pressed");

            if (socket == null || !socket.hasSelection)
            {
                Debug.Log("[Topping] socket empty or null");
                onFailed?.Invoke();
                return;
            }

            var selected = socket.firstInteractableSelected;
            var ingredient = selected.transform.GetComponent<Ingredient>();

            if (ingredient == null || !lookup.TryGetValue(ingredient.type, out string result))
            {
                Debug.Log($"[Topping] no ingredient or recipe (ingredient: {ingredient})");
                Debug.Log($"[Topping] type={ingredient.type}, recipes={lookup.Count}, has={lookup.ContainsKey(ingredient.type)}");
                onFailed?.Invoke();
                return;
            }

            var resources = ResourceManager.Instance;
            if (resources == null || !resources.TrySpend(baseResource, 1))
            {
                Debug.Log($"[Topping] can't spend {baseResource} (manager null: {resources == null})");
                onFailed?.Invoke();
                return;
            }

            resources.Add(result, 1);

            if (notif != null)
            {
                var notifInst = Instantiate(notif, transform.position + notifOffset, transform.rotation)
                    .GetComponent<ResourceNotifier>();
                notifInst.AddedResource(result, 1);
            }

            socket.interactionManager.SelectExit(socket, selected);
            Destroy(selected.transform.gameObject);

            onToppingAdded?.Invoke(result);
        }
    }
}