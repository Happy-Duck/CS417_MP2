using System.Collections.Generic;
using Shared;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Sharyq
{
    [System.Serializable]
    public struct EggYield
    {
        public EggType eggType;
        public int batter;
    }

    public class MixingStation : MonoBehaviour, IAutomatable
    {
        [Header("Setup")]
        [SerializeField] private XRSocketInteractor socket;

        [Header("Recipe")]
        [SerializeField] private IngredientType[] requiredIngredients =
            { IngredientType.Egg, IngredientType.Wheat };
        [SerializeField] private float mixDuration = 3f;
        [SerializeField] private int batterPerMix = 1;
        [SerializeField] private EggYield[] eggYields =
        {
            new EggYield { eggType = EggType.White, batter = 1 },
            new EggYield { eggType = EggType.Brown, batter = 2 },
            new EggYield { eggType = EggType.Red,   batter = 3 },
            new EggYield { eggType = EggType.Blue,  batter = 5 },
        };
        [SerializeField] private float progress = 0f;
        public float Progress => progress;

        private IngredientPopup popup;
        private ResourceNotifier finishedPopup;
        private XRBaseInteractable _currentInteractable;
        private EggType _currentEggType = EggType.White;

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

        private int GetBatterYield(EggType type)
        {
            foreach (var y in eggYields)
                if (y.eggType == type) return y.batter;
            return batterPerMix;
        }

        private void OnIngredientPlaced(SelectEnterEventArgs args)
        {
            if (IsMixing) return;

            Ingredient ingredient = args.interactableObject.transform.GetComponentInParent<Ingredient>();
            if (ingredient == null) return;

            // Only accept types the recipe needs and doesn't have
            if (!System.Array.Exists(requiredIngredients, t => t == ingredient.type)) return;
            if (!added.Add(ingredient.type)) return;

            if (ingredient is Egg egg)
                _currentEggType = egg.EggType;

            if (popup != null)
                popup.ShowIngredient(ingredient.type.ToString());

            Destroy(ingredient.gameObject);

            if (added.Count >= requiredIngredients.Length)
                StartMixing();
        }

        private void Update()
        {
            if (!IsMixing) return;
            
            float rate = mixDuration > 0f ? 1f / mixDuration : float.PositiveInfinity;
            progress += rate * Time.deltaTime;

            if (progress >= 1f)
                FinishMixing();
        }

        private void StartMixing()
        {
            bool achieved = AchievementsManager.Instance.IsAchieved("Increasing Production");
            if (_currentEggType == EggType.Blue && achieved)
                AchievementsManager.Instance.CompleteAchievement("Increasing Production");
            IsMixing = true;
            progress = 0f;
            SetInteractionEnabled(false);
            onMixingStarted?.Invoke();
        }

        private void FinishMixing()
        {
            progress = 1f;
            added.Clear();
            IsMixing = false;
            SetInteractionEnabled(true);

            int batter = GetBatterYield(_currentEggType);

            ResourceManager.Instance.Add("Batter", batter);
            onMixingFinished?.Invoke(batter);

            if (finishedPopup != null)
                finishedPopup.AddedResource("Batter", batter);

            progress = 0f;
            _currentEggType = EggType.White;
        }

        private void SetInteractionEnabled(bool value)
        {
            if (_currentInteractable != null)
                _currentInteractable.enabled = value;
            if (socket != null)
                socket.enabled = value;
        }

        public bool TryAutomate(Employee employee)
        {
            if (IsMixing || added.Count > 0) return false;

            if (ResourceManager.Instance.Get("Wheat") <= 0)
                return false;

            EggType? best = null;
            int bestYield = -1;
            foreach (EggType t in System.Enum.GetValues(typeof(EggType)))
            {
                if (ResourceManager.Instance.Get($"{t} Egg") <= 0) continue;
                int y = GetBatterYield(t);
                if (y > bestYield)
                {
                    bestYield = y;
                    best = t;
                }
            }

            if (best == null)
                return false;

            ResourceManager.Instance.TrySpend($"{best.Value} Egg", 1);
            ResourceManager.Instance.TrySpend("Wheat", 1);

            _currentEggType = best.Value;
            StartMixing();
            return true;
        }
    }
}