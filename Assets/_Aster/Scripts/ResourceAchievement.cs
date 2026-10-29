using Alyssa;
using Shared;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Aster
{
    // Awards an achievement once the player has owned at least one of EVERY listed resource (at any time).
    // Listens to the ResourceManager, so it doesn't matter where the resource came from.
    // The Console tells you which resources are still missing every time one changes.
    public class ResourceAchievement : MonoBehaviour
    {
        [SerializeField] private string[] required;
        [SerializeField] private string achievementName = "Hoarder";

        private readonly HashSet<string> obtained = new HashSet<string>();
        private bool done;

        void Start()
        {
            var rm = ResourceManager.Instance;
            if (rm == null) { Debug.LogWarning("ResourceAchievement: no ResourceManager in the scene", this); return; }
            foreach (var r in required) if (rm.Get(r) > 0) obtained.Add(r);
            rm.OnResourceChanged += OnChanged;
            Check();
        }

        void OnDestroy()
        {
            if (ResourceManager.Instance != null)
                ResourceManager.Instance.OnResourceChanged -= OnChanged;
        }

        void OnChanged(string resource, int newAmount, int change)
        {
            if (change > 0 && Array.IndexOf(required, resource) >= 0)
            {
                obtained.Add(resource);
                Check();
            }
        }

        void Check()
        {
            if (required.Length == 0)
            {
                Debug.LogWarning($"ResourceAchievement ({achievementName}): the 'required' list is EMPTY, so it can never unlock.", this);
                return;
            }

            var missing = new List<string>();
            foreach (var r in required) if (!obtained.Contains(r)) missing.Add(r);
            if (missing.Count > 0)
            {
                Debug.Log($"ResourceAchievement ({achievementName}): still need {string.Join(", ", missing)}");
                return;
            }

            done = true;
            if (done) AchievementsManager.Instance.CompleteAchievement("Hoarder");
        }
    }
}