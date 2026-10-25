using UnityEngine;

namespace Kyle
{
    public class FieldCellCollider : MonoBehaviour
    {
        public Vector2Int CellCoordinates { get; private set; }

        public void Initialize(Vector2Int coords)
        {
            CellCoordinates = coords;
        }
    }
}
