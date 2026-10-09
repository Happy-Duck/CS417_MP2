using System.Collections;
using UnityEngine;
using UnityEngine.AI;
namespace Rishi {

	[RequireComponent(typeof(AudioSource))]
	public class ChickenWander : MonoBehaviour {
		[SerializeField] private float wanderRadius = 4f;
		[SerializeField] private Vector2 idleTimeRange = new Vector2(2f, 6f);
		[SerializeField] private float walkTimeout = 10f;
		[SerializeField] private string speedParameter = "Vert";

		AudioSource chickenAudioSource;
		[SerializeField] AudioClip cluck;
		private NavMeshAgent agent;
		private Animator animator;
		private Vector3 home;

		private void Awake() {
			agent = GetComponent<NavMeshAgent>();
			animator = GetComponent<Animator>();
		}

		private void Start() {
			home = transform.position;
			chickenAudioSource = GetComponent<AudioSource>();
			StartCoroutine(WanderLoop());
		}

		private void Update() {
			float speed01 = agent.speed > 0f ? agent.velocity.magnitude / agent.speed : 0f;
			animator.SetFloat(speedParameter, Mathf.Clamp01(speed01));
		}

		private IEnumerator WanderLoop() {
			while (true) {
				chickenAudioSource.PlayOneShot(cluck);
				yield return new WaitForSeconds(Random.Range(idleTimeRange.x, idleTimeRange.y));

				if (!TryPickDestination(out Vector3 destination)) continue;
				agent.SetDestination(destination);

				float timer = 0f;
				while (timer < walkTimeout && (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)) {
					timer += Time.deltaTime;
					yield return null;
				}
				agent.ResetPath();
			}
		}

		// disabling isn't enough, the coroutine keeps running
		public void StopWandering() {
			StopAllCoroutines();
			agent.ResetPath();
		}

		private bool TryPickDestination(out Vector3 destination) {
			Vector2 offset = Random.insideUnitCircle * wanderRadius;
			Vector3 candidate = home + new Vector3(offset.x, 0f, offset.y);
			if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas)) {
				destination = hit.position;
				return true;
			}
			destination = transform.position;
			return false;
		}
	}
}