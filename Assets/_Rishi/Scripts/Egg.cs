namespace Sharyq
{
    public enum EggType { White, Brown, Red, Blue }
    public class Egg : Ingredient
    {
        public EggType EggType;

        public void Awake()
        {
            type = IngredientType.Egg;
        }
    }
}