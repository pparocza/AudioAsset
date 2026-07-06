using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
#endif

namespace AudioAsset
{
	[AddComponentMenu("")]
	public class AudioAssetVoice : MonoBehaviour
	{
		private AudioSource audioSource;
		private AudioHighPassFilter highPassFilter;
		private AudioLowPassFilter lowPassFilter;
		
		private Transform parentTransform;

		private AudioAssetPlaybackData assetPlaybackData;
		
		private AudioAssetPlayable.OnFinishedPlayingDelegate onFinishedPlayingDelegate;
		private AudioAssetPlayable.PlayingSourceID sourceID;

		private List<AudioAssetRTPCBinding> rtpcBindings = new ();
		private List<AudioAssetStateData> stateData;

		private Coroutine stateTransitionCoroutine;
		
		private Coroutine delayCoroutine;
		private EditorCoroutine delayCoroutineEditor;
		
		private Coroutine fadeInCoroutine;
		private EditorCoroutine fadeInCoroutineEditor;
		
		private Coroutine fadeOutCoroutine;
		private EditorCoroutine fadeOutCoroutineEditor;

		private bool stopPending;
		private bool stopPrepared;

		#region Lifecycle
		private void Update()
		{
			UpdateRTPCs();
		}
		#endregion

		#region Playback
		public void Play
		(
			AudioAssetPlayable asset, AudioAssetPlaybackData data, Transform parent, 
			AudioAssetPlayable.PlayingSourceID playingSourceID, 
			AudioAssetPlayable.OnFinishedPlayingDelegate onFinishedPlayingCallback,
			bool editorPreview = false
		)
		{
			audioSource = SpawnAudioSource(asset, data, parent);

			if (editorPreview)
			{
				audioSource.spatialize = false;
				audioSource.spatialBlend = 0;
			}

			onFinishedPlayingDelegate = onFinishedPlayingCallback;
			sourceID = playingSourceID;
			
			HandleStart(data, editorPreview);
		}

		private void HandleStart(AudioAssetPlaybackData data, bool editorPreview)
		{
			if (PlayImmediate(data))
			{
				audioSource.Play();
				PrepareStop(editorPreview);
			}
			else if(data.Delay > 0)
			{
				HandleDelay(data, editorPreview);
			}
			else if(data.FadeInTime > 0)
			{
				HandleFadeIn(data, editorPreview);
			}
		}

		private void HandleDelay(AudioAssetPlaybackData data, bool editorPreview)
		{
			if (!editorPreview)
			{
				delayCoroutine = StartCoroutine(DelayCoroutine(data, false));
			}
			else
			{
				delayCoroutineEditor = EditorCoroutineUtility.StartCoroutine
					(DelayCoroutine(data, true), audioSource);
			}
		}

		private void HandleFadeIn(AudioAssetPlaybackData data, bool editorPreview)
		{
			if (audioSource == null)
			{
				return;
			}
			
			audioSource.volume = 0;
			audioSource.Play();
			
			if (!editorPreview)
			{
				fadeInCoroutine = StartCoroutine
				(
					FadeCoroutine(data.FadeInTime, AudioAssetFadeType.FadeIn)
				);
			}
			else
			{
				fadeInCoroutineEditor = EditorCoroutineUtility.StartCoroutine
				(
					FadeCoroutineEditorPreview(data.FadeInTime, AudioAssetFadeType.FadeIn), 
					audioSource
				);
			}
			
			PrepareStop(editorPreview);
		}

		public void Stop(bool editorPreview = false)
		{
			if (StopImmediate(assetPlaybackData))
			{
				audioSource.Stop();
			}
			else
			{
				HandleFadeOut(editorPreview);
			}
			
			StartCoroutine(VoiceStopCoroutine());
		}
		
		private void PrepareStop(bool editorPreview)
		{
			if (audioSource.loop)
			{
				return;
			}
			
			if (assetPlaybackData.FadeOutTime > 0)
			{
				// TODO: AudioSource.time accounts for pitch shift, but fade time does not - should fade out take into consideration the shortened time such that the fade out time is honored no matter what?
				float fadeDelay = Mathf.Max(0, assetPlaybackData.AudioClip.length - assetPlaybackData.FadeOutTime);
				HandleFadeOut(editorPreview, fadeDelay);
			}

			if (!editorPreview)
			{
				StartCoroutine(VoiceStopCoroutine());
			}
			else
			{
				EditorCoroutineUtility.StartCoroutine(VoiceStopCoroutine(), audioSource);
			}
		}

