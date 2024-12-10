using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    public int maxRooms = 10;
    public Room startRoomPrefab, endRoomPrefab;
    public List<Room> availableRooms;
    public GenerationRules generationRules;
    public int seed = 0;

    private int currentRoomCount;
    private List<Room> generatedRooms;

    void Start()
    {
        Random.InitState(seed != 0 ? seed : System.Environment.TickCount);
        GenerateDungeon();
    }

    void GenerateDungeon()
    {
        currentRoomCount = 1;
        generatedRooms = new List<Room>();
        Queue<Room> roomsToProcess = new Queue<Room>();
        Room startRoom = Instantiate(startRoomPrefab, Vector3.zero, Quaternion.identity);
        generatedRooms.Add(startRoom);
        roomsToProcess.Enqueue(startRoom);

        while (roomsToProcess.Count > 0 && currentRoomCount < maxRooms)
        {
            Room currentRoom = roomsToProcess.Dequeue();
            ShuffleExits(currentRoom.exitPoints);

            foreach (ExitPoint exit in currentRoom.exitPoints)
            {
                if (exit.isConnected) continue;
                bool exitConnected = false;
                List<Room> possibleRooms = GetPossibleRooms(currentRoom.roomType);
                ShuffleRooms(possibleRooms);

                foreach (Room nextRoomPrefab in possibleRooms)
                {
                    Vector3 tempPosition = new Vector3(10000, 10000, 10000);
                    Room newRoom = Instantiate(nextRoomPrefab, tempPosition, Quaternion.identity);
                    if (newRoom.roomCollider != null) newRoom.roomCollider.enabled = false;
                    List<ExitPoint> newRoomExits = new List<ExitPoint>(newRoom.exitPoints);
                    ShuffleExits(newRoomExits);
                    bool roomPlaced = false;

                    foreach (ExitPoint newExit in newRoomExits)
                    {
                        if (newExit.isConnected) continue;
                        ConnectRooms(currentRoom, exit, newRoom, newExit);
                        if (newRoom.roomCollider != null) newRoom.roomCollider.enabled = true;

                        if (!IsOverlapping(newRoom))
                        {
                            generatedRooms.Add(newRoom);
                            roomsToProcess.Enqueue(newRoom);
                            currentRoomCount++;
                            exit.isConnected = true;
                            newExit.isConnected = true;
                            currentRoom.connectedRooms.Add(newRoom);
                            newRoom.connectedRooms.Add(currentRoom);
                            roomPlaced = true;
                            exitConnected = true;
                            break;
                        }
                        else
                        {
                            newRoom.transform.position = tempPosition;
                            newRoom.transform.rotation = Quaternion.identity;
                            if (newRoom.roomCollider != null) newRoom.roomCollider.enabled = false;
                        }
                    }

                    if (roomPlaced) break;
                    else Destroy(newRoom.gameObject);
                }

                if (!exitConnected) continue;
                if (currentRoomCount >= maxRooms) break;
            }
        }

        CloseOpenExits();
    }

    List<Room> GetPossibleRooms(string currentRoomType)
    {
        List<Room> possibleRooms = new List<Room>();
        foreach (Room room in availableRooms)
        {
            if (generationRules.CanConnect(currentRoomType, room.roomType))
                possibleRooms.Add(room);
        }
        return possibleRooms;
    }

    void ShuffleRooms(List<Room> rooms)
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            Room temp = rooms[i];
            int randomIndex = Random.Range(i, rooms.Count);
            rooms[i] = rooms[randomIndex];
            rooms[randomIndex] = temp;
        }
    }

    void ShuffleExits(List<ExitPoint> exits)
    {
        for (int i = 0; i < exits.Count; i++)
        {
            ExitPoint temp = exits[i];
            int randomIndex = Random.Range(i, exits.Count);
            exits[i] = exits[randomIndex];
            exits[randomIndex] = temp;
        }
    }

    void ConnectRooms(Room currentRoom, ExitPoint currentExit, Room newRoom, ExitPoint newExit)
    {
        Vector3 currentExitDirection = currentExit.exitTransform.forward;
        currentExitDirection.y = 0;
        currentExitDirection.Normalize();
        Vector3 newExitDirection = newExit.exitTransform.forward;
        newExitDirection.y = 0;
        newExitDirection.Normalize();

        if (currentExitDirection == Vector3.zero || newExitDirection == Vector3.zero)
        {
            Debug.LogError("Les directions des sorties ne peuvent pas être nulles sur le plan XZ.");
            return;
        }

        float angle = Vector3.SignedAngle(newExitDirection, -currentExitDirection, Vector3.up);
        Quaternion rotationDifference = Quaternion.Euler(0, angle, 0);
        newRoom.transform.rotation = rotationDifference * newRoom.transform.rotation;
        Vector3 positionOffset = currentExit.exitTransform.position - newExit.exitTransform.position;
        newRoom.transform.position += positionOffset;
    }

    bool IsOverlapping(Room newRoom)
    {
        Collider roomCollider = newRoom.roomCollider;
        if (roomCollider == null)
        {
            Debug.LogError("Le collider de la salle " + newRoom.name + " n'est pas assigné.");
            return true;
        }

        Collider[] hits = Physics.OverlapBox(
            roomCollider.bounds.center,
            roomCollider.bounds.extents * 0.80f,
            roomCollider.transform.rotation
        );

        foreach (Collider hit in hits)
        {
            if (hit.gameObject == roomCollider.gameObject) continue;
            Room existingRoom = hit.GetComponentInParent<Room>();
            if (existingRoom != null && generatedRooms.Contains(existingRoom)) return true;
        }

        return false;
    }

    void CloseOpenExits()
    {
        foreach (Room room in generatedRooms)
        {
            foreach (ExitPoint exit in room.exitPoints)
            {
                if (exit.isConnected) continue;
                Room endRoom = Instantiate(endRoomPrefab);
                ExitPoint endExit = FindAvailableExit(endRoom);
                if (endExit != null)
                {
                    if (endRoom.roomCollider != null) endRoom.roomCollider.enabled = false;
                    ConnectRooms(room, exit, endRoom, endExit);
                    if (endRoom.roomCollider != null) endRoom.roomCollider.enabled = true;

                    if (!IsOverlapping(endRoom))
                    {
                        exit.isConnected = true;
                        endExit.isConnected = true;
                        room.connectedRooms.Add(endRoom);
                        endRoom.connectedRooms.Add(room);
                    }
                    else Destroy(endRoom.gameObject);
                }
                else Destroy(endRoom.gameObject);
            }
        }
    }

    ExitPoint FindAvailableExit(Room room)
    {
        List<ExitPoint> exits = new List<ExitPoint>(room.exitPoints);
        ShuffleExits(exits);
        foreach (ExitPoint exit in exits)
        {
            if (!exit.isConnected) return exit;
        }
        return null;
    }
}