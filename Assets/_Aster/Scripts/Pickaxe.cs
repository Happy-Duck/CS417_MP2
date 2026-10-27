using Aster;
using Shared;
using UnityEngine;

namespace Aster
{
    public class Pickaxe : MonoBehaviour
    {
        public float minSpeed = 1.5f;
        public int durability = 15;        // stat durability
        public int maxDurability = 15;
        public StatBar bar;                // child bar above the handle
        public float killY = -5f;          // if it falls out of the world spawn new one T.T

        Vector3 last, vel;

        void Start()
        {
            last = transform.position;
            durability = maxDurability;
            Refresh();
        }

        void FixedUpdate()
        {
            vel = (transform.position - last) / Time.fixedDeltaTime;
            last = transform.position;
        }

        void Update()
        {
            if (transform.position.y < killY) Destroy(gameObject);
        }

        void OnTriggerEnter(Collider other)
        {
            if (vel.magnitude < minSpeed) return;
            var d = other.GetComponentInParent<Salt>();
            if (d != null && d.TakeHit())
            {
                durability--;
                Refresh();
                if (durability <= 0) {
                    AchievementsManager.Instance.CompleteAchievement("No Unbreaking");
                    Destroy(gameObject);
                }
            }
        }

        void Refresh()
        {
            if (bar != null) bar.Set(durability, maxDurability);
        }
    }
}
