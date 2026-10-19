using UnityEngine;

namespace Sharyq
{
    public enum IngredientType { Egg, Wheat, Apple, Banana, Peach, Cherry, Strawberry, Chicken, Corn, Meth }

    public class Ingredient : MonoBehaviour
    {
        public IngredientType type;
    }
}
