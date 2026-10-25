using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Sharyq
{
    /// <summary>
    /// Used to identify Batter Object
    /// </summary>
    public class Batter : MonoBehaviour
    {
        [SerializeField] private GameObject wafflePrefab;
        public WaffleStat CreateWaffle(Transform instanceLocation)
        {
            var waffle = Instantiate(wafflePrefab, instanceLocation.position, instanceLocation.rotation);
            return waffle.GetComponent<WaffleStat>();
        }
    }
}

