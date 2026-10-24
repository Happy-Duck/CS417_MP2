using System.Collections;
using Shared;
using UnityEngine;

public class ShowOnFirstWaffleMade : MonoBehaviour
{
    [SerializeField]
    private GameObject tutorialPopup;

    private bool subscribed;

    private IEnumerator Start()
    {
        yield return null;

        if (ResourceManager.Instance == null) yield break;

        ResourceManager.Instance.OnResourceChanged += HandleResourceChanged;
        subscribed = true;
    }

    private void HandleResourceChanged(string resource, int newAmount, int change)
    {
        if (resource != "Waffle" || change <= 0)
            return;

        Unsubscribe();
        tutorialPopup.SetActive(true);
    }

    private void OnDestroy() => Unsubscribe();

    private void Unsubscribe()
    {
        if (!subscribed) return;
        subscribed = false;

        if (ResourceManager.Instance != null)
            ResourceManager.Instance.OnResourceChanged -= HandleResourceChanged;
    }
}