using System.Text;
using TMPro;
using UnityEngine;

namespace Sharyq
{
    public class BuildingBoard : MonoBehaviour
    {
        [SerializeField] private RentStat rentManager;
        [SerializeField] private TMP_Text displayText;
        
        private StringBuilder builder = new StringBuilder();
        
        private void Start() {
            if (rentManager == null) {
                Debug.LogWarning("rentManager not set on " + gameObject.name);
                return;
            }
            if (displayText == null) {
                Debug.LogWarning("displayText not set on " + gameObject.name);
                return;
            }
        }

        private void FixedUpdate()
        {
            Refresh();
        }

        private void Refresh() {
            builder.Clear();
            builder.Append("Building Stats\n");
            builder.Append("Rent Due in: ");
            builder.Append((int)Mathf.Ceil(rentManager.TimeRemaining));
            builder.Append("\n");
            builder.Append("Rent Amount: $");
            builder.Append(rentManager.RentAmount);
            displayText.text = builder.ToString();
        }
    }
}

