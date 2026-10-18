using UnityEngine;

namespace Sharyq
{
    public class Seat : MonoBehaviour
    {
        public Transform leftOverDropPoint;
        public Transform tipDropPoint;
 
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
    }

}