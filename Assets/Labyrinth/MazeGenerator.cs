using UnityEngine;
using System.Collections.Generic;

public enum Direction
{
    North,
    South,
    East,
    West,
    Up,
    Down
}

public class OldMazeGenerator : MonoBehaviour
{
    public int gridWidth = 10, gridHeight = 10;
    public float cellSize = 1.0f;
    public MazeCell[,] grid = null;
    public GameObject cellPrefab;
    
    public bool useCustomSeed = false;
    public int seed = 0;

       // Room settings
    public int roomCount = 2;        // Number of rooms to carve
    public int roomMinSize = 2;      // Minimum room size
    public int roomMaxSize = 4;      // Maximum room size

    private void Start()
    {
        if (useCustomSeed)
        {
            Random.InitState(seed);
        }
        else
        {
            Random.InitState(System.DateTime.Now.GetHashCode());
        }

        InitializeGrid();
        GenerateMaze();
        CarveRooms();
    }

    private void InitializeGrid()
    {
        grid = new MazeCell[gridWidth, gridHeight];
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                Vector3 cellPosition = new Vector3(x * cellSize, 0, z * cellSize);
                GameObject cellObj = Instantiate(cellPrefab, cellPosition, Quaternion.identity);
                grid[x, z] = cellObj.GetComponent<MazeCell>();
                grid[x, z].InitializeWalls();
            }
        }
    }

    private void GenerateMaze()
    {
        DFSCarve(0, 0);
    }

    private void DFSCarve(int x, int z)
    {
        grid[x, z].IsVisited = true;

        var directions = new List<Direction> { Direction.East, Direction.West, Direction.North, Direction.South };
        Shuffle(directions, new System.Random());

        foreach (var direction in directions)
        {
            int nextX = x, nextZ = z;

            switch (direction)
            {
                case Direction.North: nextZ += 1; break;
                case Direction.South: nextZ -= 1; break;
                case Direction.East: nextX += 1; break;
                case Direction.West: nextX -= 1; break;
            }

            if (IsInBounds(nextX, nextZ) && !grid[nextX, nextZ].IsVisited)
            {
                RemoveWallBetweenCells(grid[x, z], grid[nextX, nextZ], direction);
                DFSCarve(nextX, nextZ);
            }
        }
    }

    private bool IsInBounds(int x, int z)
    {
        return x >= 0 && x < gridWidth && z >= 0 && z < gridHeight;
    }

    private void RemoveWallBetweenCells(MazeCell current, MazeCell next, Direction direction)
    {
        switch (direction)
        {
            case Direction.North:
                current.WallNorth.SetActive(false);
                next.WallSouth.SetActive(false);
                break;
            case Direction.South:
                current.WallSouth.SetActive(false);
                next.WallNorth.SetActive(false);
                break;
            case Direction.East:
                current.WallEast.SetActive(false);
                next.WallWest.SetActive(false);
                break;
            case Direction.West:
                current.WallWest.SetActive(false);
                next.WallEast.SetActive(false);
                break;
        }
    }

     private void CarveRooms()
    {
        for (int i = 0; i < roomCount; i++)
        {
           
            int roomWidth = Random.Range(roomMinSize, roomMaxSize);
            int roomHeight = Random.Range(roomMinSize, roomMaxSize);
            int startX = Random.Range(1, gridWidth - roomWidth - 1);
            int startZ = Random.Range(1, gridHeight - roomHeight - 1);

            
            for (int x = startX; x < startX + roomWidth; x++)
            {
                for (int z = startZ; z < startZ + roomHeight; z++)
                {
                    if (IsInBounds(x, z))
                    {
                        grid[x, z].IsVisited = true;
                        grid[x, z].InitializeWalls(false, false, false, false); 
                    }
                }
            }
        }
    }


    void Shuffle(List<Direction> list, System.Random rng)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(0, i + 1);
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}