		private void HandleFadeOut(bool editorPreview, float delay = 0)
		{
			if (audioSource == null)
			{
				return;
			}
			
			if (!editorPreview)
			{
				if (fadeOutCoroutine != null)
				{
					StopCoroutine(fadeOutCoroutine);
				}
				
				fadeOutCoroutine = StartCoroutine
				(
					FadeCoroutine(assetPlaybackData.FadeOutTime, AudioAssetFadeType.FadeOut, delay)
				);
			}
			else
			{
				if (fadeOutCoroutineEditor != null)
				{
					EditorCoroutineUtility.StopCoroutine(fadeOutCoroutineEditor);
				}
				
				fadeOutCoroutineEditor = EditorCoroutineUtility.StartCoroutine
				(
					FadeCoroutineEditorPreview(assetPlaybackData.FadeOutTime, AudioAssetFadeType.FadeOut, delay), 
					audioSource
				);
			}
		}
		
		private IEnumerator VoiceStopCoroutine()
		{
			if (stopPending)
			{
				yield break;
			}
			
			stopPending = true;

			yield return new WaitUntil(() => !audioSource.isPlaying);
			
			onFinishedPlayingDelegate.Invoke(sourceID);
			UnsubscribeStates();
			DestroyImmediate(gameObject);
		}
		#endregion

		#region Audio Source Handling
		private AudioSource SpawnAudioSource(AudioAssetPlayable asset, AudioAssetPlaybackData data, Transform parent)
		{
			return asset.AudioSourcePrefab != null ? 
				SpawnAudioSourceFromPrefab(asset, data, parent) : SpawnDefaultAudioSource(data, parent);
		}

		private AudioSource SpawnAudioSourceFromPrefab(AudioAssetPlayable asset, AudioAssetPlaybackData data, Transform parent)
		{
			GameObject audioSourceGameObject = parent != null ? 
				Instantiate(asset.AudioSourcePrefab, parent) : Instantiate(asset.AudioSourcePrefab);

			AudioSource prefabSource = audioSourceGameObject.GetComponent<AudioSource>();

			if (prefabSource == null)
			{
				Debug.LogWarning
					($"Audio Source prefab of {asset.name} does not have an Audio Source component. Spawning Default Audio Source");
				
				prefabSource = SpawnDefaultAudioSource(data, parent);
			}

			SetAudioSourceParameters(prefabSource, data);

			return prefabSource;
		}

		private AudioSource SpawnDefaultAudioSource(AudioAssetPlaybackData data, Transform parent)
		{
			AudioSource defaultSource = parent.gameObject.AddComponent<AudioSource>();

			defaultSource.spatialize = false;
			defaultSource.spatialBlend = 0;
			
			SetAudioSourceParameters(defaultSource, data);
			
			return defaultSource;
		}

		private void SetAudioSourceParameters(AudioSource source, AudioAssetPlaybackData data)
		{
			assetPlaybackData = data;
			
			HandleFilters(source, assetPlaybackData);
				
			source.clip = assetPlaybackData.AudioClip;
			source.volume = AudioAssetUtils.DecibelsToAmplitude(assetPlaybackData.Volume);
			source.pitch = AudioAssetUtils.CentsToRatio((int) assetPlaybackData.Pitch);

			source.loop = assetPlaybackData.Loop;
		}
		
		private void HandleFilters(AudioSource source, AudioAssetPlaybackData assetPlaybackData)
		{
			GameObject audioSourceGameObject = source.gameObject;
			
			if (assetPlaybackData.HighpassAmount > 0)
			{
				CreateHighpassFilter(audioSourceGameObject);
				highPassFilter.cutoffFrequency = 
					AudioAssetUtils.FilterAmountToCutoffFrequency(assetPlaybackData.HighpassAmount);
			}

			if (assetPlaybackData.LowpassAmount > 0)
			{
				CreateHighpassFilter(audioSourceGameObject);
				lowPassFilter.cutoffFrequency = 
					AudioAssetUtils.FilterAmountToCutoffFrequency(100 - assetPlaybackData.LowpassAmount);
			}
		}

		private void CreateHighpassFilter(GameObject audioSourceGameObject)
		{
			highPassFilter = audioSourceGameObject.AddComponent<AudioHighPassFilter>();
			highPassFilter.cutoffFrequency = 20;
		}

		private void CreateLowpassFilter(GameObject audioSourceGameObject)
		{
			lowPassFilter = audioSourceGameObject.AddComponent<AudioLowPassFilter>();
			lowPassFilter.cutoffFrequency = AudioSettings.outputSampleRate / 2.0f;
		}
		#endregion

		#region RTPC
		public void BindRTPCs(List<AudioAssetRTPCBinding> bindings)
		{
			foreach (AudioAssetRTPCBinding binding in bindings)
			{
				rtpcBindings.Add(binding);
			}
		}
		
