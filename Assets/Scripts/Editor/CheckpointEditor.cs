using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Assets.Scripts.Common;

[CustomEditor(typeof(Checkpoint))]
public class CheckpointEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        Checkpoint myTarget = (Checkpoint)target;
        if (GUILayout.Button("Set Checkpoint"))
        {
            myTarget.SaveCheckpoint();
        }

        DrawDefaultInspector();
    }

    [MenuItem("Atama/Delete All Data")]
    static void DeleteAllData()
    {
        SecureSaveFile.Instance.DeleteFileFromDisk();
        SecureSaveFile.Instance.DeleteAll();
    }
}
