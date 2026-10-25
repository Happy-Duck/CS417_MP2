using UnityEngine;

namespace Rishi {
	// all my popups go through here so they last the same amount of time
	// (uses Alyssa's Feedback for the actual text)
	public static class Popups {

		public const float hintSeconds = 6f;
		public const float firstTimeSeconds = 8f;

		private static float cardBusyUntil = 0f;

		public static bool CardBusy {
			get { return Time.time < cardBusyUntil; }
		}

		// cooldown stops the same message from stacking up, defaults to how long it stays up
		public static void Hint(string key, string message, Vector3 position, float cooldown = hintSeconds) {
			Alyssa.Feedback.Hint(key, message, position, hintSeconds, cooldown);
		}

		// only ever shows once per play
		public static void FirstTime(string key, string message, Vector3 position) {
			Alyssa.Feedback.Hint(key, message, position, firstTimeSeconds, Mathf.Infinity);
		}

		public static void Card(string message, float seconds) {
			Alyssa.Feedback.Card(message, seconds);
			cardBusyUntil = Time.time + seconds;
		}

		// pulls a spot toward the player so the text doesnt end up inside a UI panel
		public static Vector3 TowardPlayer(Vector3 position, float distance) {
			if (Camera.main == null) return position;
			Vector3 toPlayer = Camera.main.transform.position - position;
			return position + toPlayer.normalized * distance;
		}

		// right in front of the player, for buttons that do stuff somewhere else
		public static Vector3 NearPlayer() {
			if (Camera.main == null) return Vector3.zero;
			Transform cam = Camera.main.transform;
			// Feedback.Hint already adds 0.4 up
			return cam.position + cam.forward * 1.2f - Vector3.up * 0.5f;
		}
	}
}
