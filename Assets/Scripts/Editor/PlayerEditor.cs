using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Player))]
public class PlayerEditor : Editor
{
    private static Vector3 stationPosition = new Vector3(82, 30.07f, -21);

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        DrawDefaultInspector();

        Player myTarget = (Player)target;
        if (GUILayout.Button("Reset Kaede to Train Station"))
        {
            myTarget.transform.position = stationPosition;
        }
    }

    [MenuItem("Atama/Reset Kaede to Train Station")]
    static void ResetPosition()
    {
        var player = FindObjectOfType<Player>();
        player.transform.position = stationPosition;
    }

    [MenuItem("Atama/Set Game Mode/Normal")]
    static void SetNormal()
    {
        PlayerPrefs.SetString("GameMode", "Normal");
    }

    [MenuItem("Atama/Set Game Mode/Story")]
    static void SetStory()
    {
        PlayerPrefs.SetString("GameMode", "Story");
    }

    [MenuItem("Atama/Set Game Mode/Hardcore")]
    static void SetHardcore()
    {
        PlayerPrefs.SetString("GameMode", "Hardcore");
    }

    [MenuItem("Atama/Set Game Mode/Dev Commentary")]
    static void SetDevCommentary()
    {
        PlayerPrefs.SetString("GameMode", "DevCommentary");
    }
}
