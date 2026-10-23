using Aster;
using Shared;
using UnityEngine;
using UnityEngine.Events;

namespace Aster
{
   
    public class Stove : MonoBehaviour
    {
        public Recipe recipe = new Recipe();  
        public float boilTime = 8f;
        public float boilProgress;        

        public GameObject steam;               // on while boiling
        public GameObject readyIndicator;      // on when ready to collect
        public StatBar boilBar;                // boil progress bar above the pot

        public UnityEvent onStarted, onCollected, onFailed;

        enum State { Empty, Boiling, Ready }
        State state = State.Empty;

        public void Interact()
        {
            switch (state)
            {
                case State.Empty:
                    if (!recipe.CanAffordFromResources()) { onFailed?.Invoke(); return; }
                    recipe.SpendFromResources();
                    boilProgress = 0f;
                    state = State.Boiling;
                    onStarted?.Invoke();
                    break;

                case State.Ready:
                    ResourceManager.Instance.Add(recipe.output, recipe.outputAmount);
                    boilProgress = 0f;
                    state = State.Empty;
                    onCollected?.Invoke();
                    break;
            }
        }

        void Update()
        {
            if (state == State.Boiling)
            {
                boilProgress += Time.deltaTime / boilTime; 
                if (boilProgress >= 1f) { boilProgress = 1f; state = State.Ready; }
            }

            if (steam != null) steam.SetActive(state == State.Boiling);
            if (readyIndicator != null) readyIndicator.SetActive(state == State.Ready);
            if (boilBar != null) boilBar.Set(boilProgress, 1f);
        }
    }
}
