using UnityEditor;

namespace AudioAsset
{
	[CustomEditor(typeof(AudioAssetGroup)), CanEditMultipleObjects]
	public class AudioAssetGroupEditor : AudioAssetPlayableEditor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			DrawHeaderProperties();

			DrawDefaultPropertyField("selectionType", "Selection Type");
			DrawDefaultPropertyField("assets", "Assets");

			DrawFooterProperties();
			
			serializedObject.ApplyModifiedProperties();
		}
	}
}