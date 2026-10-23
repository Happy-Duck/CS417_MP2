using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Shared
{
    /// <summary>
    /// Global Manager to give Achievements to the Player
    /// and Check if player has a specific Achievement
    /// </summary>
    public class AchievementsManager : MonoBehaviour
    {
        /// <summary>
        /// Contains a name used to Identify the Achievement(Like a Key)
        /// A publicly modifiable Description(presumably for the use of displaying achievements
        /// A list of required achievements(Perhaps to also be used for display, but dev purposes,
        /// ensures that we know an achievement needs to be achieved before usually getting this one
        ///
        /// Most of Achievement Interfacing should be done through the Manager, but if you need to
        /// understand or modify achievements, you can
        /// </summary>
        [Serializable]
        public struct Achievement
        {
            [SerializeField] private string name;
            [SerializeField] private string description;
            [SerializeField] private List<string> requiredAchievementsNames;
            [SerializeField] private bool achieved;

            public string Name => name;
            public string Description => description;
            public IReadOnlyList<string> RequiredAchievementsNames => requiredAchievementsNames;
            public bool Achieved => achieved;

            /// <summary>
            /// Default Constructor, taking in name, description, and a list of required achievement names
            /// _achieved should always be false by default
            /// </summary>
            /// <param name="name"></param>
            /// <param name="description"></param>
            /// <param name="requiredAchievementsNames"></param>
            public Achievement(string name, string description, List<string> requiredAchievementsNames)
            {
                this.name = name;
                this.description = description;
                this.requiredAchievementsNames = requiredAchievementsNames;
                achieved = false;
            }
            
            /// <summary>
            /// Sets a specific achievement to true and checks if all required achievements have been acquired
            /// For Game play tree purposes
            /// </summary>
            public void AchievementAcquired()
            {
                foreach (var requiredAchievement in requiredAchievementsNames)
                {
                    bool successful = Instance.TryGetAchievementByName(requiredAchievement, out Achievement achievement);
                    if (!successful)
                    {
                        Debug.LogError($"Required Achievement {requiredAchievement} in {this.name} not found");
                        continue;
                    }

                    if (!achievement.Achieved)
                    {
                        Debug.LogWarning($"Achievement {requiredAchievement} has not been achieved; Shouldn't be able to Acquire Achievement");
                    }
                }
                //Comment out later
                Debug.Log($"Achievement {name} has been acquired");
                achieved = true;
            }
        }
        
        /// <summary>
        /// In Manager, this is where we will assign achievements
        /// </summary>
        [SerializeField]
        private List<Achievement> achievements;
        
        /// <summary>
        /// Like GridManager, how we can access the Instance from Any script
        /// </summary>
        public static AchievementsManager Instance;
        
        /// <summary>
        /// UnityEvent that can be subscribed that is invoked when
        /// 'CompleteAchievement' is called
        /// </summary>
        public UnityEvent<string, string> onAchieved;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("There are multiple instances of AchievementsManager, Destroying One of Them");
                Destroy(this);
                return;
            }

            Instance = this;
        }
        
        private void OnDestroy() {
            if (Instance == this) Instance = null;
        }
        
        /// <summary>
        /// Prefered way to check if a specific Achievement is gotten
        /// </summary>
        /// <param name="achievementName">Achievement</param>
        /// <returns>if it is found and gotten</returns>
        public bool IsAchieved(string achievementName)
        {
            bool successful = TryGetAchievementByName(achievementName, out Achievement achievement);
            if (!successful)
            {
                Debug.LogWarning("Achievement " + achievementName + " not found");
                return false;
            }
            
            return achievement.Achieved;
        }
        
        /// <summary>
        /// Preferred way to mark an achievement as complete!
        /// Invokes Event when completed
        /// </summary>
        /// <param name="achievementName"></param>
        public void CompleteAchievement(string achievementName)
        {
            bool successful = TryGetAchievementByName(achievementName, out Achievement achievement);
            if (!successful)
            {
                Debug.LogWarning("Achievement " + achievementName + " not found");
                return;
            }
            achievement.AchievementAcquired();
            onAchieved.Invoke(achievement.Name, achievement.Description);
        }
        
        /// <summary>
        /// Gets achievemment if findable. will put it into out param
        /// if achievement cant be found, will return false and empty struct
        /// </summary>
        /// <param name="achievementName"></param>
        /// <param name="achievement">out param</param>
        /// <returns>successfully got</returns>
        public bool TryGetAchievementByName(string achievementName, out Achievement achievement)
        {
            if (achievements.Exists(x => x.Name == achievementName))
            {
                achievement = achievements.Find(x => x.Name == achievementName);
                return true;
            }

            achievement = default;
            return false;
        }
    }
}