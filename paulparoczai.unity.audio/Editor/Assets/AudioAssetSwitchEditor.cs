using UnityEditor;

namespace AudioAsset
{
	[CustomEditor(typeof(AudioAssetSwitch)), CanEditMultipleObjects]
	public class AudioAssetSwitchEditor : AudioAssetPlayableEditor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			DrawHeaderProperties();
			
			DrawDefaultPropertyField("switchGroup", "Switch Group");
			DrawCurrentSelection();
			DrawDefaultPropertyField("switchData", "Switch Data");
			
			DrawFooterProperties();

			serializedObject.ApplyModifiedProperties();
		}

		private void DrawCurrentSelection()
		{
			AudioAssetSwitch assetSwitchTarget = (AudioAssetSwitch) target;
			AudioAssetSwitchGroup switchGroup = assetSwitchTarget.SwitchGroup;

			if (switchGroup != null)
			{
				EditorGUILayout.LabelField
					($"Current Option: {switchGroup.CurrentOptionIndex} - {switchGroup.CurrentOption}");
			}
			else
			{
				EditorGUILayout.LabelField("No Switch Group Specified.");
			}
		}
	}
}