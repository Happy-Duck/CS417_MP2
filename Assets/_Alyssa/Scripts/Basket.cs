using UnityEngine;
using Shared;

namespace Alyssa
{
    public class Basket : MonoBehaviour
    {
        void OnTriggerEnter(Collider other)
        {
            var fruit = other.GetComponentInParent<Fruit>();
            if (fruit == null || ResourceManager.Instance == null) return;
            ResourceManager.Instance.Add(fruit.resourceName, 1);
            GameEvents.RaiseHarvested(fruit.resourceName);
            Destroy(fruit.gameObject);
        }
    }
}