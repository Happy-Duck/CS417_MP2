using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Alyssa
{
    public enum Goal { Variety, Expansion, Volume, FertilizerPro, HarvesterPro }

    // Mastery Thresholding + Unlock Reveal + Thresholds on the Horizon
    public class Milestones : MonoBehaviour
    {
        [System.Serializable]
        public class Unlock
        {
            public Goal goal;
            public GameObject[] hidden;      // stay inactive until this goal is cleared
        }

        public Unlock[] unlocks;
        public GameObject boardRoot;         // the whole achievement board (frame + canvas), hidden until the first goal is cleared
        public TMPro.TMP_Text boardText;     // the TextMeshPro text on it
        public string[] crops = { "Strawberry", "Peach", "Cherry", "Apple", "Banana" };
        public int bedsNeeded = 4;           // Expansion: this many planted beds...
        public float cleanSeconds = 20f;     // ...with no weeds anywhere, held this long
        public float revealSeconds = 6f;

        // [0] name  [1] goal  [2] what it unlocks  [3] reveal card text
        static readonly Dictionary<Goal, string[]> info = new Dictionary<Goal, string[]>
        {
            { Goal.Variety, new[] { "Variety", "Harvest all 5 crops", "Sprinklers",
                "Buy sprinklers, and put them on a soilbed to automatically water your plants! Note that this still uses your water reserve." } },
            { Goal.Expansion, new[] { "Expansion", "Keep 4 beds planted with no weeds", "Auto-weeding",
                "Buy this autoweeder, and put it on a soilbed to automatically remove weeds from it." } },
            { Goal.Volume, new[] { "Volume", "Harvest 5 of one crop", "Fertilizer",
                "Buy/Make fertilizer, and put it on a soilbed with a plant to make it grow twice as fast! (Only works for one growth cycle)" } },
            { Goal.FertilizerPro, new[] { "Fertilizer Pro", "Fertilize 5 crops", "Auto-fertilizer",
                "Put this on a bed to automatically fertilize the plant (if fertilizer is available in resources)." } },
            { Goal.HarvesterPro, new[] { "Harvester Pro", "Harvest 20 fruits", "Auto-harvester",
                "Put this on a soilbed to automatically harvest the plant when fruit grows!" } },
        };

        static readonly Goal[] order = { Goal.Variety, Goal.Expansion, Goal.Volume, Goal.FertilizerPro, Goal.HarvesterPro };

        readonly Dictionary<string, int> harvested = new Dictionary<string, int>();
        readonly HashSet<Goal> cleared = new HashSet<Goal>();
        int totalHarvested, fertilized;
        float cleanTime, nextBedCheck;

        void Awake()
        {
            if (unlocks != null)
                foreach (var u in unlocks)
                    foreach (var go in u.hidden)
                        if (go != null) go.SetActive(false);
            if (boardRoot != null) boardRoot.SetActive(false);
        }

        void OnEnable() { GameEvents.Harvested += OnHarvested; GameEvents.Fertilized += OnFertilized; }
        void OnDisable() { GameEvents.Harvested -= OnHarvested; GameEvents.Fertilized -= OnFertilized; }

        void OnHarvested(string res)
        {
            if (System.Array.IndexOf(crops, res) < 0) return;   // weeds give Compost, which doesn't count
            harvested[res] = Count(res) + 1;
            totalHarvested++;
            Check();
        }

        void OnFertilized() { fertilized++; Check(); }

        int Count(string crop) { int n; return harvested.TryGetValue(crop, out n) ? n : 0; }

        void Update()
        {
            if (Time.time < nextBedCheck) return;
            nextBedCheck = Time.time + 0.5f;

            int planted = 0;
            bool weeds = false;
            foreach (var b in FindObjectsByType<SoilBed>())
            {
                if (!b.Placed) continue;                  // the bed still sitting on the crate isn't part of the garden
                if (b.plant != null) planted++;
                if (b.HasWeed) weeds = true;
            }
            cleanTime = (planted >= bedsNeeded && !weeds) ? cleanTime + 0.5f : 0f;
            Check();
        }

        bool Met(Goal g)
        {
            switch (g)
            {
                case Goal.Variety:
                    foreach (var c in crops) if (Count(c) == 0) return false;
                    return true;
                case Goal.Expansion:
                    return cleanTime >= cleanSeconds;
                case Goal.Volume:
                    foreach (var c in crops) if (Count(c) >= 5) return true;
                    return false;
                case Goal.FertilizerPro:
                    return fertilized >= 5;
                default:
                    return totalHarvested >= 20;
            }
        }

        void Check()
        {
            foreach (var g in order)
                if (!cleared.Contains(g) && Met(g))
                {
                    cleared.Add(g);
                    StartCoroutine(Reveal(g));
                }
        }

        IEnumerator Reveal(Goal g)
        {
            // Unlock Reveal: explain the new mechanic first...
            Feedback.Card(info[g][2] + " unlocked!\n" + info[g][3], revealSeconds);
            yield return new WaitForSeconds(revealSeconds * 0.6f);
            // ...then the new objects appear
            if (unlocks != null)
                foreach (var u in unlocks)
                    if (u.goal == g)
                        foreach (var go in u.hidden)
                            if (go != null) go.SetActive(true);
            UpdateBoard();
        }

        // Thresholds on the Horizon: only visible once a goal has been cleared
        void UpdateBoard()
        {
            if (boardRoot == null || boardText == null || cleared.Count == 0) return;
            boardRoot.SetActive(true);
            foreach (var g in order)
                if (!cleared.Contains(g))
                {
                    boardText.text = "NEXT GOAL: " + info[g][0] + "\n" + info[g][1] + "\nUnlocks: " + info[g][2];
                    return;
                }
            boardText.text = "All goals cleared!";
        }
    }
}