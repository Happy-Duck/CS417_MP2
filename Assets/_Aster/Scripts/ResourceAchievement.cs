using System.Collections.Generic;
using UnityEngine;
using Shared;

namespace Aster
{
    public class ResourceAchievement : MonoBehaviour
    {
        [SerializeField]
        private string[] required;

        private HashSet<string> obtained = new HashSet<string>();

        public void ObtainedResource(string resourceName)
        {
            if (string.IsNullOrEmpty(resourceName))
                return;

            obtained.Add(resourceName);

            CheckAllObtained();
        }

        private void CheckAllObtained()
        {
            foreach (string resource in required)
            {
                if (!obtained.Contains(resource))
                    return;
            }

            AchievementsManager.Instance.CompleteAchievement(
                "Hoarder"
            );
        }
    }
}