		private void UpdateRTPCs()
		{
			foreach (AudioAssetRTPCBinding binding in rtpcBindings)
			{
				ApplyRTPCValue(binding);
			}
		}

		private void ApplyRTPCValue(AudioAssetRTPCBinding binding)
		{
			switch (binding.ParameterType)
			{
				case (AudioAssetParameterType.FilterHighpass):
					HandleHighpassRTPC(binding);
					break;
				case (AudioAssetParameterType.FilterLowpass):
					HandleLowpassRTPC(binding);
					break;
				case (AudioAssetParameterType.Pitch):
					HandlePitchRTPC(binding);
					break;
				case (AudioAssetParameterType.Volume):
					HandleVolumeRTPC(binding);
					break;
			}
		}

		private void HandleHighpassRTPC(AudioAssetRTPCBinding binding)
		{
			if (highPassFilter == null)
			{
				CreateHighpassFilter(audioSource.gameObject);
			}

			float value = binding.Value;

			highPassFilter.cutoffFrequency = AudioAssetUtils.FilterAmountToCutoffFrequency((int) value);
		}
		
		private void HandleLowpassRTPC(AudioAssetRTPCBinding binding)
		{
			if (lowPassFilter == null)
			{
				CreateLowpassFilter(audioSource.gameObject);
			}
			
			float value = binding.Value;

			lowPassFilter.cutoffFrequency = AudioAssetUtils.FilterAmountToCutoffFrequency(100 - (int) value);
		}
		
		private void HandlePitchRTPC(AudioAssetRTPCBinding binding)
		{
			float value = binding.Value;
			
			audioSource.pitch = AudioAssetUtils.CentsToRatio((int) value);
		}

		private void HandleVolumeRTPC(AudioAssetRTPCBinding binding)
		{
			float value = binding.Value;
			
			audioSource.volume =  AudioAssetUtils.DecibelsToAmplitude(value);
		}
		#endregion

		#region State
		public void InitializeState(List<AudioAssetStateData> stateDataList)
		{
			stateData = stateDataList;
			
			foreach (AudioAssetStateData data in stateData)
			{
				data.StateGroup.OnStateChanged += HandleStateTransition;
				HandleStateTransition(data.StateGroup, data.StateGroup.CurrentState, 0);
			}
		}

		private void UnsubscribeStates()
		{
			if (stateData == null)
			{
				return;
			}
			
			foreach (AudioAssetStateData data in stateData)
			{
				data.StateGroup.OnStateChanged -= HandleStateTransition;
			}
		}

		private void HandleStateTransition(AudioAssetStateGroup stateGroup, string state, float transitionTime)
		{
			AudioAssetStateData data = stateData.Find(data => data.StateGroup == stateGroup);
			AudioAssetStateSetting setting = data.StateSettings.Find(setting => setting.State == state);
			AudioAssetStateAudioParameters offsets = setting.Offsets;

			if (stateTransitionCoroutine != null)
			{
				StopCoroutine(stateTransitionCoroutine);
				stateTransitionCoroutine = null;
			}

			stateTransitionCoroutine = StartCoroutine(StateTransition(offsets, transitionTime));
		}

		private IEnumerator StateTransition(AudioAssetStateAudioParameters offsets, float transitionTime)
		{
			float volumeStart = audioSource.volume;
			float pitchStart = audioSource.pitch;
			float volumeTarget = AudioAssetUtils.DecibelsToAmplitude(offsets.Volume);
			float pitchTarget = AudioAssetUtils.CentsToRatio(offsets.Pitch);

			float highpassStart = 0;
			float lowpassStart = 0;
			float highpassTarget = 0;
			float lowpassTarget = 0;

			if (highPassFilter != null)
			{
				highpassTarget = AudioAssetUtils.FilterAmountToCutoffFrequency
					(offsets.Highpass) - highPassFilter.cutoffFrequency;
			}

			if (lowPassFilter != null)
			{
				lowpassTarget = AudioAssetUtils.FilterAmountToCutoffFrequency
					(100 - offsets.Lowpass) - lowPassFilter.cutoffFrequency;
			}

			if (highpassTarget > 0)
			{
				if (highPassFilter == null)
				{
					CreateHighpassFilter(audioSource.gameObject);
				}
				
				highpassStart = highPassFilter.cutoffFrequency;
			}

			if (lowpassTarget > 0)
			{
				if (lowPassFilter == null)
				{
					CreateLowpassFilter(audioSource.gameObject);
				}
				
				lowpassStart = lowPassFilter.cutoffFrequency;
			}
			
			float currentTime = 0.0f;

			while (currentTime <= transitionTime)
			{
				float progress = transitionTime > 0 ? currentTime / transitionTime : 1.0f;
				float nextTime = currentTime + Time.deltaTime;

				audioSource.volume = Mathf.Lerp(volumeStart, volumeTarget, progress);
				audioSource.pitch = Mathf.Lerp(pitchStart, pitchTarget, progress);

				if (highpassTarget > 0)
				{
					highPassFilter.cutoffFrequency = Mathf.Lerp(highpassStart, highpassTarget, progress);
				}

				if (lowpassTarget > 0)
				{
					lowPassFilter.cutoffFrequency = Mathf.Lerp(lowpassStart, lowpassTarget, progress);
				}
				
				currentTime = nextTime;
				yield return null;
			}
		}
		#endregion

