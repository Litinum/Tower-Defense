using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BotSimulator : MonoBehaviour
{
    [Range(0f,1f)]
    public double chanceToChangeUnafforableStructure = 0.2;
    public int buildAwayFromPath = 2;

    private GameObject[] towerPrefabs;
    private GameObject structureToBuild;
    private List<Cell> buildableCells = new List<Cell>();
    private Cell cellToBuildOn;
    private int mapWidth, mapHeight;
    private bool isFirstStructurePlaced;

    // Start is called before the first frame update
    void Start()
    {
        towerPrefabs = Resources.LoadAll<GameObject>("Structures");

        Grid.Instance.GetMapSize(out mapWidth, out mapHeight);
        GetBuildableCells();
    }

    // Update is called once per frame
    void Update()
    {
        // Choose structure to build (randomly, but prefer affordable)
        // Place structure (next to path)
        // Randomly choose target priority
        if (GameLoop.Instance.isGameLoopRunning)
        {
            if (structureToBuild == null)
            {
                structureToBuild = ChooseStructureToBuild();
                BuildingSystem.Instance.SetBuildStructure(structureToBuild);
            }

            if (cellToBuildOn == null && buildableCells.Count() > 0)
                cellToBuildOn = ChooseRandomCellToBuildOn();

            if(structureToBuild != null && cellToBuildOn != null)
            {
                if(BuildOnCell(cellToBuildOn))
                {
                    structureToBuild = null;
                    cellToBuildOn = null;
                    isFirstStructurePlaced = true;
                }
            }
        }
    }

    GameObject ChooseStructureToBuild()
    {
        GameObject selected = towerPrefabs[Random.Range(0, towerPrefabs.Length)];
        TowerBehaviour towerBehaviour = selected.GetComponent<TowerBehaviour>();

        if (towerBehaviour.GetBuildCost() > PlayerStats.Instance.gold)
        {
            if (Random.Range(0f, 1f) < chanceToChangeUnafforableStructure || !isFirstStructurePlaced)
                return ChooseStructureToBuild();
        }

        return selected;
    }

    void GetBuildableCells()
    {
        for(int x=0;x<mapWidth;x++)
            for(int y=0; y<mapHeight; y++)
                if(Grid.Instance.GetCell(x,y).isPath)
                    for(int i=x-buildAwayFromPath;i>=0 && i<mapWidth && i<=x+buildAwayFromPath; i++)
                        for(int j=y-buildAwayFromPath;j>=0 && j<mapHeight && j<=y+buildAwayFromPath; j++)
                        {
                            Cell cell = Grid.Instance.GetCell(i, j);
                            if (!cell.isPath)
                                buildableCells.Add(cell);
                        }

        Debug.Log($"BOT: Buildable Cells: {buildableCells.Count()}");
    }

    Cell ChooseRandomCellToBuildOn()
    {
        int cellIndex = Random.Range(0, buildableCells.Count());
        Cell cell = buildableCells[cellIndex];
        Debug.Log($"Selected Cell: {cell}");
        buildableCells.RemoveAt(cellIndex);

        return cell;
    }

    bool BuildOnCell(Cell cell)
    {
        int x, z;

        cellToBuildOn.GetXZ(new Vector3(cellToBuildOn.transform.position.x,0, cellToBuildOn.transform.position.z), out x, out z);
        return BuildingSystem.Instance.BuildStructure(x, z);
    }
}
