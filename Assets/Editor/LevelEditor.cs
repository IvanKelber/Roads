using UnityEngine;
using System.Collections;
using UnityEditor;


[CustomEditor(typeof(Level), true)]
public class LevelEditor : Editor
{

	Level level;
	bool showPosition = false;

	public void OnEnable()
	{
		level = (Level) target;
	}

	public void OnDisable()
	{
	}

	public override void OnInspectorGUI()
	{

		EditorGUILayout.BeginVertical();
		SerializedProperty startingIndex = serializedObject.FindProperty("startingIndex");
        SerializedProperty endingIndex = serializedObject.FindProperty("endingIndex");
		EditorGUILayout.PropertyField(startingIndex);
        EditorGUILayout.PropertyField(endingIndex);
		EditorGUILayout.EndVertical();

		EditorGUILayout.BeginHorizontal();
		SerializedProperty numberOfRowsProp = serializedObject.FindProperty("numberOfRows");
        SerializedProperty numberOfColumnsProp = serializedObject.FindProperty("numberOfColumns");
		EditorGUILayout.PropertyField(numberOfColumnsProp);
        EditorGUILayout.PropertyField(numberOfRowsProp);
		EditorGUILayout.EndHorizontal();
		serializedObject.ApplyModifiedProperties();

		EditorGUI.BeginDisabledGroup(level.numberOfColumns == 0 || level.numberOfRows == 0);
		EditorGUILayout.LabelField("Level Matrix");
		SerializedProperty levelMatrix = serializedObject.FindProperty("levelMatrix");
		if(levelMatrix == null
				|| levelMatrix.FindPropertyRelative("numberOfColumns").intValue != level.numberOfColumns
				|| levelMatrix.FindPropertyRelative("numberOfRows").intValue != level.numberOfRows) {
			level.levelMatrix = new Level.TileMatrix(level.numberOfColumns, level.numberOfRows);
			serializedObject.ApplyModifiedProperties();
		}

		EditorGUILayout.BeginVertical("box");
		SerializedProperty rows = levelMatrix.FindPropertyRelative("rows");

		for(int j = rows.arraySize - 1; j >= 0; j--) {
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
			Rect scale = GUILayoutUtility.GetLastRect();

			EditorGUIUtility.LookLikeInspector();
			EditorGUIUtility.LookLikeControls(scale.width/2);
			SerializedProperty row = rows.GetArrayElementAtIndex(j).FindPropertyRelative("row");
			Rect horizontalRect = EditorGUILayout.BeginHorizontal("box");
			
			for(int i = 0; i < row.arraySize; i++) {
				Rect tileRect = new Rect(horizontalRect.x + 10 * i, horizontalRect.y, 40, horizontalRect.height);
				EditorGUILayout.PropertyField(row.GetArrayElementAtIndex(i));
			}

			EditorGUILayout.EndHorizontal();
		}

		EditorGUILayout.EndVertical();

			// if(levelMatrix != null)
				// EditorGUI.PropertyField(new Rect(40,40,40,40), levelMatrix.GetArrayElementAtIndex(0).GetArrayElementAtIndex(0), GUIContent.none);
			// for(int j = 0; j < level.numberOfRows; j++) {
			// 	EditorGUILayout.BeginHorizontal();
			// 	for(int i = 0; i < level.numberOfColumns; i++) {
					
			// 	}
			// 	EditorGUILayout.EndHorizontal();
			// }	
		
		EditorGUI.EndDisabledGroup();

		// if (GUILayout.Button("Preview"))
		// {
		// 	((AudioEvent) target).Play(_previewer, 1);
		// }
        // IncreasingPitchAudioEvent ip = target as IncreasingPitchAudioEvent;
        // if(ip != null) {
        //     if(GUILayout.Button("Reset")) {
        //         ip.Reset();
        //     }
        // }

	}
}