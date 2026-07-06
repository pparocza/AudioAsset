using UnityEditor;

namespace AudioAsset
{
	[CustomEditor(typeof(AudioAssetClip)), CanEditMultipleObjects]
	public class AudioAssetClipEditor : AudioAssetPlayableEditor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			DrawHeaderProperties();
			
			DrawDefaultPropertyField("audioClip", "Audio Clip");
			
			DrawFooterProperties();

			serializedObject.ApplyModifiedProperties();
		}
	}
}