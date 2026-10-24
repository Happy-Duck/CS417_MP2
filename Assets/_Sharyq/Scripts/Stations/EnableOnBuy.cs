using System.Collections.Generic;
using Shared;
using UnityEngine;

namespace Sharyq
{
    public class EnableOnBuy : MonoBehaviour
    {
        [SerializeField]
        private int cost = 100;
        [SerializeField]
        private List<GameObject> enableItems;
        [SerializeField]
        private List<GameObject> disableItems;

        public void Trigger()
        {
            if (!ResourceManager.Instance.TrySpend("Dollar", cost)) return;
            enableItems.ForEach(item => item.SetActive(true));
            disableItems.ForEach(item => item.SetActive(false));
        }
    }
}

