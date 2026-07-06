using UnityEditor;
using System.Collections.Generic;

namespace AudioAsset
{
	[CustomEditor(typeof(AudioAssetStateGroup)), CanEditMultipleObjects]
	public class AudioAssetStateGroupEditor : AudioAssetControlEditor
	{
		private AudioAssetStateGroup assetStateGroupTarget;
		
		private void OnEnable()
		{
			assetStateGroupTarget = (AudioAssetStateGroup) target;
		}
		
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			DrawDefaultProperties();
			DrawCurrentState();
			DrawDefaultPropertyField("defaultTransitionTime", "Default Transition Time");
			DrawDefaultPropertyField("states", "States");
			DrawDefaultPropertyField("transitions", "Transitions");

			serializedObject.ApplyModifiedProperties();
		}

		private void DrawCurrentState()
		{
			List<string> switchGroupOptions = assetStateGroupTarget.States;

			SerializedProperty currentStateIndex = serializedObject.FindProperty("currentStateIndex");
			
			currentStateIndex.intValue = EditorGUILayout.Popup
			(
				"Current State", currentStateIndex.intValue, switchGroupOptions.ToArray()
			);
		}
	}
}