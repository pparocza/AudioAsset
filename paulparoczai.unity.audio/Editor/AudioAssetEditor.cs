using UnityEditor;
using UnityEngine;

namespace AudioAsset
{
	public abstract class AudioAssetEditor : Editor
	{
		protected void DrawScriptProperty()
		{
			GUI.enabled = false;
			EditorGUILayout.ObjectField
			(
				"Script", 
				MonoScript.FromScriptableObject((ScriptableObject)target), GetType(), false
			);
			GUI.enabled = true;
		}
		
		protected void DrawDefaultPropertyField(string propertyName, string label = "")
		{
			EditorGUILayout.PropertyField
				(serializedObject.FindProperty(propertyName), new GUIContent(label));
		}

		protected void DrawHorizontalLine()
		{
			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
		}
	}
}