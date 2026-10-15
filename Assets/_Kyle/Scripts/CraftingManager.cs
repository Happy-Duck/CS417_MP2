using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using Shared;

namespace Kyle
{
    public class CraftingManager : MonoBehaviour
    {
        [Header("Socket References")]
        [SerializeField] private XRSocketInteractor[] craftingSockets;

        public void TryCraft()
        {
            int cornCount = 0;
            int wheatCount = 0;
            int weedCount = 0;

            foreach (var socket in craftingSockets)
            {
                if (socket.interactablesSelected.Count > 0)
                {
                    InventoryItem tracker = socket.interactablesSelected[0].transform.GetComponent<InventoryItem>();
                    if (tracker == null) continue;

                    if (tracker.ResourceName == "Corn") cornCount++;
                    else if (tracker.ResourceName == "Wheat") wheatCount++;
                    else if (tracker.ResourceName == "Weeds") weedCount++;
                }
            }

            if (cornCount >= 2 && wheatCount == 0 && weedCount == 0)
            {
                ConsumeItems(2, "Corn");
                ResourceManager.Instance.Add("CornSeed", 3);
                InventoryManager.Instance.UpdateNums();
                return;
            }

            if (wheatCount >= 2 && cornCount == 0 && weedCount == 0)
            {
                ConsumeItems(2, "Wheat");
                ResourceManager.Instance.Add("WheatSeed", 3);
                InventoryManager.Instance.UpdateNums();
                return;
            }

            if (weedCount >= 4 && cornCount == 0 && wheatCount == 0)
            {
                ConsumeItems(4, "Weeds");
                ResourceManager.Instance.Add("Fertilizer", 4);
                InventoryManager.Instance.UpdateNums();
                return;
            }
        }

        private void ConsumeItems(int amountNeeded, string targetResourceName)
        {
            int destroyed = 0;
            foreach (var socket in craftingSockets)
            {
                if (socket.interactablesSelected.Count > 0 && destroyed < amountNeeded)
                {
                    GameObject item = socket.interactablesSelected[0].transform.gameObject;
                    InventoryItem tracker = item.GetComponent<InventoryItem>();

                    if (tracker != null && tracker.ResourceName == targetResourceName)
                    {
                        tracker.MarkAsCrafted();

                        socket.enabled = false;
                        socket.enabled = true;
                        Destroy(item);
                        destroyed++;
                    }
                }
            }
        }
    }
}