using UnityEngine;

namespace Alyssa
{
    public class SoilBed : MonoBehaviour
    {
        public Plant plant;   // Unity null check handles a dead (destroyed) plant

        public bool TryPlant(Plant prefab)
        {
            if (plant != null) return false;
            plant = Instantiate(prefab, transform.position + Vector3.up * 0.05f,
                                Quaternion.identity, transform);   // child of the bed
            return true;
        }
    }
}