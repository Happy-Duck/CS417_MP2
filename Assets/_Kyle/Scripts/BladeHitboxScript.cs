using Kyle;
using UnityEngine;

namespace Kyle
{
    public class BladeHitboxScript : MonoBehaviour
    {
        [SerializeField] private HarvestToolScript harvestTool;

        private void OnTriggerEnter(Collider other)
        {
            if (harvestTool != null)
            {
                harvestTool.ProcessBladeHit(other);
            }
        }
    }
}
