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
            ResourceManager.Instance.Add("Dollar", 9999);
            ResourceManager.Instance.Add("Batter", 0);
            ResourceManager.Instance.Add("Waffle", 30);
            ResourceManager.Instance.Add("Apple Waffle", 10);
            ResourceManager.Instance.Add("Banana Waffle", 10);
            ResourceManager.Instance.Add("Cherry Waffle", 10);
            ResourceManager.Instance.Add("Peach Waffle", 10);
            ResourceManager.Instance.Add("Strawberry Waffle", 10);
            ResourceManager.Instance.Add("Chicken Waffle", 10);
            ResourceManager.Instance.Add("Corn Waffle", 10);
            
            ResourceManager.Instance.Add("Leftover", 10);
            ResourceManager.Instance.Add("Burnt Waffle", 10);
            ResourceManager.Instance.Add("Energizing Waffle", 100);
            
            // Egg resources
            ResourceManager.Instance.Add("White Egg", 10);
            ResourceManager.Instance.Add("Brown Egg", 10);
            ResourceManager.Instance.Add("Red Egg", 10);
            ResourceManager.Instance.Add("Blue Egg", 10);

            ResourceManager.Instance.Add("Chicken", 10);
            ResourceManager.Instance.Add("Corn", 10);

            ResourceManager.Instance.Add("Strawberry", 10);
            ResourceManager.Instance.Add("Banana", 10);
            ResourceManager.Instance.Add("Apple", 10);
            ResourceManager.Instance.Add("Cherry", 10);
            ResourceManager.Instance.Add("Peach", 10);
            ResourceManager.Instance.Add("METH", 10);
        }
    }
}