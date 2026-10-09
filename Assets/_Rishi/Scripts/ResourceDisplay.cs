using Shared;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace Rishi {
	public class ResourceDisplay : MonoBehaviour {

		[SerializeField] TMP_Text resourceText;
		[SerializeField] List<string> onlyShowTheseResources = new List<string>();

		private StringBuilder builder = new StringBuilder();

		void Start() {
			if (resourceText == null) {
				Debug.LogWarning("ResourceDisplay on " + gameObject.name + " has no resourceText");
				return;
			}
			if (ResourceManager.Instance == null) {
				Debug.LogWarning("ResourceDisplay on " + gameObject.name + " can't find a ResourceManager");
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

			if (onlyShowTheseResources.Count > 0) {
				foreach (string resource in onlyShowTheseResources) {
					AddLine(resource, ResourceManager.Instance.Get(resource));
				}
			} else {
				// sorted so the list doesn't jump around
				List<string> names = new List<string>(ResourceManager.Instance.All.Keys);
				names.Sort();
				foreach (string resource in names) {
					AddLine(resource, ResourceManager.Instance.Get(resource));
				}
			}

			resourceText.text = builder.ToString();
		}

		private void AddLine(string resource, int amount) {
			if (builder.Length > 0) builder.Append("\n");
			builder.Append(resource + "s");
			builder.Append(": ");
			builder.Append(amount);
		}

		void OnDestroy() {
			if (ResourceManager.Instance != null) {
				ResourceManager.Instance.OnResourceChanged -= HandleResourceChanged;
			}
		}
	}
}
