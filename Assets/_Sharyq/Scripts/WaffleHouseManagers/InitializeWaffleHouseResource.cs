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
            ResourceManager.Instance.Add("Waffle", 30);
            ResourceManager.Instance.Add("Apple Waffle", 0);
            ResourceManager.Instance.Add("Banana Waffle", 0);
            ResourceManager.Instance.Add("Cherry Waffle", 0);
            ResourceManager.Instance.Add("Peach Waffle", 0);
            ResourceManager.Instance.Add("Strawberry Waffle", 0);
            ResourceManager.Instance.Add("Chicken Waffle", 0);
            ResourceManager.Instance.Add("Corn Waffle", 0);
            
            ResourceManager.Instance.Add("Leftover", 0);
            ResourceManager.Instance.Add("Burnt Waffle", 0);
            ResourceManager.Instance.Add("Energizing Waffle", 0);
            
            // Egg resources
            ResourceManager.Instance.Add("White Egg", 0);
            ResourceManager.Instance.Add("Brown Egg", 0);
            ResourceManager.Instance.Add("Red Egg", 0);
            ResourceManager.Instance.Add("Blue Egg", 0);
        }
    }
}