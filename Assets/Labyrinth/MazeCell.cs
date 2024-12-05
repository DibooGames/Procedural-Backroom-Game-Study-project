using UnityEngine;

public class MazeCell : MonoBehaviour
{
    public bool IsVisited = false;  // Tracks whether the cell has been visited during maze generation.
    public GameObject WallNorth, WallSouth, WallEast, WallWest;  // References to each of the cell's walls.

    // Initializes all walls as active by default.
    public void InitializeWalls()
    {
        WallNorth.SetActive(true);
        WallSouth.SetActive(true);
        WallEast.SetActive(true);
        WallWest.SetActive(true);
    }

    public void InitializeWalls(bool north, bool south, bool east, bool west)
    {
        WallNorth.SetActive(north);
        WallSouth.SetActive(south);
        WallEast.SetActive(east);
        WallWest.SetActive(west);
    }

    public void InitializeWalls(Direction direction, bool active)
    {
        switch (direction)
        {
            case Direction.North:
                WallNorth.SetActive(active);
                break;
            case Direction.South:
                WallSouth.SetActive(active);
                break;
            case Direction.East:
                WallEast.SetActive(active);
                break;
            case Direction.West:
                WallWest.SetActive(active);
                break;
        }
    }
}