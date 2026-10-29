using Alyssa;
using Shared;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Aster
{

    public class UnlockStation : MonoBehaviour
    {
   
        public string requiredAchievement;            
        [TextArea] public string lockedText = "Locked";
        [TextArea] public string unlockedText = "Unlock";
        public TMP_Text label;

        public GameObject[] enableOnUnlock;         
        public GameObject[] disableOnUnlock;          // old content
        public GameObject buttonRoot;                 // hidden once used 
        public string grantAchievement; 
        public UnityEvent onUnlocked;              

        public string revealTitle;
        [TextArea] public string revealBody;

        bool used;

        bool Met => string.IsNullOrEmpty(requiredAchievement) ||
                    (AchievementsManager.Instance != null && AchievementsManager.Instance.IsAchieved(requiredAchievement));

        void Start()
        {
            if (buttonRoot == null) buttonRoot = gameObject;
            foreach (var g in enableOnUnlock) if (g != null) g.SetActive(false);
            if (AchievementsManager.Instance != null)
                AchievementsManager.Instance.onAchieved.AddListener(OnAchieved);
            Refresh();
        }

        void OnDestroy()
        {
            if (AchievementsManager.Instance != null)
                AchievementsManager.Instance.onAchieved.RemoveListener(OnAchieved);
        }

        void OnAchieved(string n, string d) { Refresh(); }

        void Refresh()
        {
            if (label != null) label.text = used ? "" : (Met ? unlockedText : lockedText);
        }

        public void Interact()
        {
            if (used) return;
            if (!Met) return;

            used = true;
            buttonRoot.SetActive(false);

            Apply();
           
        
        }

        void Apply()
        {
            foreach (var g in enableOnUnlock) if (g != null) g.SetActive(true);
            foreach (var g in disableOnUnlock) if (g != null) g.SetActive(false);
            onUnlocked?.Invoke();
            if(!string.IsNullOrEmpty(grantAchievement)) AchievementsManager.Instance.CompleteAchievement(grantAchievement);
        }
    }
}
