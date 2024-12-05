/*
                  /\
                 /  \
                /    \
               /      \
              /   ██   \
             /    ██    \
            /     ██     \
           /      ██      \
          /       ██       \
         /        ██        \
        /         ██         \
       /                      \
      /                        \
     /            ██            \
    /                            \
   /______________________________\


Ce script n'est pas de moi je l'ai trouvé sur github et je l'ai adapté pour mon projet.

*/




using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(GenerationRules))]
public class GenerationRulesEditor : Editor
{
    private GenerationRules generationRules;
    private SerializedProperty roomRulesProperty;

    private List<string> roomTypeOptions = new List<string>();

    void OnEnable()
    {
        generationRules = (GenerationRules)target;
        roomRulesProperty = serializedObject.FindProperty("roomRules");

        UpdateRoomTypeOptions();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

     
        generationRules.dungeonGenerator = (DungeonGenerator)EditorGUILayout.ObjectField("Dungeon Generator", generationRules.dungeonGenerator, typeof(DungeonGenerator), true);

        
        if (GUILayout.Button("Mettre à jour les types de salles"))
        {
            UpdateRoomTypeOptions();
        }

       
        EditorGUILayout.LabelField("Room Rules", EditorStyles.boldLabel);

        if (roomTypeOptions.Count == 0)
        {
            EditorGUILayout.HelpBox("Aucun type de salle disponible. Assurez-vous que 'Dungeon Generator' est assigné et que 'Available Rooms' contient des salles avec des 'Room Type' définis.", MessageType.Warning);
        }
        else
        {
            for (int i = 0; i < generationRules.roomRules.Count; i++)
            {
                RoomRule rule = generationRules.roomRules[i];

                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();
              
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    generationRules.roomRules.RemoveAt(i);
                    break;
                }

                int currentRoomTypeIndex = roomTypeOptions.IndexOf(rule.roomType);
                if (currentRoomTypeIndex == -1) currentRoomTypeIndex = 0;

                int selectedRoomTypeIndex = EditorGUILayout.Popup("Room Type", currentRoomTypeIndex, roomTypeOptions.ToArray());
                rule.roomType = roomTypeOptions[selectedRoomTypeIndex];
                EditorGUILayout.EndHorizontal();

               
                EditorGUILayout.LabelField("Forbidden Room Types", EditorStyles.boldLabel);

                for (int j = 0; j < rule.forbiddenRoomTypes.Count; j++)
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        rule.forbiddenRoomTypes.RemoveAt(j);
                        break;
                    }

                    int forbiddenRoomTypeIndex = roomTypeOptions.IndexOf(rule.forbiddenRoomTypes[j]);
                    if (forbiddenRoomTypeIndex == -1) forbiddenRoomTypeIndex = 0;

                    int selectedForbiddenRoomTypeIndex = EditorGUILayout.Popup(forbiddenRoomTypeIndex, roomTypeOptions.ToArray());
                    rule.forbiddenRoomTypes[j] = roomTypeOptions[selectedForbiddenRoomTypeIndex];
                    EditorGUILayout.EndHorizontal();
                }

               
                if (GUILayout.Button("Ajouter un type interdit"))
                {
                    rule.forbiddenRoomTypes.Add(roomTypeOptions[0]);
                }

                EditorGUILayout.EndVertical();
            }

           
            if (GUILayout.Button("Ajouter une règle"))
            {
                generationRules.roomRules.Add(new RoomRule { roomType = roomTypeOptions[0], forbiddenRoomTypes = new List<string>() });
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void UpdateRoomTypeOptions()
    {
        roomTypeOptions.Clear();

        if (generationRules.dungeonGenerator != null)
        {
            foreach (Room room in generationRules.dungeonGenerator.availableRooms)
            {
                if (!string.IsNullOrEmpty(room.roomType) && !roomTypeOptions.Contains(room.roomType))
                {
                    roomTypeOptions.Add(room.roomType);
                }
            }
        }
    }
}