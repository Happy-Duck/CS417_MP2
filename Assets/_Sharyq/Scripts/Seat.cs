using UnityEngine;

namespace Sharyq
{
    public class Seat : MonoBehaviour
    {
        [SerializeField]
        private BoxCollider tableArea;
 
        public bool Occupied { get; private set; }
 
        public bool TryReserve()
        {
            if (Occupied) return false;
            Occupied = true;
            return true;
        }
 
        public void Release()
        {
            Occupied = false;
        }

        public Vector3 GetRandomTablePoint()
        {
            Vector3 size = tableArea.size;
            Vector3 local = tableArea.center + new Vector3(
                Random.Range(-size.x / 2f, size.x / 2f),
                size.y / 2f,
                Random.Range(-size.z / 2f, size.z / 2f));

            return tableArea.transform.TransformPoint(local);
        }
    }

}