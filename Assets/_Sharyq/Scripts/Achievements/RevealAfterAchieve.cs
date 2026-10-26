using System.Collections.Generic;
using Shared;
using UnityEngine;

namespace Sharyq
{
    public class RevealAfterAchieve : MonoBehaviour, IAchievementUnlockable
    {
        [SerializeField]
        private string achievementName;
        [SerializeField]
        private List<GameObject> revealableObjects;

        private void Awake()
        {
            AchievementsManager.Instance.onAchieved.AddListener(Unlock);
            revealableObjects.ForEach(x => x.SetActive(false));
        }

        public void Unlock(string achievement, string _)
        {
            if (achievementName == achievement)
                revealableObjects.ForEach(x => x.SetActive(true));
        }
    }
}