		private IEnumerator DelayCoroutine(AudioAssetPlaybackData data, bool editorPreview)
		{
			if (!editorPreview)
			{
				yield return new WaitForSeconds(data.Delay);
			}
			else
			{
				yield return new EditorWaitForSeconds(data.Delay);
			}

			if (data.FadeInTime > 0)
			{
				HandleFadeIn(data, editorPreview);
			}
			else
			{
				audioSource.Play();				
				PrepareStop(editorPreview);
			}
		}
		
		#region Fade
		private IEnumerator FadeCoroutine(float fadeTime, AudioAssetFadeType fadeType, float delay = 0)
		{
			if (delay > 0)
			{
				yield return new WaitUntil(() => audioSource.time >= delay);
			}
			
			float currentTime = 0.0f;

			float volumeStart = fadeType == AudioAssetFadeType.FadeIn ? 0.0f : audioSource.volume;
			float volumeTarget = fadeType == AudioAssetFadeType.FadeIn ? 
				AudioAssetUtils.DecibelsToAmplitude(assetPlaybackData.Volume) : 0.0f;
			
			while (currentTime <= fadeTime)
			{
				float progress = currentTime / fadeTime;
				float nextTime = currentTime + Time.deltaTime;
				
				if (audioSource == null)
				{
					yield break;
				}

				audioSource.volume = Mathf.Lerp(volumeStart, volumeTarget, progress);

				currentTime = nextTime;
				
				yield return null;
			}

			if (fadeType == AudioAssetFadeType.FadeOut)
			{
				audioSource.Stop();
			}
		}
		#endregion
		
		#region Helpers
		private bool PlayImmediate(AudioAssetPlaybackData assetPlaybackData)
		{
			return assetPlaybackData.FadeInTime <= 0.0f && assetPlaybackData.Delay <= 0.0f;
		}

		private bool StopImmediate(AudioAssetPlaybackData assetPlaybackData)
		{
			return assetPlaybackData.FadeOutTime <= 0.0f;
		}
		#endregion

		#region Editor
#if UNITY_EDITOR
		public void PlayEditorPreview
		(
			AudioAssetPlayable audioAsset, AudioAssetPlaybackData data, Transform previewParentTransform,
			AudioAssetPlayable.PlayingSourceID playingSourceID,
			AudioAssetPlayable.OnFinishedPlayingDelegate onFinishedPlayingCallback
		)
		{
			parentTransform = previewParentTransform;
			
			Play(audioAsset, data, parentTransform, playingSourceID, onFinishedPlayingCallback, true);
		}
		
		private IEnumerator FadeCoroutineEditorPreview
			(float fadeTime, AudioAssetFadeType fadeType, float delay = 0)
		{
			if (delay > 0)
			{
				yield return new WaitUntil(() => audioSource.time >= delay);
			}

			const float frameDuration = 0.01667f;
			float currentTime = 0.0f;

			EditorWaitForSeconds frame = new (frameDuration);

			float volumeStart = fadeType == AudioAssetFadeType.FadeIn ? 0.0f : audioSource.volume;
			float volumeTarget = fadeType == AudioAssetFadeType.FadeIn ? 
				AudioAssetUtils.DecibelsToAmplitude(assetPlaybackData.Volume) : 0.0f;

			while (currentTime <= fadeTime)
			{
				float progress = currentTime / fadeTime;
				float nextTime = currentTime + frameDuration;
				
				if (audioSource == null)
				{
					yield break;
				}

				audioSource.volume = Mathf.Lerp(volumeStart, volumeTarget, progress);

				currentTime = nextTime;
				
				yield return frame;
			}

			audioSource.volume = volumeTarget;

			if (fadeType == AudioAssetFadeType.FadeOut)
			{
				audioSource.Stop();
			}
		}
#endif
		#endregion
	}
}