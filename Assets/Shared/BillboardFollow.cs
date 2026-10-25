using UnityEngine;

namespace Shared
{
    public class BillboardFollow : MonoBehaviour
    {
        [SerializeField]
        private Transform target;
        [SerializeField]
        private Vector3 offset = new Vector3(2f, 0, 0);
        [SerializeField]
        private float smoothSpeed = 10f;

        private Transform _mainCameraTransform;

        void Start()
        {
            if (Camera.main != null)
            {
                _mainCameraTransform = Camera.main.transform;
            }
        }

        void LateUpdate()
        {
            if (target == null || _mainCameraTransform == null) return;
            
            Vector3 targetPosition = target.TransformPoint(offset);
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
            
            transform.rotation = Quaternion.LookRotation(transform.position - _mainCameraTransform.position);
        }
    }
}
