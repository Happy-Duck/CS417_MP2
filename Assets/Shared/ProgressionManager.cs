using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Shared {

	// When your area is finished call ProgressionManager.CompleteArea("AREA NAME");
	//		
	// To respond to someone else's area finishing:
	//		put this in Start(): ProgressionManager.AreaCompleted.AddListener(HandleAreaDone);
	//		and this in OnDestroy(): ProgressionManager.AreaCompleted.RemoveListener(HandleAreaDone);
	//		void HandleAreaDone(string areaName) { 
	// 			if (areaName == "AREA NAME") { //do something }
	// 		}
	//
	// To check whenever you want:
	//		if (ProgressionManager.IsAreaComplete("AREA NAME")) { //do something }
	public static class ProgressionManager {
		public static UnityEvent<string> AreaCompleted = new UnityEvent<string>();
		private static readonly HashSet<string> completed = new HashSet<string>();

		// every area that's been completed so far
		public static IReadOnlyCollection<string> CompletedAreas {
			get { return completed; }
		}

		// only fires the event the first time an area is completed
		public static void CompleteArea(string areaName) {
			if (string.IsNullOrEmpty(areaName)) return;
			if (!completed.Add(areaName)) return;

			Debug.Log("Finished " + areaName);
			AreaCompleted.Invoke(areaName);
		}

		public static bool IsAreaComplete(string areaName) {
			return completed.Contains(areaName);
		}
	}
}
