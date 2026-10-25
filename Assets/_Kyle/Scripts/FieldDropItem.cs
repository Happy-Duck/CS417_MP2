using UnityEngine;
using Shared;

namespace Kyle
{
    [RequireComponent(typeof(Rigidbody))]
    public class FieldDropItem : MonoBehaviour
    {
        public string resourceKey;
        [SerializeField] private AudioClip collectSound;
        private Rigidbody rb;
        private bool isCollected = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        public void Launch()
        {
            float randomX = Random.Range(-2f, 2f);
            float randomZ = Random.Range(-2f, 2f);
            float upwardForce = Random.Range(4f, 7f);

            rb.linearVelocity = new Vector3(randomX, upwardForce, randomZ);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isCollected) return;

            if (other.CompareTag("Player") || other.name.Contains("Hand") || other.name.Contains("Controller"))
            {
                CollectItem();
            }
        }

        public void CollectItem()
        {
            if (isCollected) return;
            isCollected = true;

            ResourceManager.Instance.Add(resourceKey, 1);
            InventoryManager.Instance?.UpdateNums();

            if (collectSound != null)
            {
                AudioSource.PlayClipAtPoint(collectSound, transform.position, 1.0f);
            }

            Destroy(gameObject);
        }
    }
}