using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crop : MonoBehaviour
{
    private int harvestActionCount = 0;

    [HideInInspector]
    public Vector2Int cropGridPosition;

    public void ProcessToolAction(ItemDetails equippedItemDetails)
    {
        GridPropertyDetails gridPropertyDetails = GridPropertiesManager.Instance.GetGridPropertyDetails(cropGridPosition.x, cropGridPosition.y);

        if (gridPropertyDetails == null)
            return;

        ItemDetails seedItemDetails = InventoryManager.Instance.GetItemDetails(gridPropertyDetails.seedItemCode);
        if (seedItemDetails == null)
            return;

        CropDetails cropDetails = GridPropertiesManager.Instance.GetCropDetails(seedItemDetails.ItemCode);
        if (cropDetails == null)
            return;


        int requiredHarvestActions = cropDetails.RequiredHarvestActionsForTool(equippedItemDetails.ItemCode);
        if (requiredHarvestActions == -1)
            return;

        harvestActionCount += 1;

        if (harvestActionCount >= requiredHarvestActions)
            HarvestCrop(cropDetails, gridPropertyDetails);
    }

    private void HarvestCrop(CropDetails cropDetails, GridPropertyDetails gridPropertyDetails)
    {
        gridPropertyDetails.seedItemCode = -1;
        gridPropertyDetails.growthDays = -1;
        gridPropertyDetails.daysSinceLastHarvest = -1;
        gridPropertyDetails.daysSinceWatered = -1;

        GridPropertiesManager.Instance.SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails);

        HarvestActions(cropDetails, gridPropertyDetails);
    }

    private void HarvestActions(CropDetails cropDetails, GridPropertyDetails gridPropertyDetails)
    {
        SpawnHarvestedItem(cropDetails);

        Destroy(gameObject);
    }

    private void SpawnHarvestedItem(CropDetails cropDetails)
    {
        for (int i = 0; i < cropDetails.cropProducedItemCode.Length; i++)
        {
            int cropsToProcedure;

            if (cropDetails.cropProducedMinQuantity[i] == cropDetails.cropProducedMaxQuantity[i] ||
                cropDetails.cropProducedMaxQuantity[i] < cropDetails.cropProducedMinQuantity[i])
            {
                cropsToProcedure = cropDetails.cropProducedMinQuantity[i];
            }
            else
            {
                cropsToProcedure = Random.Range(cropDetails.cropProducedMinQuantity[i], cropDetails.cropProducedMaxQuantity[i] + 1);
            }

            for (int j = 0; j < cropsToProcedure; j++)
            {
                Vector3 spawnPosition;
                if (cropDetails.spawnCropProducedAtPlayerPosition)
                {
                    InventoryManager.Instance.AddItem(InventoryLocation.player, cropDetails.cropProducedItemCode[j]);
                }
                else
                {
                    spawnPosition = new Vector3(transform.position.x + Random.Range(-1f, 1f), transform.position.y + Random.Range(-1f, 1f), 0f);
                    SceneItemsManager.Instance.InstantiateSceneItem(cropDetails.cropProducedItemCode[j], spawnPosition);
                }
            }
        }
    }
}
