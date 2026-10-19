using Shared;
using UnityEngine;

namespace Sharyq {
    /// <summary>
    /// Initializes All Needed Resources For Waffle House
    /// Current Resources: Dollars to be Spent and Gained from Customers
    /// Waffle: TO be baked and given to Customer
    /// </summary>
    public class InitializeWaffleHouseResources : MonoBehaviour {
        
        void Start() {
            ResourceManager.Instance.Add("Dollar", 1000);
            ResourceManager.Instance.Add("Batter", 0);
            ResourceManager.Instance.Add("Waffle", 10);
            ResourceManager.Instance.Add("Leftover", 0);
        }
    }
}