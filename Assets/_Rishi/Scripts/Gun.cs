using Shared;
using UnityEngine;

namespace Rishi {
	public class Gun : MonoBehaviour {

		[SerializeField] GameObject bullet;
		[SerializeField] Transform bulletSpawn;
		[SerializeField] float forceMagnitude;

		[SerializeField] AudioSource gunAudio;
		[SerializeField] AudioClip shootSound;

		BoxCollider hitBox;

		public void Awake() {
			hitBox = GetComponent<BoxCollider>();
		}

		public void shoot() {
			if (ResourceManager.Instance.TrySpend("Bullet", 1)) {
				GameObject bulletInstance = Instantiate(bullet, bulletSpawn.position, bulletSpawn.rotation);
				gunAudio.PlayOneShot(shootSound);
				bulletInstance.GetComponent<Rigidbody>().AddForce(transform.forward * forceMagnitude);
				Destroy(bulletInstance, 10f);
			}
		}
	}
}