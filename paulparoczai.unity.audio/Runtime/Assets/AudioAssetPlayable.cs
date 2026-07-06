using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
#endif

namespace AudioAsset
{
	public abstract class AudioAssetPlayable : ScriptableObject, IAudioAssetPlayable
	{
		public struct PlayingSourceID
		{
			public int CallerTransformHashCode;
			public int SourceTransformHashCode;
		}

		public delegate void OnFinishedPlayingDelegate(PlayingSourceID sourceID);
		private static readonly OnFinishedPlayingDelegate OnFinishedPlaying = OnSourceFinishedPlaying;
		private static readonly OnFinishedPlayingDelegate OnPreviewFinishedPlaying = OnPreviewSourceFinishedPlaying;

		private static readonly Dictionary<PlayingSourceID, AudioAssetVoice> PlayingSources = new();
		private static readonly Dictionary<PlayingSourceID, AudioAssetVoice> PreviewSources = new();

		[SerializeField] 
		protected GameObject audioSourcePrefab;
		public GameObject AudioSourcePrefab => audioSourcePrefab;

		[SerializeField]
		protected AudioAssetPlaybackParameters playbackParameters;
		public AudioAssetPlaybackParameters PlaybackParameters => playbackParameters;

		[SerializeField]
		protected List<AudioAssetStateData> states = new();
		public List<AudioAssetStateData> States => states;

		[SerializeField]
		protected int voiceLimit;
		public int VoiceLimit => voiceLimit;

		private void OnEnable()
		{
			RefreshStates();
		}

		#region Playback
		public void Play(Transform parent)
		{
			GameObject audioSourceObject = new()
			{
				transform = { parent = parent },
				name = $"AudioAssetSource: {name}"
			};

			audioSourceObject.transform.position = parent.position;
			audioSourceObject.transform.rotation = parent.rotation;
			
			AudioAssetVoice audioAssetVoice = audioSourceObject.AddComponent<AudioAssetVoice>();

			PlayingSourceID sourceID = new()
			{
				CallerTransformHashCode = parent.GetHashCode(),
				SourceTransformHashCode = audioSourceObject.transform.GetHashCode()
			};
			
			audioAssetVoice.BindRTPCs(GenerateRTPCBindings());
			audioAssetVoice.InitializeState(states);
			
			audioAssetVoice.Play(this, GeneratePlaybackData(), audioSourceObject.transform, sourceID, OnFinishedPlaying);

			PlayingSources.Add(sourceID, audioAssetVoice);
		}

		public void Stop(Transform parent)
		{
			List<PlayingSourceID> playingSourcesKeys = PlayingSources.Keys.ToList();
			
			foreach (PlayingSourceID sourceID in playingSourcesKeys)
			{
				if (sourceID.CallerTransformHashCode != parent.GetHashCode())
				{
					continue;
				}

				AudioAssetVoice playingAssetVoice = PlayingSources[sourceID];
				playingAssetVoice.Stop();
				PlayingSources.Remove(sourceID);
			}
		}

		private static void OnSourceFinishedPlaying(PlayingSourceID sourceID)
		{
			PlayingSources.Remove(sourceID);
		}
		
		private static void OnPreviewSourceFinishedPlaying(PlayingSourceID sourceID)
		{
			PreviewSources.Remove(sourceID);
		}

		public abstract AudioAssetPlaybackData GeneratePlaybackData();
		#endregion

		#region RTPC
		private List<AudioAssetRTPCBinding> GenerateRTPCBindings()
		{
			List<AudioAssetRTPCBinding> rtpcBindings = new();

			if (CreateRTPCBinding(playbackParameters.PitchParameter) != null)
			{
				rtpcBindings.Add(CreateRTPCBinding(playbackParameters.PitchParameter));
			}
			
			if (CreateRTPCBinding(playbackParameters.VolumeParameter) != null)
			{
				rtpcBindings.Add(CreateRTPCBinding(playbackParameters.VolumeParameter));
			}
			
			if (CreateRTPCBinding(playbackParameters.HighpassParameter) != null)
			{
				rtpcBindings.Add(CreateRTPCBinding(playbackParameters.HighpassParameter));
			}
			
			if (CreateRTPCBinding(playbackParameters.LowpassParameter) != null)
			{
				rtpcBindings.Add(CreateRTPCBinding(playbackParameters.LowpassParameter));
			}

			return rtpcBindings;
		}

		private AudioAssetRTPCBinding CreateRTPCBinding(AudioAssetParameter parameter)
		{
			if (!parameter.Rtpc.Enable || parameter.Rtpc.RTPC == null)
			{
				return null;
			}
			
			return new AudioAssetRTPCBinding()
			{
				ParameterType = parameter.ParameterType,
				rTPCData = parameter.Rtpc
			};
		}
		#endregion
		
		#region States
		public void RefreshStates()
		{
			foreach (AudioAssetStateData stateData in states)
			{
				if (stateData.StateGroup == null)
				{
					stateData.StateSettings = new List<AudioAssetStateSetting>();
					continue;
				}

				for (int stateIndex = 0; stateIndex < stateData.StateGroup.States.Count; stateIndex++)
				{
					string state = stateData.StateGroup.States[stateIndex];

					if (stateIndex > stateData.StateSettings.Count - 1)
					{
						stateData.StateSettings.Add(new AudioAssetStateSetting()
						{
							State = state
						});
					}
					else if (stateData.StateSettings[stateIndex] == null)
					{
						stateData.StateSettings.Add(new AudioAssetStateSetting()
						{
							State = state
						});
					}
					else
					{
						stateData.StateSettings[stateIndex].State = state;
					}
				}
			}
		}
		#endregion
		
		#region Editor Preview
#if UNITY_EDITOR
		public void Preview()
		{
			PlayInEditor(GeneratePlaybackData());
		}

		public void PlayInEditor(AudioAssetPlaybackData assetPlaybackData)
		{
			GameObject audioSourceObject = new()
			{
				hideFlags = HideFlags.DontSave,
				name = "AudioAssetAsset Preview"
			};
			
			PlayingSourceID sourceID = new()
			{
				CallerTransformHashCode = 0,
				SourceTransformHashCode = audioSourceObject.transform.GetHashCode()
			};
			
			AudioAssetVoice previewAssetVoice = audioSourceObject.AddComponent<AudioAssetVoice>();

			previewAssetVoice.PlayEditorPreview
				(this, assetPlaybackData, audioSourceObject.transform, sourceID, OnPreviewFinishedPlaying);
			
			PreviewSources.Add(sourceID, previewAssetVoice);
		}

		public void StopInEditor()
		{
			foreach (AudioAssetVoice previewSource in PreviewSources.Values)
			{
				previewSource.Stop(true);
			}
			
			PreviewSources.Clear();
		}
#endif
		#endregion
	}
}