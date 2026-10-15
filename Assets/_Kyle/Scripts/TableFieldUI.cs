using UnityEngine;
using TMPro;

namespace Kyle
{
    public class HoverUIPanel : MonoBehaviour
    {
        [Header("Status")]
        [SerializeField] GameObject status;
        [SerializeField] TextMeshProUGUI statusText;

        [Header("Crop Type")]
        [SerializeField] GameObject crop;
        [SerializeField] TextMeshProUGUI cropText;

        [Header("Fertilizer")]
        [SerializeField] GameObject fertilizer;
        [SerializeField] TextMeshProUGUI fertilizerLevel;
        [SerializeField] GameObject fertilizerMax;

        [Header("Water")]
        [SerializeField] GameObject water;
        [SerializeField] TextMeshProUGUI waterLevel;
        [SerializeField] GameObject waterMax;

        [Header("Growth")]
        [SerializeField] GameObject growth;
        [SerializeField] TextMeshProUGUI growthLevel;
        [SerializeField] GameObject growthMax;

        private void Update()
        {
            Vector2Int targetCell = HighlightSync.Instance.GetCellForUI();

            if (targetCell.x < 0)
            {
                ShowUI(false, false, false, false, false);
                return;
            }

            CropData cropData = FieldActionManager.Instance.GetCropData(targetCell);

            switch (cropData.state)
            {
                case CellState.Empty:
                    ShowUI(true, true, true, false, false);
                    statusText.text = "EMPTY";
                    cropText.text = "NONE";
                    fertilizerLevel.text = Mathf.RoundToInt(cropData.fertilization).ToString();
                    break;

                case CellState.Weedy:
                    ShowUI(true, false, false, false, false);
                    statusText.text = "WEEDY";
                    break;

                case CellState.Fertilized:
                    ShowUI(true, true, true, false, false);
                    statusText.text = "FERTILIZED";
                    cropText.text = "NONE";
                    fertilizerLevel.text = Mathf.RoundToInt(cropData.fertilization).ToString();
                    break;

                case CellState.CornPlanted:
                    ShowUI(true, true, true, true, true);
                    statusText.text = "GROWING";
                    cropText.text = "CORN";

                    fertilizerLevel.text = Mathf.RoundToInt(cropData.fertilization).ToString();
                    waterLevel.text = Mathf.RoundToInt(cropData.water).ToString();
                    growthLevel.text = Mathf.RoundToInt(cropData.growth).ToString();
                    break;

                case CellState.WheatPlanted:
                    ShowUI(true, true, true, true, true);
                    statusText.text = "GROWING";
                    cropText.text = "WHEAT";

                    fertilizerLevel.text = Mathf.RoundToInt(cropData.fertilization).ToString();
                    waterLevel.text = Mathf.RoundToInt(cropData.water).ToString();
                    growthLevel.text = Mathf.RoundToInt(cropData.growth).ToString();
                    break;

                case CellState.CornReadyForHarvest:
                    ShowUI(true, true, true, true, false);
                    statusText.text = "READY";
                    cropText.text = "CORN";
                    fertilizerLevel.text = Mathf.RoundToInt(cropData.fertilization).ToString();
                    waterLevel.text = Mathf.RoundToInt(cropData.water).ToString();
                    break;

                case CellState.WheatReadyForHarvest:
                    ShowUI(true, true, true, true, false);
                    statusText.text = "READY";
                    cropText.text = "WHEAT";
                    fertilizerLevel.text = Mathf.RoundToInt(cropData.fertilization).ToString();
                    waterLevel.text = Mathf.RoundToInt(cropData.water).ToString();
                    break;
            }
        }
        private void ShowUI(bool showStatus, bool showCrop, bool showFertile, bool showWater, bool showGrowth)
        {
            status.SetActive(showStatus);
            statusText.gameObject.SetActive(showStatus);

            crop.SetActive(showCrop);
            cropText.gameObject.SetActive(showCrop);

            fertilizer.SetActive(showFertile);
            fertilizerLevel.gameObject.SetActive(showFertile);
            fertilizerMax.SetActive(showFertile);

            water.SetActive(showWater);
            waterLevel.gameObject.SetActive(showWater);
            waterMax.SetActive(showWater);

            growth.SetActive(showGrowth);
            growthLevel.gameObject.SetActive(showGrowth);
            growthMax.SetActive(showGrowth);
        }
    }
}