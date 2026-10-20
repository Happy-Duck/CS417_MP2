using System;
using Shared;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace Sharyq {
    /// <summary>
    /// Based off Rishi's Version customized for Managers Display Board
    /// So that I can work with the global resource system setup
    /// Using Events provided
    /// </summary>
    public class ResourceDisplay : MonoBehaviour {
        
        [Serializable]
        private struct ResourceEntry {
            public string resourceKey;
            public string displayName;
        }

        [SerializeField]
        private TMP_Text resourceText;
        [SerializeField]
        private List<ResourceEntry> displayedResources = new List<ResourceEntry>();
        
        private StringBuilder builder = new StringBuilder();

        private void Start() {
            if (resourceText == null) {
                Debug.LogWarning("resourceText not set on " + gameObject.name);
                return;
            }
            if (ResourceManager.Instance == null) {
                Debug.LogWarning("No ResourceManager found in Scene ");
                return;
            }

            ResourceManager.Instance.OnResourceChanged += HandleResourceChanged;
            Refresh();
        }

        private void HandleResourceChanged(string resource, int newAmount, int change) {
            Refresh();
        }

        private void Refresh() {
            builder.Clear();
            builder.Append("Current Stock");

            if (displayedResources.Count > 0) {
                foreach(ResourceEntry resource in displayedResources) {
                    AddLineWithDisplayName(resource, ResourceManager.Instance.Get(resource.resourceKey));
                }
            } else {
                Debug.LogWarning("Resource Name not Provided on " + gameObject.name);
            }

            resourceText.text = builder.ToString();
        }

        /// <summary>
        /// Custom Formatting for certain reosurce Icons
        /// </summary>
        /// <param name="resource">Resource in system and wanted display name</param>
        /// <param name="amount"> Amount</param>
        private void AddLineWithDisplayName(ResourceEntry resource, int amount) {
            if (builder.Length > 0) builder.Append("\n");
            if (resource.displayName == null || resource.displayName == String.Empty)
            {
                builder.Append(resource.resourceKey + "s: ");
            }
            else
            {
                builder.Append(resource.displayName);
            }
            builder.Append(amount);
        }

        private void OnDestroy() {
            if (ResourceManager.Instance != null) {
                ResourceManager.Instance.OnResourceChanged -= HandleResourceChanged;
            }
        }
    }
}