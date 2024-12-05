using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    public int maxRooms = 10; // Nombre maximum de salles à générer
    public Room startRoomPrefab; // Prefab de la salle de départ
    public Room endRoomPrefab; // Prefab de la salle de fin
    public List<Room> availableRooms; // Liste des prefabs de salles disponibles
    public GenerationRules generationRules; // Référence au script GenerationRules

    public int seed = 0; // Seed pour la génération aléatoire (0 pour une seed aléatoire)

    private int currentRoomCount;
    private List<Room> generatedRooms;

    void Start()
    {
        InitializeRandom();
        GenerateDungeon();
    }

    void InitializeRandom()
    {
        if (seed != 0)
        {
            Random.InitState(seed);
        }
        else
        {
            Random.InitState(System.Environment.TickCount);
        }
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

            // Mélanger les sorties pour introduire de l'aléatoire
            ShuffleExits(currentRoom.exitPoints);

            foreach (ExitPoint exit in currentRoom.exitPoints)
            {
                if (!exit.isConnected)
                {
                    bool exitConnected = false;

                    // Obtenir les salles possibles
                    List<Room> possibleRooms = GetPossibleRooms(currentRoom.roomType);

                    // Mélanger les salles possibles pour l'aléatoire
                    ShuffleRooms(possibleRooms);

                    foreach (Room nextRoomPrefab in possibleRooms)
                    {
                        // Créer une nouvelle salle à une position temporaire
                        Vector3 tempPosition = new Vector3(10000, 10000, 10000);
                        Room newRoom = Instantiate(nextRoomPrefab, tempPosition, Quaternion.identity);

                        // Désactiver le collider de la nouvelle salle pendant le positionnement
                        if (newRoom.roomCollider != null)
                        {
                            newRoom.roomCollider.enabled = false;
                        }

                        List<ExitPoint> newRoomExits = new List<ExitPoint>(newRoom.exitPoints);
                        ShuffleExits(newRoomExits);

                        bool roomPlaced = false;

                        foreach (ExitPoint newExit in newRoomExits)
                        {
                            if (!newExit.isConnected)
                            {
                                // Tenter de connecter les salles
                                ConnectRooms(currentRoom, exit, newRoom, newExit);

                                // Réactiver le collider de la nouvelle salle
                                if (newRoom.roomCollider != null)
                                {
                                    newRoom.roomCollider.enabled = true;
                                }

                                // Vérifier les collisions
                                if (!IsOverlapping(newRoom))
                                {
                                    // Pas de collision, on peut placer la salle
                                    generatedRooms.Add(newRoom);
                                    roomsToProcess.Enqueue(newRoom);
                                    currentRoomCount++;

                                    // Marquer les sorties comme connectées
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
                                    // Collision détectée, réinitialiser la position et la rotation de la salle
                                    newRoom.transform.position = tempPosition;
                                    newRoom.transform.rotation = Quaternion.identity;

                                    // Désactiver le collider de la nouvelle salle pour réessayer
                                    if (newRoom.roomCollider != null)
                                    {
                                        newRoom.roomCollider.enabled = false;
                                    }
                                }
                            }
                        }

                        if (roomPlaced)
                        {
                            break; // On a placé une salle, on passe à la prochaine sortie
                        }
                        else
                        {
                            // Détruire la salle si elle ne peut pas être placée
                            Destroy(newRoom.gameObject);
                        }
                    }

                    if (!exitConnected)
                    {
                        // Aucune salle n'a pu être placée sur cette sortie
                        // On peut choisir de marquer la sortie comme bloquée ou continuer
                        // Ici, on continue simplement avec la prochaine sortie
                        continue;
                    }

                    if (currentRoomCount >= maxRooms)
                        break;
                }
            }
        }

        // Fermer les sorties ouvertes
        CloseOpenExits();
    }

    List<Room> GetPossibleRooms(string currentRoomType)
    {
        List<Room> possibleRooms = new List<Room>();

        foreach (Room room in availableRooms)
        {
            if (generationRules.CanConnect(currentRoomType, room.roomType))
            {
                possibleRooms.Add(room);
            }
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
        // Obtenir les directions des sorties et les projeter sur le plan XZ
        Vector3 currentExitDirection = currentExit.exitTransform.forward;
        currentExitDirection.y = 0;
        currentExitDirection.Normalize();

        Vector3 newExitDirection = newExit.exitTransform.forward;
        newExitDirection.y = 0;
        newExitDirection.Normalize();

        // Vérifier que les directions ne sont pas nulles
        if (currentExitDirection == Vector3.zero || newExitDirection == Vector3.zero)
        {
            Debug.LogError("Les directions des sorties ne peuvent pas être nulles sur le plan XZ.");
            return;
        }

        // Calculer l'angle de rotation autour de l'axe Y pour aligner les sorties
        float angle = Vector3.SignedAngle(newExitDirection, -currentExitDirection, Vector3.up);
        Quaternion rotationDifference = Quaternion.Euler(0, angle, 0);

        // Appliquer la rotation à la nouvelle salle
        newRoom.transform.rotation = rotationDifference * newRoom.transform.rotation;

        // Mettre à jour la position de la nouvelle salle pour aligner les sorties
        Vector3 positionOffset = currentExit.exitTransform.position - newExit.exitTransform.position;
        newRoom.transform.position += positionOffset;
    }

    bool IsOverlapping(Room newRoom)
    {
        Collider roomCollider = newRoom.roomCollider;

        if (roomCollider == null)
        {
            Debug.LogError("Le collider de la salle " + newRoom.name + " n'est pas assigné.");
            return true; // Considérer qu'il y a une collision si le collider n'est pas défini
        }

        Collider[] hits = Physics.OverlapBox(
            roomCollider.bounds.center,
            roomCollider.bounds.extents * 0.80f,
            roomCollider.transform.rotation
        );

        foreach (Collider hit in hits)
        {
            if (hit.gameObject == roomCollider.gameObject)
            {
                continue; // Ignorer le collider de la nouvelle salle elle-même
            }

            // Vérifier que le collider appartient à une salle déjà placée
            Room existingRoom = hit.GetComponentInParent<Room>();
            if (existingRoom != null && generatedRooms.Contains(existingRoom))
            {
                return true; // Collision détectée
            }
        }

        return false; // Pas de collision
    }

    void CloseOpenExits()
    {
        foreach (Room room in generatedRooms)
        {
            foreach (ExitPoint exit in room.exitPoints)
            {
                if (!exit.isConnected)
                {
                    Room endRoom = Instantiate(endRoomPrefab);
                    ExitPoint endExit = FindAvailableExit(endRoom);

                    if (endExit != null)
                    {
                        // Désactiver le collider de la salle de fin
                        if (endRoom.roomCollider != null)
                        {
                            endRoom.roomCollider.enabled = false;
                        }

                        ConnectRooms(room, exit, endRoom, endExit);

                        // Réactiver le collider de la salle de fin
                        if (endRoom.roomCollider != null)
                        {
                            endRoom.roomCollider.enabled = true;
                        }

                        // Vérifier les collisions
                        if (!IsOverlapping(endRoom))
                        {
                            // Marquer les sorties comme connectées
                            exit.isConnected = true;
                            endExit.isConnected = true;

                            room.connectedRooms.Add(endRoom);
                            endRoom.connectedRooms.Add(room);

                            // Pas besoin d'ajouter endRoom à generatedRooms car elle ne sera pas étendue
                        }
                        else
                        {
                            // Collision détectée, détruire la salle de fin
                            Destroy(endRoom.gameObject);
                        }
                    }
                    else
                    {
                        Destroy(endRoom.gameObject);
                    }
                }
            }
        }
    }

    ExitPoint FindAvailableExit(Room room)
    {
        List<ExitPoint> exits = new List<ExitPoint>(room.exitPoints);
        ShuffleExits(exits);

        foreach (ExitPoint exit in exits)
        {
            if (!exit.isConnected)
            {
                return exit;
            }
        }
        return null;
    }
}
