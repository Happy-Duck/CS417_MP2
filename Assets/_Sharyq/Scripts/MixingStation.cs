using System.Collections;
using System.Collections.Generic;
using Shared;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Sharyq
{
    public class MixingStation : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] private XRSocketInteractor socket;

        [Header("Recipe")]
        [SerializeField] private IngredientType[] requiredIngredients =
            { IngredientType.Egg, IngredientType.Wheat };
        [SerializeField] private float mixDuration = 3f;
        [SerializeField] private int batterPerMix = 1;

        private IngredientPopup popup;
        private ResourceNotifier finishedPopup;
        private XRBaseInteractable _currentInteractable;

        public bool IsMixing { get; private set; }

        public UnityEvent onMixingStarted;
        public UnityEvent<int> onMixingFinished;

        private readonly HashSet<IngredientType> added = new HashSet<IngredientType>();

        private void Awake()
        {
            if (socket == null)
                socket = GetComponentInChildren<XRSocketInteractor>();

            if (popup == null)
                popup = GetComponentInChildren<IngredientPopup>();

            if (finishedPopup == null)
                finishedPopup = GetComponentInChildren<ResourceNotifier>();

            if (_currentInteractable == null)
                _currentInteractable = GetComponent<XRBaseInteractable>();

            if (socket == null)
                Debug.LogError($"{name}: No XRSocketInteractor found in children.", this);

            if (_currentInteractable == null)
                Debug.LogWarning($"{name}: No XRBaseInteractable found on this object.", this);
        }

        private void OnEnable()
        {
            if (socket != null)
                socket.selectEntered.AddListener(OnIngredientPlaced);
        }

        private void OnDisable()
        {
            if (socket != null)
                socket.selectEntered.RemoveListener(OnIngredientPlaced);
        }

        private void OnIngredientPlaced(SelectEnterEventArgs args)
        {
            if (IsMixing) return;

            Ingredient ingredient = args.interactableObject.transform.GetComponentInParent<Ingredient>();
            if (ingredient == null) return;

            // Only accept types the recipe needs and doesn't have
            if (!System.Array.Exists(requiredIngredients, t => t == ingredient.type)) return;
            if (!added.Add(ingredient.type)) return;

            if (popup != null)
                popup.ShowIngredient(ingredient.type.ToString());

            Destroy(ingredient.gameObject);

            if (added.Count >= requiredIngredients.Length)
                StartCoroutine(MixRoutine());
        }

        private IEnumerator MixRoutine()
        {
            IsMixing = true;
            SetInteractionEnabled(false);
            onMixingStarted?.Invoke();

            yield return new WaitForSeconds(mixDuration);

            added.Clear();
            IsMixing = false;
            SetInteractionEnabled(true);

            ResourceManager.Instance.Add("Batter", batterPerMix);
            onMixingFinished?.Invoke(batterPerMix);

            if (finishedPopup != null)
                finishedPopup.AddedResource("Batter", batterPerMix);
        }

        private void SetInteractionEnabled(bool value)
        {
            if (_currentInteractable != null)
                _currentInteractable.enabled = value;
            if (socket != null)
                socket.enabled = value;
        }
    }
}