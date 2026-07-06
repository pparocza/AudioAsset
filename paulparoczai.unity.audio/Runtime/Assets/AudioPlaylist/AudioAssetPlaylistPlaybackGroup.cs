using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace AudioAsset
{
	[AddComponentMenu("")]
	public class AudioAssetPlaylistPlaybackGroup : MonoBehaviour
	{
		private struct AudioSourceData
		{
			public AudioSource AudioSource;
			public float TargetVolume;
			public bool OneShot;
			public bool OneShotIgnoresChanges;
		}

		private bool loop;
		public bool Loop
		{
			get => loop;
			set => SetLoop(value);
		}
		
		private bool isStopping;
		
		private readonly List<AudioSourceData> audioSourceData = new();

		#region Play/Stop
		public void PlayAtTime
			(double time, double fadeTime, AudioAssetPlaylistFadeType fadeType, AudioAssetPlaylistFadeCurve fadeCurve)
		{
			bool fadeIn = fadeTime > 0.0f;

			if (fadeIn)
			{
				PrepareFadeIn(time, fadeTime, fadeType, fadeCurve);
				return;
			}

			foreach (AudioSourceData sourceData in audioSourceData)
			{
				sourceData.AudioSource.PlayScheduled(time);
			}
		}

		public void Stop(float fadeTime, AudioAssetPlaylistFadeCurve fadeCurve)
		{
			if (!gameObject.activeInHierarchy || isStopping)
			{
				return;
			}

			isStopping = true;
			StartCoroutine(FadeOutAndStop(fadeTime, fadeCurve));
		}
		#endregion

		#region Fade Handling
		private void PrepareFadeIn
			(double time, double fadeTime, AudioAssetPlaylistFadeType fadeType, AudioAssetPlaylistFadeCurve fadeCurve)
		{
			foreach (AudioSourceData sourceData in audioSourceData)
			{
				sourceData.AudioSource.volume = 0;

				switch (fadeType)
				{
					case AudioAssetPlaylistFadeType.Immediate:
						PrepareFadeInImmediate(time, sourceData);
						break;
					case AudioAssetPlaylistFadeType.AtEnd:
						PrepareFadeInAtEnd(time, fadeTime, sourceData);
						break;
					case AudioAssetPlaylistFadeType.AtStart:
						PrepareFadeInAtStart(time, sourceData);
						break;
				}
			}

			if (gameObject.activeInHierarchy)
			{
				StartCoroutine(FadeInAtTime(time, fadeTime, fadeCurve));
			}
		}
		
		private void PrepareFadeInImmediate(double time, AudioSourceData sourceData)
		{
			sourceData.AudioSource.PlayScheduled(time);
		}

		private void PrepareFadeInAtEnd(double time, double fadeTime, AudioSourceData sourceData)
		{
			if (sourceData.OneShot)
			{
				sourceData.AudioSource.PlayScheduled(time + fadeTime);
				return;
			}
				
			float audioClipLength = sourceData.AudioSource.clip.length;
			sourceData.AudioSource.time = (float) (audioClipLength - fadeTime);
			sourceData.AudioSource.loop = true;
			StartCoroutine(HandleFadeLoop(fadeTime));
			sourceData.AudioSource.PlayScheduled(time);
		}
		
		private void PrepareFadeInAtStart(double time, AudioSourceData sourceData)
		{
			sourceData.AudioSource.loop = !sourceData.OneShot;
			sourceData.AudioSource.PlayScheduled(time);
		}
		
		private IEnumerator FadeInAtTime(double startTime, double fadeTime, AudioAssetPlaylistFadeCurve fadeCurve)
		{
			while (AudioSettings.dspTime < startTime)
			{
				yield return null;
			}
			
			float currentTime = 0;

			while (currentTime <= fadeTime)
			{
				float progress = (float) (currentTime / fadeTime);
				float fadeFactor = ApplyFadeCurve(fadeCurve, progress);
				
				float nextTime = currentTime + Time.deltaTime;
				
				foreach(AudioSourceData sourceData in audioSourceData)
				{
					float fadeVolume = sourceData.TargetVolume * fadeFactor;
					sourceData.AudioSource.volume = nextTime < fadeTime ? fadeVolume : sourceData.TargetVolume;
				}
				
				currentTime = nextTime;
				
				yield return null;
			}
		}
		
		private IEnumerator FadeOutAndStop(float fadeTime, AudioAssetPlaylistFadeCurve fadeCurve)
		{
			float currentTime = 0;

			// Fade out all currently playing sources that don't ignore changes
			while (currentTime <= fadeTime)
			{
				float progress = currentTime / fadeTime;
				float fadeFactor = ApplyFadeCurve(fadeCurve, 1 - progress);
				
				float nextTime = currentTime + Time.deltaTime;
				
				foreach (AudioSourceData sourceData in audioSourceData)
				{
					if (sourceData.OneShot && sourceData.OneShotIgnoresChanges)
					{
						continue;
					}
					
					float fadeVolume = sourceData.TargetVolume * fadeFactor;
					sourceData.AudioSource.volume = nextTime < fadeTime ? fadeVolume : 0.0f;
				}

				currentTime = nextTime;

				yield return null;
			}
			
			// Stop and destroy all sources that don't ignore changes
			foreach (AudioSourceData sourceData in audioSourceData)
			{
				if (sourceData.OneShot && sourceData.OneShotIgnoresChanges)
				{
					continue;
				}
				
				AudioSource audioSource = sourceData.AudioSource;
				audioSource.Stop();
				Destroy(audioSource.gameObject);
			}

			// Wait for oneshots that ignore changes to finish playing
			while (OneShotPlaying)
			{
				yield return null;
			}

			// Destroy everything that's left
			foreach (AudioSourceData sourceData in audioSourceData)
			{
				if (sourceData.AudioSource != null)
				{
					Destroy(sourceData.AudioSource.gameObject);
				}
			}
			
			audioSourceData.Clear();

			Destroy(gameObject);
		}
		
		private IEnumerator HandleFadeLoop(double fadeTime)
		{
			bool hasLooped = false;
			
			while (!hasLooped)
			{
				hasLooped = audioSourceData.Any(sourceData => 
					sourceData.AudioSource.time < sourceData.AudioSource.clip.length - fadeTime &&
					sourceData.AudioSource.time > 0.0f
				);
				
				yield return null;
			}
			
			foreach(AudioSourceData sourceData in audioSourceData)
			{
				sourceData.AudioSource.loop = false;
			}
		}
		#endregion
		
		#region Audio Source Creation
		public void CreateAudioSources
		(
			AudioAssetPlaylistArrangement arrangement, GameObject audioSourceParent, 
			bool quantized, bool bypassReverbZones, bool playOneShots
		)
		{
			foreach (AudioAssetPlaylistArrangementTrack arrangementTrack in arrangement.ArrangementTracks)
			{
				if (!ShouldPlay(arrangementTrack.Probability))
				{
					continue;
				}

				if (arrangementTrack.OneShot && !playOneShots)
				{
					continue;
				}

				SetLoop(!quantized && !arrangementTrack.OneShot);
				
				AudioSourceData source = 
					CreateAudioSourceDataForPlaylistTrack(arrangementTrack, audioSourceParent, bypassReverbZones);

				audioSourceData.Add(source);
			}
		}
		
		private AudioSourceData CreateAudioSourceDataForPlaylistTrack
			(AudioAssetPlaylistArrangementTrack assetPlaylistArrangementTrack, GameObject parent, bool bypassReverbZones)
		{
			AudioAssetPlaylistTrack playlistTrack = assetPlaylistArrangementTrack.PlaylistTrack;
			
			UnityEngine.AudioClip clip = playlistTrack.GetAudioClip();
			clip.LoadAudioData();

			GameObject sourceObject = new()
			{
				transform = {parent = gameObject.transform},
				name = clip.name
			};
			
			sourceObject.transform.parent = parent.transform;
			sourceObject.name = clip.name;
			
			AudioSource source = sourceObject.AddComponent<AudioSource>();
			source.volume = AudioAssetUtils.DecibelsToAmplitude(playlistTrack.Volume);
			source.clip = clip;
			source.playOnAwake = false;
			source.loop = loop;
			source.spatialBlend = 0;
			source.outputAudioMixerGroup = playlistTrack.AudioMixerGroup != null ? playlistTrack.AudioMixerGroup : null;

			source.bypassReverbZones = bypassReverbZones;

			AudioSourceData sourceData = new ()
			{
				AudioSource = source,
				TargetVolume = AudioAssetUtils.DecibelsToAmplitude(playlistTrack.Volume),
				OneShot = assetPlaylistArrangementTrack.OneShot,
				OneShotIgnoresChanges = assetPlaylistArrangementTrack.IgnoreSectionChanges
			};

			return sourceData;
		}
		#endregion

		#region Helpers
		public bool IsPlaying => audioSourceData.Any(sourceData => sourceData.AudioSource.isPlaying);
		public bool IsReady => audioSourceData.All
			(sourceData => sourceData.AudioSource.clip.loadState == AudioDataLoadState.Loaded);

		private bool OneShotPlaying => audioSourceData.Any
			(sourceData => sourceData.OneShot && sourceData.OneShotIgnoresChanges && sourceData.AudioSource.isPlaying);

		private static bool ShouldPlay(int probability)
		{
			int randomInt = Random.Range(0, 100);
			bool shouldPlay = randomInt <= probability;
			return shouldPlay;
		}
		
		private float ApplyFadeCurve(AudioAssetPlaylistFadeCurve fadeCurve, float value) => fadeCurve switch
		{
			AudioAssetPlaylistFadeCurve.EqualPower => Mathf.Sqrt(value),
			AudioAssetPlaylistFadeCurve.Exponential => Mathf.Pow(value, 2),
			AudioAssetPlaylistFadeCurve.Linear => value,
			_ => value
		};

		private void SetLoop(bool shouldLoop)
		{
			foreach (AudioSourceData sourceData in audioSourceData)
			{
				if (!sourceData.OneShot)
				{
					sourceData.AudioSource.loop = shouldLoop;
				}
			}

			loop = shouldLoop;
		}
		#endregion
	}
}