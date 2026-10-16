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

        [SerializeField]
        private TMP_Text resourceText;
        [SerializeField]
        private List<string> displayedResources = new List<string>();
        [SerializeField]
        private List<string> resourceName = new List<string>();

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

            if (displayedResources.Count > 0 && displayedResources.Count == resourceName.Count) {
                for (int i = 0; i < displayedResources.Count; i++) {
                    AddLineWithDisplayName(resourceName[i], ResourceManager.Instance.Get(displayedResources[i]));
                }
            } else {
                Debug.LogWarning("Resource Name not Provided on " + gameObject.name);
            }

            resourceText.text = builder.ToString();
        }
        
        /// <summary>
        /// Custom Formatting for certain reosurce Icons
        /// </summary>
        /// <param name="name">Display Title for it</param>
        /// <param name="amount"> Amount</param>
        private void AddLineWithDisplayName(string name, int amount) {
            if (builder.Length > 0) builder.Append("\n");
            builder.Append(name);
            builder.Append(amount);
        }

        private void OnDestroy() {
            if (ResourceManager.Instance != null) {
                ResourceManager.Instance.OnResourceChanged -= HandleResourceChanged;
            }
        }
    }
}