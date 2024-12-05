using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoomRule
{
    public string roomType; 
    public List<string> forbiddenRoomTypes; 
}

public class GenerationRules : MonoBehaviour
{
    public DungeonGenerator dungeonGenerator; 
    public List<RoomRule> roomRules;

    private Dictionary<string, List<string>> forbiddenConnections = new Dictionary<string, List<string>>();

    void Awake()
    {
        InitializeRules();
    }

    void InitializeRules()
    {
        foreach (RoomRule rule in roomRules)
        {
            if (!forbiddenConnections.ContainsKey(rule.roomType))
            {
                forbiddenConnections.Add(rule.roomType, new List<string>());
            }
            forbiddenConnections[rule.roomType].AddRange(rule.forbiddenRoomTypes);
        }
    }
    public bool CanConnect(string currentRoomType, string nextRoomType)
    {
        if (forbiddenConnections.ContainsKey(currentRoomType))
        {
            return !forbiddenConnections[currentRoomType].Contains(nextRoomType);
        }
        return true;
    }
}
