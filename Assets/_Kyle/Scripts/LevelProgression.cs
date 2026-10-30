using UnityEngine;
using Shared;

namespace Kyle
{
    public class LevelProgression : MonoBehaviour
    {
        [SerializeField] GameObject gate;
        private bool levelCompleted = false;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            // change later to !levelCompleted && ResourceManager.Instance.Get("PlaneUpgrade") >= 1 && ResourceManager.Instance.Get("TractorUpgrade") >= 1
            // if we fix plane
            if (ResourceManager.Instance.Get("TractorUpgrade") >= 1)
            {
                levelCompleted = true;
                ProgressionManager.CompleteArea("Champaign Cornfield");

                gate.SetActive(false);
                AchievementsManager.Instance.CompleteAchievement("Farming Tycoon");
            }
        }
    }

}