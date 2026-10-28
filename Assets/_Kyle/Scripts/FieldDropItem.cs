using UnityEngine;
using Shared;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;

namespace Kyle
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(XRSimpleInteractable))]
    public class FieldDropItem : MonoBehaviour
    {
        public string resourceKey;
        [SerializeField] private AudioClip collectSound;

        private Rigidbody rb;
        private XRSimpleInteractable simpleInteractable;
        private bool isCollected = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            simpleInteractable = GetComponent<XRSimpleInteractable>();
        }
        private void OnEnable()
        {
            simpleInteractable.hoverEntered.AddListener(OnHoverEntered);
        }

        private void OnDisable()
        {
            simpleInteractable.hoverEntered.RemoveListener(OnHoverEntered);
        }

        public void Launch()
        {
            float randomX = Random.Range(-0.1f, 0.1f);
            float randomZ = Random.Range(-0.1f, 0.1f);
            float upwardForce = Random.Range(1f, 1.5f);

            rb.linearVelocity = new Vector3(randomX, upwardForce, randomZ);
        }

        //private void OnTriggerEnter(Collider other)
        //{
        //    if (isCollected) return;

        //    if (other.CompareTag("Player") || other.name.Contains("Hand") || other.name.Contains("Controller"))
        //    {
        //        CollectItem();
        //    }
        //}

        private void OnHoverEntered(HoverEnterEventArgs args)
        {
            CollectItem();
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

            //Shared.AchievementsManager.Instance.CompleteAchievement("3rd Degree Herbicide");

            Destroy(gameObject);
        }
    }
}