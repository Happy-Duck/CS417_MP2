using UnityEngine;

namespace Sharyq
{
    public enum IngredientType { Egg, HarvestedWheat, Apple, Banana, Peach, Cherry, Strawberry, Chicken, HarvestedCorn, METH }

    public class Ingredient : MonoBehaviour
    {
        public IngredientType type;
    }
}
