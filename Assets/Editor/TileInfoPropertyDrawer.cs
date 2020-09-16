using UnityEditor;
using UnityEngine;
using TileRotation;

[CustomPropertyDrawer(typeof(TileInfo))]
public class TileInfoPropertyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        return 40;
    }
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        bool isStraight = property.FindPropertyRelative("isStraight").boolValue;
        Orientation orientation = (Orientation) property.FindPropertyRelative("startingOrientation").enumValueIndex;
        string texturePath = "Assets/Editor/Textures/" + ( isStraight ? "straight_road" : "bent_road") + "_" + orientation + ".png";
        Texture2D iconTexture = (Texture2D)AssetDatabase.LoadMainAssetAtPath(texturePath);

        var rect = new Rect(position.x, position.y, position.height, position.height);


        var e = Event.current;
        if(rect.Contains(e.mousePosition) && e.type == EventType.MouseDown && e.button == 1){
            e.Use();
            SetProperty(property, "isStraight", !isStraight);
        }
        else if (rect.Contains(e.mousePosition) && e.type == EventType.MouseDown && e.button == 0) {
            e.Use();
            Orientation nextOrientation = ((Orientation) property.FindPropertyRelative("startingOrientation").enumValueIndex).GetNextOrientation(90);
            SetProperty(property, "startingOrientation", (int) nextOrientation);
        } 
        GUI.DrawTexture(rect, iconTexture);
        // GUI.Label(new Rect(rect.x + 50, rect.y, position.width, 30), "texture: " + iconTexture);
        EditorGUI.EndProperty();
    }

    private void SetProperty(SerializedProperty property, string propertyName, bool value) {
        var propRelative = property.FindPropertyRelative(propertyName);
        propRelative.boolValue = value;
        propRelative.serializedObject.ApplyModifiedProperties();
    }

    private void SetProperty(SerializedProperty property, string propertyName, int value) {
        var propRelative = property.FindPropertyRelative(propertyName);
        propRelative.enumValueIndex = value;
        propRelative.serializedObject.ApplyModifiedProperties();
    }
}