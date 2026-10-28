using System.Collections.Generic;
using Shared;
using UnityEngine;
using UnityEngine.Events;

namespace Sharyq
{
    public class IsResourcePresent : MonoBehaviour
    {
        public UnityEvent onResourcePresent;
        [SerializeField]
        private List<string> resources;
        private void Awake()
        {
            ResourceManager.Instance.OnResourceChanged += CheckRequirement;
        }

        private void CheckRequirement(string arg1, int arg2, int arg3)
        {
            foreach (var r in resources)
            {
                if (ResourceManager.Instance.Get(r) > 0)
                {
                    ResourceManager.Instance.OnResourceChanged -= CheckRequirement;
                    onResourcePresent.Invoke();
                    return;
                }
            }
        }

        private void OnDestroy()
        {
            if (ResourceManager.Instance != null)
                ResourceManager.Instance.OnResourceChanged -= CheckRequirement;
        }
    } 
}

