using Aster;
using Shared;
using UnityEngine;

namespace Aster
{
   
    public class Salt : MonoBehaviour
    {
        public int health = 5;             // stat health
        public int maxHealth = 5;
        public Harvestable chunkPrefab;    // must have resourceName = Salt
        public Renderer rend;
        public Color damagedColor = new Color(0.45f, 0.35f, 0.35f);
        public ParticleSystem hitFx;
        public StatBar bar;
        public float hitCooldown = 0.4f;

        float lastHit = -10f, punch;
        Vector3 baseScale;

        void Start() { baseScale = transform.localScale; UpdateVisual(); }

        void Update()
        {
            punch = Mathf.MoveTowards(punch, 0f, 2f * Time.deltaTime);
            transform.localScale = baseScale * (1f + punch);
        }

        public bool TakeHit()
        {
            if (health <= 0 || Time.time - lastHit < hitCooldown) return false;
            lastHit = Time.time;
            health--;
            punch = 0.12f;

            var c = Instantiate(chunkPrefab,
                transform.position + Vector3.up * 0.4f + Random.insideUnitSphere * 0.15f,
                Random.rotation, transform.root);
            var rb = c.GetComponent<Rigidbody>();
            if (rb != null) rb.AddForce(new Vector3(Random.Range(-1f, 1f), 1.5f, Random.Range(-1f, 1f)), ForceMode.Impulse);

            if (hitFx != null) hitFx.Play();
            UpdateVisual();
            if (health <= 0) {
                AchievementsManager.Instance.CompleteAchievement("This is just like Minecraft...");
                Destroy(gameObject);
            }
            return true;
        }

        void UpdateVisual()
        {
            if (rend != null) rend.material.color = Color.Lerp(damagedColor, Color.white, (float)health / maxHealth);
            if (bar != null) bar.Set(health, maxHealth);
        }
    }
}
