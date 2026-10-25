using System;
using UnityEngine;

namespace Sharyq
{
    public class DetectInArea : MonoBehaviour
    {
        private bool inArea = false;

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<Batter>(out Batter batter))
                inArea = true;
        }

        private void OnTriggerExit(Collider other)
        {
            inArea = false;
        }

        public bool IsInArea()
        {
            return inArea;
        }
    }
}

