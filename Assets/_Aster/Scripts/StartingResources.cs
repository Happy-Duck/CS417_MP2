using Aster;
using Shared;
using System.Collections.Generic;
using UnityEngine;

namespace Aster
{//mostly for testing
    public class StartingResources : MonoBehaviour
    {
        public List<Amount> amounts = new List<Amount>();

        void Start()
        {
            foreach (var a in amounts) ResourceManager.Instance.Add(a.resource, a.count);
        }
    }
}
