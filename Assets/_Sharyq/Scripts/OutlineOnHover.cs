using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRBaseInteractable))]
public class OutlineOnHover : MonoBehaviour
{
    [SerializeField]
    private Material outlineMaterial;

    private XRBaseInteractable _interactable;
    private readonly Dictionary<Renderer, Material[]> _originalMaterials = new();

    private void Awake()
    {
        _interactable = GetComponent<XRBaseInteractable>();

        if (outlineMaterial == null)
            outlineMaterial = Resources.Load<Material>("_Sharyq/Materials/Outline");

        if (outlineMaterial == null)
            Debug.LogError($"{name}: outline material not assigned and not found in Resources.", this);
    }

    private void OnEnable()
    {
        _interactable.hoverEntered.AddListener(OnHoverEntered);
        _interactable.hoverExited.AddListener(OnHoverExited);
    }

    private void OnDisable()
    {
        _interactable.hoverEntered.RemoveListener(OnHoverEntered);
        _interactable.hoverExited.RemoveListener(OnHoverExited);
        RemoveOutline();
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        if (args.interactorObject is not NearFarInteractor)
            return;
        
        if (_originalMaterials.Count == 0)
            AddOutline();
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        if (args.interactorObject is not NearFarInteractor)
            return;
        
        if (!_interactable.isHovered)
            RemoveOutline();
    }

    private void AddOutline()
    {
        if (outlineMaterial == null) return;

        foreach (var rend in GetComponentsInChildren<MeshRenderer>())
        {
            var original = rend.sharedMaterials;
            _originalMaterials[rend] = original;

            var withOutline = new Material[original.Length + 1];
            original.CopyTo(withOutline, 0);
            withOutline[original.Length] = outlineMaterial;

            rend.sharedMaterials = withOutline;
        }
    }

    private void RemoveOutline()
    {
        foreach (var pair in _originalMaterials)
        {
            if (pair.Key != null)
                pair.Key.sharedMaterials = pair.Value;
        }
        _originalMaterials.Clear();
    }
}