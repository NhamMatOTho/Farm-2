using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(GenerateGUID))]
public class GridPropertiesManager : SingletonMonobehavior<GridPropertiesManager>, ISaveable
{
    private Transform cropsParentTransform;
    private Tilemap groundDecoration1;
    private Tilemap groundDecoration2;
    private Grid grid;
    private Dictionary<string, GridPropertyDetails> gridPropertyDictionary;
    [SerializeField] private SO_CropDetailsList so_CropDetailsList = null;
    [SerializeField] private SO_GridProperties[] so_gridPropertiesArray = null;
    [SerializeField] private Tile[] dugGround = null;
    [SerializeField] private Tile[] wateredGround = null;

    private string _iSaveableUniqueID;

    private GameObjectSave _gameObjectSave;

    public string ISaveableUniqueID { get { return _iSaveableUniqueID; } set { _iSaveableUniqueID = value; } }
    public GameObjectSave GameObjectSave { get { return _gameObjectSave; } set { _gameObjectSave = value; } }

    protected override void Awake()
    {
        base.Awake();

        ISaveableUniqueID = GetComponent<GenerateGUID>().GUID;
        GameObjectSave = new GameObjectSave();
    }

    private void OnEnable()
    {
        ISaveableRegister();
        EventHandler.AfterSceneLoadEvent += AfterSceneLoaded;
        EventHandler.AdvanceGameDayEvent += AdvanceDay;
    }

    private void OnDisable()
    {
        ISaveableDeregister();
        EventHandler.AfterSceneLoadEvent -= AfterSceneLoaded;
        EventHandler.AdvanceGameDayEvent -= AdvanceDay;
    }

    private void Start()
    {
        InitialiseGridProperties();
    }

    private void ClearDisplayGroundDecorations()
    {
        //remove ground decoration
        groundDecoration1.ClearAllTiles();
        groundDecoration2.ClearAllTiles();
    }

    private void ClearDisplayGridPropertyDetails()
    {
        ClearDisplayGroundDecorations();

        ClearDisplayAllPlantedCrops();
    }

    private void ClearDisplayAllPlantedCrops()
    {
        Crop[] cropArray;
        cropArray = FindObjectsOfType<Crop>();

        foreach(Crop crop in cropArray)
        {
            Destroy(crop.gameObject);
        }
    }

    public void DisplayDugGround(GridPropertyDetails gridPropertyDetails)
    {
        if(gridPropertyDetails.daysSinceDug > -1)
        {
            ConnectDugGround(gridPropertyDetails);
        }
    }

    public void DisplayWateredGround(GridPropertyDetails gridPropertyDetails)
    {
        if (gridPropertyDetails.daysSinceWatered > -1)
        {
            ConnectWateredGround(gridPropertyDetails);
        }
    }

