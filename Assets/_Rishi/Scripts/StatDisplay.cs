using System.Collections.Generic;
using Shared;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Rishi {
	[RequireComponent(typeof(Stats))]
	public class StatDisplay : MonoBehaviour {

		[SerializeField] GameObject barPrefab;
		[SerializeField] List<string> onlyShowTheseStats = new List<string>();
		[SerializeField] float heightAboveObject = 0.2f;
		[SerializeField] float canvasUnitSize = 0.005f;
		// 0 = always show
		[SerializeField] float showDistance = 0f;
		[SerializeField] bool hideWhileHeld = false;

		private class StatBar {
			public Stat stat;
			public Slider slider;
		}

		private Stats stats;
		private GridOccupant occupant;
		private XRGrabInteractable grab;
		private GameObject canvasObject;
		private List<StatBar> bars = new List<StatBar>();
		private Transform cameraTransform;
		private float panelHeight;
		private bool isShopStock = false;

		public void SetShopStock(bool value) {
			isShopStock = value;
		}

		void Start() {
			stats = GetComponent<Stats>();
			occupant = GetComponent<GridOccupant>();
			grab = GetComponent<XRGrabInteractable>();
			if (barPrefab == null) {
				Debug.LogWarning("StatDisplay on " + gameObject.name + " has no barPrefab");
				return;
			}
			BuildCanvas();
			BuildBars();
		}

		private void BuildCanvas() {
			// unparented so rolling eggs don't roll the bars
			canvasObject = new GameObject("StatCanvas");

			Canvas canvas = canvasObject.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.WorldSpace;

			RectTransform rect = canvasObject.GetComponent<RectTransform>();
			rect.pivot = new Vector2(0.5f, 0f);

			canvasObject.transform.localScale = new Vector3(canvasUnitSize, canvasUnitSize, canvasUnitSize);

			VerticalLayoutGroup layout = canvasObject.AddComponent<VerticalLayoutGroup>();
			layout.childAlignment = TextAnchor.MiddleCenter;
			layout.childControlWidth = true;
			layout.childControlHeight = true;
			layout.childForceExpandWidth = false;
			layout.childForceExpandHeight = false;
			layout.spacing = 4f;

			ContentSizeFitter fitter = canvasObject.AddComponent<ContentSizeFitter>();
			fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
			fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

			panelHeight = GetPanelHeight();
			canvasObject.transform.position = transform.position + Vector3.up * panelHeight;
		}

		private float GetPanelHeight() {
			Renderer[] renderers = GetComponentsInChildren<Renderer>();
			if (renderers.Length == 0) return heightAboveObject;
			Bounds bounds = renderers[0].bounds;
			foreach (Renderer r in renderers) {
				bounds.Encapsulate(r.bounds);
			}
			return bounds.max.y - transform.position.y + heightAboveObject;
		}

		private void BuildBars() {
			foreach (Stat stat in stats.statList) {
				if (!ShouldShow(stat.statName)) continue;

				GameObject barObject = Instantiate(barPrefab, canvasObject.transform);
				barObject.transform.localScale = Vector3.one;
				barObject.transform.localRotation = Quaternion.identity;

				TMP_Text label = barObject.GetComponentInChildren<TMP_Text>();
				Slider slider = barObject.GetComponentInChildren<Slider>();
				if (label == null || slider == null) {
					Debug.LogWarning("barPrefab needs a TMP_Text and a Slider as children");
					Destroy(barObject);
					continue;
				}

				label.text = stat.statName;
				slider.minValue = 0f;
				slider.maxValue = 1f;
				slider.value = stat.GetNormalized();

				StatBar bar = new StatBar();
				bar.stat = stat;
				bar.slider = slider;
				bars.Add(bar);
			}
		}

		private bool ShouldShow(string statName) {
			if (onlyShowTheseStats.Count == 0) return true;
			return onlyShowTheseStats.Contains(statName);
		}

		void Update() {
			foreach (StatBar bar in bars) {
				bar.slider.value = bar.stat.GetNormalized();
			}
		}

		void LateUpdate() {
			if (canvasObject == null) return;

			if (cameraTransform == null) {
				if (Camera.main != null) cameraTransform = Camera.main.transform;
			}
			if (cameraTransform == null) return;

			canvasObject.transform.position = transform.position + Vector3.up * panelHeight;

			bool visible = true;
			if (isShopStock) visible = false;
			if (hideWhileHeld && grab != null && grab.isSelected) visible = false;
			if (occupant != null && !occupant.IsPlaced) visible = false;

			if (visible && showDistance > 0f) {
				float distance = Vector3.Distance(cameraTransform.position, canvasObject.transform.position);
				if (distance > showDistance) visible = false;
			}

			if (canvasObject.activeSelf != visible) canvasObject.SetActive(visible);
			if (!visible) return;

			// yaw only billboard
			Vector3 toPanel = canvasObject.transform.position - cameraTransform.position;
			toPanel.y = 0f;
			if (toPanel.sqrMagnitude > 0.0001f) {
				canvasObject.transform.rotation = Quaternion.LookRotation(toPanel);
			}
		}

		void OnDestroy() {
			if (canvasObject != null) Destroy(canvasObject);
		}
	}
}
