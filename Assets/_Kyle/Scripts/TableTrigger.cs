using UnityEngine;


namespace Kyle
{
    public class TableTrigger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out FarmingToolScript tool))
            {
                tool.SetOverTable(true);
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out FarmingToolScript tool))
            {
                tool.SetOverTable(false);
            }
        }
    }
}
