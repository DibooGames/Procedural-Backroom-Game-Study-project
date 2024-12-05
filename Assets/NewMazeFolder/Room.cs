using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public string roomType; // Type de la salle (chaîne de caractères)
    public List<ExitPoint> exitPoints; // Liste des sorties de la salle
    public List<Room> connectedRooms; // Salles adjacentes

    [Tooltip("Collider utilisé pour la détection des collisions lors de la génération du donjon")]
    public Collider roomCollider; // Collider de la salle

    void Awake()
    {
        connectedRooms = new List<Room>();
    }
}

[System.Serializable]
public class ExitPoint
{
    public Transform exitTransform; // Position et orientation de la sortie
    public bool isConnected = false; // Statut de connexion
}
