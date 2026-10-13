using UnityEngine;

namespace Alyssa
{
    public class SoilBed : MonoBehaviour
    {
        public Plant plant;
        public Vector3 plantOffset = new Vector3(0, 0.25f, 0);   // soil surface height

        public bool TryPlant(Plant prefab)
        {
            if (plant != null) return false;
            plant = Instantiate(prefab, transform.position + plantOffset,
                                Quaternion.identity, transform);
            return true;
        }
    }
}