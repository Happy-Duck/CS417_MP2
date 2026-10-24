using UnityEngine;

namespace Sharyq
{
    public class DestroyOnQueue : MonoBehaviour
    {
        public void DestroyObject()
        {
            Destroy(gameObject);
        }
    }
}

