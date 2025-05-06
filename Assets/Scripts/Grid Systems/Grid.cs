using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class Grid : MonoBehaviour
{
    [SerializeField] private int mapWidth, mapHeight;
    [SerializeField] private GameObject tileReference;
    [SerializeField] private GameObject tileStraight;
    [SerializeField] private GameObject tileTurn;
    [SerializeField] private GameObject tileStart;
    [SerializeField] private GameObject tileEnd;
    [SerializeField] private GameObject waypointPrefab;


    private GameObject waypointsParent;
    private GameObject tilesParent;

    public Cell[,] grid;
    public Transform startPoint { get; private set; }
    public GameObject endPoint { get; private set; }
    public bool isMapReady = false;
    public static Grid Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GenerateParentObjects();

        GenerateGrid();
        StartCoroutine(GeneratePath());
    }

    private enum MovementDirection
    {
        NONE,
        LEFT,
        RIGHT,
        DOWN,
        UP
    }

    //float[,] GenerateNoiseMap()
    //{
    //    float[,] noiseMap = new float[size, size];
    //    float xOffset = Random.Range(-10000f, 10000f);
    //    float yOffset = Random.Range(-10000f, 10000f);

    //    for (int x = 0; x < size; x++)
    //        for (int y = 0; y < size; y++)
    //        {
    //            float noiseValue = Mathf.PerlinNoise(y * noiseScale + yOffset, x * noiseScale + xOffset);
    //            noiseMap[y, x] = noiseValue;
    //        }

    //    return noiseMap;
    //}

    public void GetMapSize(out  int outMapWidth, out int outMapHeight)
    {
        outMapHeight = mapHeight;
        outMapWidth = mapWidth;
    }

    void GenerateParentObjects()
    {
        waypointsParent = new GameObject("waypointsParent");
        tilesParent = new GameObject("tilesParent");
    }

    void PostGeneration()
    {
        isMapReady = true;         // Map is ready
        waypointsParent.AddComponent<Waypoints>();


        onFinishMapGeneration?.Invoke();
    }

    public Cell GetCell(int x, int y)
    {
        return grid[x, y];
    }

    void GenerateGrid()
    {
        grid = new Cell[mapWidth, mapHeight];

        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
            {
                Cell cell = new Cell();
                GameObject newCell = Instantiate(tileReference, new Vector3(x, 0, y), Quaternion.identity);
                cell.renderer = newCell.GetComponent<Renderer>();
                cell.isPath = false;
                cell.transform = newCell.transform;
                cell.transform.parent = tilesParent.transform;

                grid[x,y] = cell;
            }
    }

    IEnumerator GeneratePath()
    {
        int startX = UnityEngine.Random.Range(3, mapWidth - 3);
        int curX = startX;
        int curY = 0;

        MovementDirection currentDirection = MovementDirection.UP;
        List<MovementDirection> movementHistory = new List<MovementDirection>();

        UpdateMap(curX, curY);
        startPoint = grid[curX, curY].transform;

        movementHistory.Add(MovementDirection.NONE);
        movementHistory.Add(currentDirection);

        while (curY <= mapHeight - 2)
        {
            ChooseDirection(ref curX, ref curY,ref currentDirection, movementHistory);

            //Debug.Log($"NextDir: {currentDirection}");

            if (curY <= mapHeight - 2)
            {
                UpdateMap(curX, curY);
                movementHistory.Add(currentDirection);
            }

            yield return new WaitForSeconds(0.05f);
        }

        UpdateMap(curX, curY);
        movementHistory.Add(MovementDirection.UP);
        movementHistory.RemoveAt(0);

        IterateThroughtPathAndInstantiateCells(startX, movementHistory);

        PostGeneration();
    }

    void ChooseDirection(ref int curX, ref int curY, ref MovementDirection currentDirection, List<MovementDirection> movementHistory)
    {
        int changeDirectionPerc = UnityEngine.Random.Range(0,101);
        int changeDirectionThreshold;

        if (currentDirection == MovementDirection.UP)
            changeDirectionThreshold = 60;
        else
            changeDirectionThreshold = 50;

        if (changeDirectionPerc <= changeDirectionThreshold - CountLastUpDirectionAndReduceChangeDirThershold(movementHistory))     // keep direction + reduced threshold
            if (!CheckDirectionNeighbourPath(currentDirection, ref curX, ref curY))
                return;

        List<MovementDirection> validDirections = FillValidDirections(currentDirection, curX, curY, movementHistory);
        MovementDirection possiblyNewDirection = currentDirection;
        bool isNewDirectionValid = false;
        int attemptCounter = 0;
        System.Random random = new System.Random();

        while(!isNewDirectionValid && attemptCounter < 3)
        {
            attemptCounter++;

            possiblyNewDirection = validDirections[random.Next(validDirections.Count)];

            isNewDirectionValid = !CheckDirectionNeighbourPath(possiblyNewDirection, ref curX, ref curY);
        }

        if(attemptCounter == 3)
            Debug.Log($"Attempt: {attemptCounter};      CurrDir: {currentDirection};        PossDir: {possiblyNewDirection}");

        if (isNewDirectionValid)
            currentDirection = possiblyNewDirection;

    }

    bool IsNeighbourAPath(int x, int y)
    {
        if (x>=3 && x<mapWidth-3 && y>=0 && y<mapHeight)
            return grid[x, y].isPath;

        return true;
    }

    int CountLastUpDirectionAndReduceChangeDirThershold(List<MovementDirection> movementHistory)
    {
        int counter = 0;

        for(int i=movementHistory.Count -1; i>movementHistory.Count-6;i--)
        {
            if (movementHistory[i] == MovementDirection.UP)
                counter++;
            else
                break;
        }

        return counter * 5;
    }

    List<MovementDirection> FillValidDirections(MovementDirection excludeDireciton, int curX, int curY, List<MovementDirection> movementHistory)
    {
        List<MovementDirection> validDirections = new List<MovementDirection>();

        validDirections.Add(MovementDirection.UP);

        if (excludeDireciton != MovementDirection.LEFT && excludeDireciton != MovementDirection.RIGHT && movementHistory[movementHistory.Count - 2] == MovementDirection.UP)
        {
            if (curX > 0)
                validDirections.Add(MovementDirection.LEFT);
            if (curX < mapWidth - 1)
                validDirections.Add(MovementDirection.RIGHT);
        }

        return validDirections;
    }

    bool CheckDirectionNeighbourPath(MovementDirection newDirection, ref int curX, ref int curY)
    {
            switch (newDirection)
            {
                case MovementDirection.UP:
                    if (!IsNeighbourAPath(curX, curY + 1))
                    {
                        curY++;
                        return false;
                    }
                    break;
                case MovementDirection.LEFT:
                    if (!IsNeighbourAPath(curX - 1, curY))
                    {
                        curX--;
                        return false;
                    }
                    break;
                case MovementDirection.RIGHT:
                    if (!IsNeighbourAPath(curX + 1, curY))
                    {
                        curX++;
                        return false;
                    }
                    break;
                case MovementDirection.DOWN:
                    if (!IsNeighbourAPath(curX, curY - 1))
                    {
                        curY--;
                        return false;
                    }
                    break;
            }

            return true;
        }

    private void UpdateMap(int mapX, int mapY)
    {
        grid[mapX, mapY].transform.position = new Vector3(grid[mapX, mapY].transform.position.x, -0.3f, grid[mapX, mapY].transform.position.z);
        grid[mapX, mapY].isPath = true;
        grid[mapX, mapY].renderer.material.color = new Color(255, 255, 0);
        GenerateWaypoint(mapX, mapY);


    }

    void GenerateWaypoint(int mapX, int mapY)
    {
        GameObject waypointObject = Instantiate(waypointPrefab, new Vector3(grid[mapX, mapY].transform.position.x, 0.3f, grid[mapX, mapY].transform.position.z), Quaternion.identity);

        waypointObject.transform.parent = waypointsParent.transform;
    }

    void IterateThroughtPathAndInstantiateCells(int x ,List<MovementDirection> movementHistory)
    {
        int y = 0;

        for (int i = 0; i < movementHistory.Count-1; i++)
        {
            float rotation = 0;
            GameObject tileToSet = GetTileToAndRotationSet(movementHistory, i, ref rotation);
            InstantiateCell(x, y, tileToSet, rotation);

            switch (movementHistory[i])
            {
                case MovementDirection.UP:
                    if (movementHistory[i + 1] == MovementDirection.UP)
                        y++;
                    else if (movementHistory[i + 1] == MovementDirection.LEFT)
                        x--;
                    else if (movementHistory[i + 1] == MovementDirection.RIGHT)
                        x++;
                    break;
                case MovementDirection.LEFT:
                    if (movementHistory[i + 1] == MovementDirection.LEFT)
                        x--;
                    if (movementHistory[i + 1] == MovementDirection.UP)
                        y++;
                    else if (movementHistory[i + 1] == MovementDirection.DOWN)
                        y--;
                    break;
                case MovementDirection.RIGHT:
                    if (movementHistory[i + 1] == MovementDirection.RIGHT)
                        x++;
                    if (movementHistory[i + 1] == MovementDirection.UP)
                        y++;
                    else if (movementHistory[i + 1] == MovementDirection.DOWN)
                        y--;
                    break;
            }
        }

        InstantiateCell(x, y, tileEnd, -180f);
    }

    void InstantiateCell(int mapX, int mapY, GameObject tileToSet, float rotation)
    {
        if (tileToSet == null)
            return;

        if (!grid[mapX, mapY].isPath)
            Debug.Log($"ERROR");

        //grid[mapX, mapY].transform.position = new Vector3(grid[mapX, mapY].transform.position.x, -0.3f, grid[mapX, mapY].transform.position.z);

        GameObject newCell = Instantiate(tileToSet, new Vector3(mapX, 0, mapY), Quaternion.identity);
        newCell.transform.parent = tilesParent.transform;

        if (rotation != 0)
            newCell.transform.Rotate(0, rotation, 0, Space.Self);

        grid[mapX, mapY].renderer = newCell.GetComponent<Renderer>();
        grid[mapX, mapY].transform = newCell.transform;
        //grid[mapX, mapY].isPath = true;

        if(tileToSet == tileEnd)
            endPoint = newCell;
    }

    GameObject GetTileToAndRotationSet(List<MovementDirection> movementHistory, int index, ref float rotation)
    {
        if (index == 0)
            return tileStart;

        MovementDirection currentDir = movementHistory[index];
        MovementDirection nextDir = movementHistory[index + 1];

        if (currentDir != nextDir)
        {
            switch(currentDir)
            {
                case MovementDirection.UP:
                    if (nextDir == MovementDirection.LEFT)
                        rotation = 180;
                    else if (nextDir == MovementDirection.RIGHT)
                        rotation = 90;
                    break;
                case MovementDirection.LEFT:
                    if (nextDir == MovementDirection.UP)
                        rotation = 0;
                    else if(nextDir == MovementDirection.DOWN)
                        rotation = 90;
                    break;
                case MovementDirection.RIGHT:
                    if (nextDir == MovementDirection.UP)
                        rotation = -90;
                    else if (nextDir == MovementDirection.DOWN)
                        rotation = 180;
                    break;
            }

            return tileTurn;
        }
        else
        {
            switch (currentDir)
            {
                case MovementDirection.UP:
                case MovementDirection.DOWN:
                case MovementDirection.NONE:
                    rotation = 0f;
                    break;
                case MovementDirection.LEFT:
                    rotation = -90f;
                    break;
                case MovementDirection.RIGHT:
                    rotation = 90f;
                    break;
            }

            return tileStraight;
        }
    }

    public event Action onFinishMapGeneration;
}
