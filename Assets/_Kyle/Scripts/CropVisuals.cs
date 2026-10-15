using Shared;
using UnityEngine;

namespace Kyle
{
    public class CropVisuals : MonoBehaviour
    {
        [SerializeField] Transform miniatureModel;
        [SerializeField] Transform giantModel;

        private GridOccupant occupantData;

        private void Awake()
        {
            occupantData = GetComponent<GridOccupant>();
        }

        public bool Plant(Vector2Int targetCell)
        {
            Vector3 gridPosition = GridManager.Instance.CellToWorld(targetCell, 0);

            if (!occupantData.TryPlaceAt(gridPosition))
            {
                return false;
            }
            miniatureModel.position = GridTranslator.Instance.GetTablePosition(targetCell);
            giantModel.position = GridTranslator.Instance.GetFieldPosition(targetCell);

            return true;
        }
    }
}
