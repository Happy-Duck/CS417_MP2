using System.Collections.Generic;
using Shared;
using UnityEngine;

namespace Sharyq
{
    public class UnlockArea : MonoBehaviour
    {
        [SerializeField]
        private List<string> neededAreas;
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
            if (neededAreas.Contains(areaName))
            {
                neededAreas.Remove(areaName);
            }

            if (neededAreas.Count == 0)
            {
                enableItems.ForEach(item => item.SetActive(true));
                disableItems.ForEach(item => item.SetActive(false));
            }
        }
    }
}
