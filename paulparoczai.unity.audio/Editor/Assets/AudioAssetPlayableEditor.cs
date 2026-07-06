using UnityEditor;
using UnityEngine;

namespace AudioAsset
{
	public abstract class AudioAssetPlayableEditor : AudioAssetEditor
	{
		#region Property String Constants
		private const string AudioSourcePrefabPropertyName = "audioSourcePrefab";
		private const string AudioSourcePrefabPropertyLabel = "AudioSource Prefab";
		
		private const string PlaybackParametersPropertyName = "playbackParameters";
		private const string PlaybackParametersPropertyLabel = "Playback Parameters";
		
		private const string StatePropertyName = "states";
		private const string StatePropertyLabel = "States";
		
		private const string VoiceLimitPropertyName = "voiceLimit";
		private const string VoiceLimitPropertyLabel = "Voice Limit";

		private const string PreviewButtonText = "Preview";
		private const string StopButtonText = "Stop";
		#endregion
		
		private AudioAssetPlayable assetPlayableTarget;
		
		private void OnEnable()
		{
			assetPlayableTarget = (AudioAssetPlayable) target;
		}

		#region Default Properties
		protected void DrawHeaderProperties()
		{
			DrawScriptProperty();
			DrawHorizontalLine();
			
			DrawPreviewButtons();
			DrawHorizontalLine();
		}

		protected void DrawFooterProperties()
		{
			DrawHorizontalLine();
			
			DrawDefaultPropertyField(AudioSourcePrefabPropertyName, AudioSourcePrefabPropertyLabel);
			DrawHorizontalLine();
			
			DrawDefaultPropertyField(PlaybackParametersPropertyName, PlaybackParametersPropertyLabel);
			DrawHorizontalLine();
			
			DrawStates();
			DrawHorizontalLine();
			
			DrawVoiceLimit();
			DrawHorizontalLine();
		}

		private void DrawPreviewButtons()
		{
			EditorGUILayout.BeginHorizontal();
			
			if (GUILayout.Button(PreviewButtonText))
			{
				assetPlayableTarget = (AudioAssetPlayable) target;
				assetPlayableTarget.Preview();
			}

			if (GUILayout.Button(StopButtonText))
			{
				assetPlayableTarget = (AudioAssetPlayable) target;
				assetPlayableTarget.StopInEditor();
			}
			
			EditorGUILayout.EndHorizontal();
		}

		private void DrawStates()
		{
			assetPlayableTarget.RefreshStates();
			DrawDefaultPropertyField(StatePropertyName, StatePropertyLabel);
		}

		private void DrawVoiceLimit()
		{
			DrawDefaultPropertyField(VoiceLimitPropertyName, VoiceLimitPropertyLabel);
		}
		#endregion
	}
}