    private void ConnectDugGround(GridPropertyDetails gridPropertyDetails)
    {
        //select tile based on surrounding dug tile
        Tile dugTile0 = SetDugTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY);
        groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY, 0), dugTile0);

        //set 4 tile if dug surrounding current tile
        GridPropertyDetails adjacentGridPropertyDetails;

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceDug > -1)
        {
            Tile dugTile1 = SetDugTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1);
            groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1, 0), dugTile1);
        }

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceDug > -1)
        {
            Tile dugTile2 = SetDugTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1);
            groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1, 0), dugTile2);
        }

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceDug > -1)
        {
            Tile dugTile3 = SetDugTile(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY);
            groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY, 0), dugTile3);
        }

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceDug > -1)
        {
            Tile dugTile4 = SetDugTile(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY);
            groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY, 0), dugTile4);
        }
    }

    private void ConnectWateredGround(GridPropertyDetails gridPropertyDetails)
    {
        Tile wateredTile0 = SetWateredTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY);
        groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY, 0), wateredTile0);

        //set 4 tile if watered surrounding current tile
        GridPropertyDetails adjacentGridPropertyDetails;

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceWatered > -1)
        {
            Tile wateredTile1 = SetWateredTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1);
            groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1, 0), wateredTile1);
        }

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceWatered > -1)
        {
            Tile wateredTile2 = SetWateredTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1);
            groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1, 0), wateredTile2);
        }

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceWatered > -1)
        {
            Tile wateredTile3 = SetWateredTile(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY);
            groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY, 0), wateredTile3);
        }

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceWatered > -1)
        {
            Tile wateredTile4 = SetWateredTile(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY);
            groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY, 0), wateredTile4);
        }
    }

    private Tile SetDugTile(int xGrid, int yGrid)
    {
        bool upDug = IsGridSquareDug(xGrid, yGrid + 1);
        bool downDug = IsGridSquareDug(xGrid, yGrid - 1);
        bool leftDug = IsGridSquareDug(xGrid - 1, yGrid);
        bool rightDug = IsGridSquareDug(xGrid + 1, yGrid);

        #region Set approciate tile based on whether surrounding tiles are dug or not
        if(!upDug && !downDug && !leftDug && !rightDug)
        {
            return dugGround[0];
        }
        else if(!upDug && downDug && !leftDug && rightDug)
        {
            return dugGround[1];
        }
        else if (!upDug && downDug && leftDug && rightDug)
        {
            return dugGround[2];
        }
        else if (!upDug && downDug && leftDug && !rightDug)
        {
            return dugGround[3];
        }
        else if (!upDug && downDug && !leftDug && !rightDug)
        {
            return dugGround[4];
        }
        else if (upDug && downDug && !leftDug && rightDug)
        {
            return dugGround[5];
        }
        else if (upDug && downDug && leftDug && rightDug)
        {
            return dugGround[6];
        }
        else if (upDug && downDug && leftDug && !rightDug)
        {
            return dugGround[7];
        }
        else if (upDug && downDug && !leftDug && !rightDug)
        {
            return dugGround[8];
        }
        else if (upDug && !downDug && !leftDug && rightDug)
        {
            return dugGround[9];
        }
        else if (upDug && !downDug && leftDug && rightDug)
        {
            return dugGround[10];
        }
        else if (upDug && !downDug && leftDug && !rightDug)
        {
            return dugGround[11];
        }
        else if (upDug && !downDug && !leftDug && !rightDug)
        {
            return dugGround[12];
        }
        else if (!upDug && !downDug && !leftDug && rightDug)
        {
            return dugGround[13];
        }
        else if (!upDug && !downDug && leftDug && rightDug)
        {
            return dugGround[14];
        }
        else if (!upDug && !downDug && leftDug && !rightDug)
        {
            return dugGround[15];
        }

        return null;

        #endregion
    }

    private Tile SetWateredTile(int xGrid, int yGrid)
    {
        bool upWatered = IsGridSquareWatered(xGrid, yGrid + 1);
        bool downWatered = IsGridSquareWatered(xGrid, yGrid - 1);
        bool leftWatered = IsGridSquareWatered(xGrid - 1, yGrid);
        bool rightWatered = IsGridSquareWatered(xGrid + 1, yGrid);

        #region Set approciate tile based on whether surrounding tiles are Watered or not
        if (!upWatered && !downWatered && !leftWatered && !rightWatered)
        {
            return wateredGround[0];
        }
        else if (!upWatered && downWatered && !leftWatered && rightWatered)
        {
            return wateredGround[1];
        }
        else if (!upWatered && downWatered && leftWatered && rightWatered)
        {
            return wateredGround[2];
        }
        else if (!upWatered && downWatered && leftWatered && !rightWatered)
        {
            return wateredGround[3];
        }
        else if (!upWatered && downWatered && !leftWatered && !rightWatered)
        {
            return wateredGround[4];
        }
        else if (upWatered && downWatered && !leftWatered && rightWatered)
        {
            return wateredGround[5];
        }
        else if (upWatered && downWatered && leftWatered && rightWatered)
        {
            return wateredGround[6];
        }
        else if (upWatered && downWatered && leftWatered && !rightWatered)
        {
            return wateredGround[7];
        }
        else if (upWatered && downWatered && !leftWatered && !rightWatered)
        {
            return wateredGround[8];
        }
        else if (upWatered && !downWatered && !leftWatered && rightWatered)
        {
            return wateredGround[9];
        }
        else if (upWatered && !downWatered && leftWatered && rightWatered)
        {
            return wateredGround[10];
        }
        else if (upWatered && !downWatered && leftWatered && !rightWatered)
        {
            return wateredGround[11];
        }
        else if (upWatered && !downWatered && !leftWatered && !rightWatered)
        {
            return wateredGround[12];
        }
        else if (!upWatered && !downWatered && !leftWatered && rightWatered)
        {
            return wateredGround[13];
        }
        else if (!upWatered && !downWatered && leftWatered && rightWatered)
        {
            return wateredGround[14];
        }
        else if (!upWatered && !downWatered && leftWatered && !rightWatered)
        {
            return wateredGround[15];
        }

        return null;

        #endregion
    }

    private bool IsGridSquareDug(int xGrid, int yGrid)
    {
        GridPropertyDetails gridPropertyDetails = GetGridPropertyDetails(xGrid, yGrid);

        if (gridPropertyDetails == null)
            return false;
        else if (gridPropertyDetails.daysSinceDug > -1)
            return true;
        else return false;
    }

    private bool IsGridSquareWatered(int xGrid, int yGrid)
    {
        GridPropertyDetails gridPropertyDetails = GetGridPropertyDetails(xGrid, yGrid);

        if (gridPropertyDetails == null)
            return false;
        else if (gridPropertyDetails.daysSinceWatered > -1)
            return true;
        else return false;
    }

    private void DisplayGridPropertyDetails()
    {
        foreach(KeyValuePair<string, GridPropertyDetails> item in gridPropertyDictionary)
        {
            GridPropertyDetails gridPropertyDetails = item.Value;
            DisplayDugGround(gridPropertyDetails);
            DisplayWateredGround(gridPropertyDetails);
            DisplayPlantedCrop(gridPropertyDetails);
        }
    }

    public void DisplayPlantedCrop(GridPropertyDetails gridPropertyDetails)
    {
        if(gridPropertyDetails.seedItemCode > -1)
        {
            CropDetails cropDetails = so_CropDetailsList.GetCropDetails(gridPropertyDetails.seedItemCode);

            if (cropDetails != null)
            {
                GameObject cropPrefab;

                int growthStages = cropDetails.growthDays.Length;
                int currentGrowthStage = 0;

                for (int i = growthStages - 1; i >= 0; i--)
                {
                    if (gridPropertyDetails.growthDays >= cropDetails.growthDays[i])
                    {
                        currentGrowthStage = i;
                        break;
                    }
                }

                cropPrefab = cropDetails.growthPrefab[currentGrowthStage];
                Sprite growthSprite = cropDetails.growthSprite[currentGrowthStage];

                Vector3 worldPosition = groundDecoration2.CellToWorld(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY, 0));
                worldPosition = new Vector3(worldPosition.x + Settings.gridCellSize / 2, worldPosition.y, worldPosition.z);

                GameObject cropInstance = Instantiate(cropPrefab, worldPosition, Quaternion.identity);
                cropInstance.GetComponentInChildren<SpriteRenderer>().sprite = growthSprite;
                cropInstance.transform.SetParent(cropsParentTransform);
                cropInstance.GetComponent<Crop>().cropGridPosition = new Vector2Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY);
            }
        }
    }

    /// <summary>
    /// Initialise the grid property dictionary with the values from the so_gridProperties asset and store the values for each scene in
    /// GameObjectSave sceneData
    /// </summary>
    private void InitialiseGridProperties()
    {
        foreach(SO_GridProperties so_GridProperties in so_gridPropertiesArray)
        {
            Dictionary<string, GridPropertyDetails> gridPropertyDictionary = new Dictionary<string, GridPropertyDetails>();
            foreach(GridProperty gridProperty in so_GridProperties.gridPropertyList)
            {
                GridPropertyDetails gridPropertyDetails;
                gridPropertyDetails = GetGridPropertyDetails(gridProperty.gridCoordinate.x, gridProperty.gridCoordinate.y, gridPropertyDictionary);

                if(gridPropertyDetails == null)
                {
                    gridPropertyDetails = new GridPropertyDetails();
                }

                switch (gridProperty.gridBoolProperty)
                {
                    case GridBoolProperty.diggable:
                        gridPropertyDetails.isDiggable = gridProperty.gridBoolValue;
                        break;
                    case GridBoolProperty.canDropitem:
                        gridPropertyDetails.canDropItem = gridProperty.gridBoolValue;
                        break;
                    case GridBoolProperty.canPlaceFurniture:
                        gridPropertyDetails.canPlaceFurniture = gridProperty.gridBoolValue;
                        break;
                    case GridBoolProperty.isPath:
                        gridPropertyDetails.isPath = gridProperty.gridBoolValue;
                        break;
                    case GridBoolProperty.isNPCObstacle:
                        gridPropertyDetails.isNPCObstacle = gridProperty.gridBoolValue;
                        break;
                    default:
                        break;
                }
                SetGridPropertyDetails(gridProperty.gridCoordinate.x, gridProperty.gridCoordinate.y, gridPropertyDetails, gridPropertyDictionary);
            }
            SceneSave sceneSave = new SceneSave();
            sceneSave.gridPropertyDetailsDictionary = gridPropertyDictionary;
            if(so_GridProperties.sceneName.ToString() == SceneControllerManager.Instance.startingSceneName.ToString())
            {
                this.gridPropertyDictionary = gridPropertyDictionary;
            }
            GameObjectSave.sceneData.Add(so_GridProperties.sceneName.ToString(), sceneSave);
        }
    }

    private void AfterSceneLoaded()
    {
        if(GameObject.FindGameObjectWithTag(Tags.CropsParentTransform) != null)
        {
            cropsParentTransform = GameObject.FindGameObjectWithTag(Tags.CropsParentTransform).transform;
        }
        else
        {
            cropsParentTransform = null;
        }

        grid = GameObject.FindObjectOfType<Grid>();

        groundDecoration1 = GameObject.FindGameObjectWithTag(Tags.GroundDecoration1).GetComponent<Tilemap>();
        groundDecoration2 = GameObject.FindGameObjectWithTag(Tags.GroundDecoration2).GetComponent<Tilemap>();
    }

    public GridPropertyDetails GetGridPropertyDetails(int gridX, int gridY, Dictionary<string, GridPropertyDetails> gridPropertyDictionary)
    {
        string key = "x" + gridX + "y" + gridY;
        GridPropertyDetails gridPropertyDetails;

        if(!gridPropertyDictionary.TryGetValue(key, out gridPropertyDetails))
        {
            return null;
        }
        else
        {
            return gridPropertyDetails;
        }
    }

    public GridPropertyDetails GetGridPropertyDetails(int gridX, int gridY)
    {
        return GetGridPropertyDetails(gridX, gridY, gridPropertyDictionary);
    }

    public void SetGridPropertyDetails(int gridX, int gridY, GridPropertyDetails gridPropertyDetails)
    {
        SetGridPropertyDetails(gridX, gridY, gridPropertyDetails, gridPropertyDictionary);
    }

    public void SetGridPropertyDetails(int gridX, int gridY, GridPropertyDetails gridPropertyDetails,Dictionary<string, GridPropertyDetails> gridPropertyDictionary)
    {
        string key = "x" + gridX + "y" + gridY;
        gridPropertyDetails.gridX = gridX;
        gridPropertyDetails.gridY = gridY;
        gridPropertyDictionary[key] = gridPropertyDetails;
    }

    public Crop GetCropObjectAtGridLocation(GridPropertyDetails gridPropertyDetails)
    {
        Vector3 worldPosition = grid.GetCellCenterWorld(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY, 0));
        Collider2D[] collider2DArray = Physics2D.OverlapPointAll(worldPosition);

        Crop crop = null;

        for (int i = 0; i < collider2DArray.Length; i++)
        {
            crop = collider2DArray[i].gameObject.GetComponentInParent<Crop>();
            if (crop != null && crop.cropGridPosition == new Vector2Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY))
                break;
            crop = collider2DArray[i].gameObject.GetComponentInChildren<Crop>();
            if (crop != null && crop.cropGridPosition == new Vector2Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY))
                break;
        }
        return crop;
    }

    public CropDetails GetCropDetails(int seedItemCode)
    {
        return so_CropDetailsList.GetCropDetails(seedItemCode);
    }

    public void ISaveableRegister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Add(this);
    }

    public void ISaveableDeregister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Remove(this);
    }

    public void ISaveableStoreScene(string sceneName)
    {
        GameObjectSave.sceneData.Remove(sceneName);
        SceneSave sceneSave = new SceneSave();
        sceneSave.gridPropertyDetailsDictionary = gridPropertyDictionary;
        GameObjectSave.sceneData.Add(sceneName, sceneSave);
    }

    public void ISaveableRestoreScene(string sceneName)
    {
        if (GameObjectSave.sceneData.TryGetValue(sceneName, out SceneSave sceneSave))
        {
            if (sceneSave.gridPropertyDetailsDictionary != null)
            {
                gridPropertyDictionary = sceneSave.gridPropertyDetailsDictionary;
            }

            if(gridPropertyDictionary.Count > 0)
            {
                ClearDisplayGridPropertyDetails();
                DisplayGridPropertyDetails();
            }
        }
    }

    private void AdvanceDay(int gameYear, Season gameSeason, int gameDay, string gameDayOfWeek, int gameHour, int gameMinute, int gameSecond)
    {
        ClearDisplayGridPropertyDetails();

        foreach(SO_GridProperties so_GridProperties in so_gridPropertiesArray)
        {
            if (GameObjectSave.sceneData.TryGetValue(so_GridProperties.sceneName.ToString(), out SceneSave sceneSave))
            {
                if(sceneSave.gridPropertyDetailsDictionary != null)
                {
                    for(int i = sceneSave.gridPropertyDetailsDictionary.Count - 1; i >= 0; i--)
                    {
                        KeyValuePair<string, GridPropertyDetails> item = sceneSave.gridPropertyDetailsDictionary.ElementAt(i);

                        GridPropertyDetails gridPropertyDetails = item.Value;

                        #region Update all grid properties to reflect the advance in the day
                        if (gridPropertyDetails.growthDays > -1)
                            gridPropertyDetails.growthDays += 1;
                        
                        if (gridPropertyDetails.daysSinceWatered > -1)
                            gridPropertyDetails.daysSinceWatered = -1;

                        SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails, sceneSave.gridPropertyDetailsDictionary);
                        #endregion
                    }
                }
            }

        }

        DisplayGridPropertyDetails();
    }
}
