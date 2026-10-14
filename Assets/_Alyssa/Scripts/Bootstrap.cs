using UnityEngine;
using Shared;

namespace Alyssa
{
    public class Bootstrap : MonoBehaviour
    {
        void Start()
        {   // Start, not Awake, so ResourceManager is ready
            ResourceManager.Instance.Add("Compost", 10);
        }
    }
}