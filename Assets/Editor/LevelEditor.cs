using UnityEngine;
using System.Collections;
using UnityEditor;


[CustomEditor(typeof(Level), true)]
public class LevelEditor : Editor
{

	Level level;

	public void OnEnable()
	{
		level = (Level) target;
	}

	public void OnDisable()
	{
	}

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		// if(level.numberOfColumns > 0 && level.numberOfRows > 0) {
		// 	EditorGUI.BeginDisabledGroup(serializedObject.isEditingMultipleObjects);
		// 	for(int i = 0; i < level.numberOfColumns; i++) {
		// 		EditorGUI.Toggle(new Rect(0 + i,5, 15+ i, 20),false);
		// 	}
		// 	EditorGUI.EndDisabledGroup();
		// }
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