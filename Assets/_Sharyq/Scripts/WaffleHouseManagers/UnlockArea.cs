using System.Collections.Generic;
using Shared;
using UnityEngine;

namespace Sharyq
{
    public class UnlockArea : MonoBehaviour
    {
        [SerializeField]
        private string unlockAreaName;
        [SerializeField]
        private List<GameObject> enableItems;
        [SerializeField]
        private List<GameObject> disableItems;

        private void Start()
        {
            ProgressionManager.AreaCompleted.AddListener(Trigger);
        }

        public void Trigger(string areaName)
        {
            if (areaName != unlockAreaName)
                return;
            enableItems.ForEach(item => item.SetActive(true));
            disableItems.ForEach(item => item.SetActive(false));
        }
    }
}
