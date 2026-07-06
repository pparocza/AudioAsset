using UnityEditor;
using System.Collections.Generic;

namespace AudioAsset
{
	[CustomEditor(typeof(AudioAssetSwitchGroup)), CanEditMultipleObjects]
	public class AudioAssetSwitchGroupEditor : AudioAssetControlEditor
	{
		private AudioAssetSwitchGroup assetSwitchGroupTarget;
		
		private void OnEnable()
		{
			assetSwitchGroupTarget = (AudioAssetSwitchGroup) target;
		}
		
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			DrawDefaultProperties();
			DrawCurrentSelection();
			DrawDefaultPropertyField("options", "Options");

			serializedObject.ApplyModifiedProperties();
		}

		private void DrawCurrentSelection()
		{
			List<string> switchGroupOptions = assetSwitchGroupTarget.Options;

			SerializedProperty currentSelectionIndex = serializedObject.FindProperty("currentOptionIndex");
			
			currentSelectionIndex.intValue = EditorGUILayout.Popup
			(
				"Current Option", currentSelectionIndex.intValue, switchGroupOptions.ToArray()
			);
		}
	}
}