using System;
using TMPro;
using UnityEngine;
using UnityEditor;

public enum GameArea
{
    Alleyway,
    Courtyard,
    Residential,
    OutsideManor,
    InsideManor,
    UnderManor,
    OldTown,
    Forest
};

public class ReadOnlyAttribute : PropertyAttribute
{
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label);
        GUI.enabled = true;
    }
}
#endif

[CreateAssetMenu(fileName = "New Journal Entry", menuName = "Journal Entry")]
public class JournalEntry: ScriptableObject
{
    [ReadOnly] public string internalId;
    public string entryName;
    [TextArea] public string entryText;
    public GameArea area;
    public int areaOrder;
    public TMP_FontAsset font;
    public bool useDefaultFont = true;
    public Sprite background;
    public bool useDefaultBackground = true;

    private void OnValidate()
    {
        #if UNITY_EDITOR
        if (string.IsNullOrEmpty(internalId))
        {
            Debug.Log("Assigning new internal ID to " + entryName + " " + internalId);
            internalId = Guid.NewGuid().ToString();
            EditorUtility.SetDirty(this);
        }
        #endif
    }

    public string GetGameAreaName()
    {
        switch (area)
        {
            case GameArea.Alleyway:
                return "Factory District";
            case GameArea.Courtyard:
                return "Shantytown Courtyard";
            case GameArea.Residential:
                return "Residential District";
            case GameArea.InsideManor:
                return "Inside the Manor";
            case GameArea.OutsideManor:
                return "Outside the Manor";
            case GameArea.UnderManor:
                return "Beneath the Manor";
            case GameArea.OldTown:
                return "Old Town";        
            case GameArea.Forest:
                return "The Forest";
        }

        return "Unknown";
    }
}
