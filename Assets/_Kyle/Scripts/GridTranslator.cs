using UnityEngine;
using Shared;

namespace Kyle
{
    public class GridTranslator : MonoBehaviour
    {
        public static GridTranslator Instance { get; private set; }

        [SerializeField] Transform tableOrigin;
        [SerializeField] float tableCellSize = 0.00625f;

        [SerializeField] Transform fieldOrigin;
        [SerializeField] float fieldCellSize = 0.5f;

        private void Awake()
        {
            if (Instance == null) { Instance = this; }
        }

        public Vector2Int GetCellFromTable(Vector3 hit)
        {
            Vector3 localHit = tableOrigin.InverseTransformPoint(hit);

            int x = Mathf.FloorToInt(localHit.x / tableCellSize);
            int z = Mathf.FloorToInt(localHit.z / tableCellSize);
            return new Vector2Int(x, z);
        }

        public Vector3 GetTablePosition(Vector2Int cell)
        {
            return tableOrigin.position + (tableOrigin.right * (cell.x * tableCellSize)) + (tableOrigin.forward * (cell.y * tableCellSize));
        }

        public Vector3 GetFieldPosition(Vector2Int cell)
        {
            return fieldOrigin.position + (fieldOrigin.right * (cell.x * fieldCellSize)) + (fieldOrigin.forward * (cell.y * fieldCellSize));
        }
    }
}