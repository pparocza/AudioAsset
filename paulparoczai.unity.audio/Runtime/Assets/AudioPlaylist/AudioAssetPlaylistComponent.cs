using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace AudioAsset
{
	[AddComponentMenu("")]
	public class AudioAssetPlaylistComponent : MonoBehaviour
	{
		private class PlaylistQueueItem
		{
			public AudioAssetPlaylistPlaybackGroup PlaybackGroup;
			public double StartTime;
			public double EndTime;
			public double Length => EndTime - StartTime;
		}

		private bool isStarted;
		private int currentSectionIndex;

		private Queue<PlaylistQueueItem> playlistQueue = new();
		private PlaylistQueueItem currentlyPlayingQuantizedQueueItem;
		private PlaylistQueueItem autoScheduledQuantizedQueueItem;
		
		public AudioAssetPlaylist Playlist { get; set; }

		private void Update()
		{
			if (Playlist == null || !isStarted || !Playlist.Quantize)
			{
				return;
			}
			
			if (playlistQueue.Count > 0 && AudioSettings.dspTime < playlistQueue.Peek().StartTime)
			{
				return;
			}
			
			HandleCurrentQuantizedSectionFinished(Playlist);
		}
		
		#region Playlist Control
		public void StartPlaylist()
		{
			if (Playlist == null)
			{
				Debug.LogWarning("AudioPlaylist attempted to start a section, but PlaylistConfig is null.");
				return;
			}

			if (Playlist.Quantize)
			{
				StartQuantized(Playlist);
			}
			else
			{
				StartNotQuantized(Playlist);
			}
		}

		private void StartQuantized(AudioAssetPlaylist playlist)
		{
			EnqueueAndScheduleQuantizedStartPlaylistItem
			(
				playlist.DefaultSection, playlist.StartFadeInLength, playlist, 
				AudioAssetPlaylistFadeType.Immediate, playlist.StartStopFadeCurve
			);
		}
		
		private void StartNotQuantized(AudioAssetPlaylist playlist)
		{
			EnqueueAndSchedulePlaylistItem
			(
				playlist.DefaultSection, AudioSettings.dspTime, playlist.StartFadeInLength, 
				playlist, AudioAssetPlaylistFadeType.Immediate, playlist.StartStopFadeCurve
			);
				
			isStarted = true;
		}

		public void AdvancePlaylist(bool overrideCurrentSchedule)
		{
			if (Playlist == null)
			{
				Debug.LogWarning("AudioPlaylist attempted to advance, but PlaylistConfig is null.");
				return;
			}
			
			if (Playlist.Quantize)
			{
				EnqueueNextQuantizedPlaybackGroup
					(GetNextSectionIndex(Playlist), Playlist, overrideCurrentSchedule);
			}
			else
			{
				PlayNextPlaybackGroupImmediate(GetNextSectionIndex(Playlist), Playlist);
			}
		}

		public void StartSection(int sectionIndex, bool overrideCurrentSchedule)
		{
			if (Playlist == null)
			{
				Debug.LogWarning("AudioPlaylist attempted to start a section, but PlaylistConfig is null.");
				return;
			}

			if (Playlist.Quantize)
			{
				EnqueueNextQuantizedPlaybackGroup(sectionIndex, Playlist, overrideCurrentSchedule);
			}
			else
			{
				PlayNextPlaybackGroupImmediate(sectionIndex, Playlist);
			}
		}

		public void StopPlaylist(bool disposePlaylist = false)
		{
			if (Playlist == null)
			{
				Debug.LogWarning("AudioPlaylist attempted to stop, but PlaylistConfig is null.");
				return;
			}
			
			AudioAssetPlaylistPlaybackGroup currentlyPlayingPlaybackGroup = null;

			if (Playlist.Quantize && currentlyPlayingQuantizedQueueItem != null)
			{
				currentlyPlayingPlaybackGroup = currentlyPlayingQuantizedQueueItem.PlaybackGroup;
			}
			else if(playlistQueue.Count > 0)
			{
				currentlyPlayingPlaybackGroup = playlistQueue.Peek().PlaybackGroup;
			}

			foreach (PlaylistQueueItem queueItem in playlistQueue)
			{
				if (queueItem.PlaybackGroup != null)
				{
					queueItem.PlaybackGroup.Stop(Playlist.StopFadeOutLength, Playlist.StartStopFadeCurve);
				}
			}
			
			playlistQueue.Clear();

			if (!disposePlaylist)
			{
				return;
			}

			if (currentlyPlayingPlaybackGroup == null)
			{
				Destroy(gameObject);
			}
			else
			{
				currentlyPlayingPlaybackGroup.Stop(Playlist.StopFadeOutLength, Playlist.StartStopFadeCurve);
				StartCoroutine(DestroySelf(currentlyPlayingPlaybackGroup));
			}
		}
		#endregion

		#region Playback Group Scheduling
		private void HandleCurrentQuantizedSectionFinished(AudioAssetPlaylist playlist)
		{
			if (currentlyPlayingQuantizedQueueItem != null)
			{
				if (currentlyPlayingQuantizedQueueItem.EndTime <= AudioSettings.dspTime)
				{
					currentlyPlayingQuantizedQueueItem.PlaybackGroup.Stop
						(0, playlist.ChangeSectionFadeCurve);
				}
				else
				{
					currentlyPlayingQuantizedQueueItem.PlaybackGroup.Stop
						(playlist.ChangeSectionFadeOutLength, playlist.ChangeSectionFadeCurve);
				}
			}

			if (playlistQueue.Count == 0)
			{
				return;
			}
			
			currentlyPlayingQuantizedQueueItem = playlistQueue.Dequeue();

			if (playlistQueue.Count == 0)
			{
				ScheduleNextPlaybackGroupForCurrentSection(playlist, true);
			}
		}
		
		private void ScheduleNextPlaybackGroupForCurrentSection(AudioAssetPlaylist playlist, bool autoScheduled)
		{
			double playTime = currentlyPlayingQuantizedQueueItem.EndTime;

			EnqueueAndSchedulePlaylistItem
			(
				currentSectionIndex, playTime, 0, playlist, AudioAssetPlaylistFadeType.Immediate, 
				playlist.ChangeSectionFadeCurve, autoScheduled
			);

			if (playlist.ChangeSectionFadeType == AudioAssetPlaylistFadeType.AtStart)
			{
				SetCurrentlyPlayingQuantizedQueueItemLoop(false);
			}
		}

		private void EnqueueNextQuantizedPlaybackGroup
			(int sectionIndex, AudioAssetPlaylist playlist, bool overrideCurrentSchedule = false)
		{
			// If something was automatically scheduled or the schedule should be overriden
			if (autoScheduledQuantizedQueueItem != null || overrideCurrentSchedule)
			{
				OverrideNextQuantizedPlaybackGroup(sectionIndex, playlist);
			}
			else
			{
				EnqueueQuantizedPlaybackGroup(sectionIndex, playlist);
			}
			
			currentSectionIndex = sectionIndex;
		}

		private void OverrideNextQuantizedPlaybackGroup(int sectionIndex, AudioAssetPlaylist playlist)
		{
			bool itemsInQueue = playlistQueue.Count > 0;
			
			if (itemsInQueue)
			{
				PlaylistQueueItem queueItemToDiscard = playlistQueue.Dequeue();
				
				EnqueueAndSchedulePlaylistItem
				(
					sectionIndex, queueItemToDiscard.StartTime, playlist.ChangeSectionFadeInLength,
					playlist, playlist.ChangeSectionFadeType, playlist.ChangeSectionFadeCurve
				);
				
				Destroy(queueItemToDiscard.PlaybackGroup.gameObject);
			}

			autoScheduledQuantizedQueueItem = null;
		}

		private void EnqueueQuantizedPlaybackGroup(int sectionIndex, AudioAssetPlaylist playlist)
		{
			double playTime = 0;

			if (playlistQueue.Count > 0)
			{
				PlaylistQueueItem lastQueueItem = GetLastQueueItem();
				playTime = lastQueueItem.EndTime;
			}

			EnqueueAndSchedulePlaylistItem
			(
				sectionIndex, playTime, playlist.ChangeSectionFadeInLength, playlist,
				playlist.ChangeSectionFadeType, playlist.ChangeSectionFadeCurve
			);
		}
		
		private void PlayNextPlaybackGroupImmediate(int sectionIndex, AudioAssetPlaylist playlist)
		{
			if (playlistQueue.Count > 0)
			{
				PlaylistQueueItem queueItem = playlistQueue.Dequeue();

				if (queueItem.PlaybackGroup != null)
				{
					queueItem.PlaybackGroup.Stop
						(playlist.ChangeSectionFadeOutLength, playlist.StartStopFadeCurve);
				}
			}

			EnqueueAndSchedulePlaylistItem
			(
				sectionIndex, AudioSettings.dspTime, playlist.ChangeSectionFadeInLength, playlist,
				AudioAssetPlaylistFadeType.Immediate, playlist.ChangeSectionFadeCurve
			);
			
			currentSectionIndex = sectionIndex;
		}
		#endregion

		#region Queue Handlers
		private void EnqueueAndSchedulePlaylistItem
		(
			int sectionIndex, double sectionStartTime, double fadeInLength, 
			AudioAssetPlaylist playlist, AudioAssetPlaylistFadeType fadeType, 
			AudioAssetPlaylistFadeCurve fadeCurve, bool autoScheduled = false
		)
		{
			AudioAssetPlaylistSectionData sectionData = playlist.GetPlaylistSection(sectionIndex);

			if (sectionData == null)
			{
				return;
			}

			bool quantize = playlist.Quantize;
			bool bypassReverbZones = playlist.BypassReverbZones;
			bool playOneShots = sectionIndex != currentSectionIndex;
			
			AudioAssetPlaylistPlaybackGroup playbackGroup = CreatePlaybackGroup
				(sectionData, quantize, bypassReverbZones, playOneShots);
			
			PlaylistQueueItem queueItem = CreatePlaylistQueueItem
				(sectionStartTime, fadeInLength, playlist, sectionData, fadeType, playbackGroup);

			playlistQueue.Enqueue(queueItem);
			
			playbackGroup.PlayAtTime(queueItem.StartTime, fadeInLength, fadeType, fadeCurve);

			if (autoScheduled)
			{
				autoScheduledQuantizedQueueItem = queueItem;
			}
		}
		
		private void EnqueueAndScheduleQuantizedStartPlaylistItem
		(
			int sectionIndex, double fadeInLength, AudioAssetPlaylist playlist, 
			AudioAssetPlaylistFadeType fadeType, AudioAssetPlaylistFadeCurve fadeCurve
		)
		{
			AudioAssetPlaylistSectionData sectionData = playlist.GetPlaylistSection(sectionIndex);

			if (sectionData == null)
			{
				return;
			}

			StartCoroutine
			(
			StartPlaybackGroupWhenReady
					(sectionIndex, fadeInLength, playlist, sectionData, fadeType, fadeCurve)
			);
		}

		private PlaylistQueueItem CreatePlaylistQueueItem
		(
			double sectionStartTime, double fadeInTime, AudioAssetPlaylist playlist, 
			AudioAssetPlaylistSectionData sectionData, AudioAssetPlaylistFadeType fadeType, 
			AudioAssetPlaylistPlaybackGroup playbackGroup
		)
		{
			return fadeType switch
			{
				AudioAssetPlaylistFadeType.AtEnd =>
					HandleFadeAtEnd(sectionStartTime, fadeInTime, playlist, sectionData, playbackGroup),
				AudioAssetPlaylistFadeType.AtStart => 
					HandleFadeAtStart(sectionStartTime, playlist, sectionData, playbackGroup),
				AudioAssetPlaylistFadeType.Immediate =>
					HandleFadeImmediate(sectionStartTime, playlist, playbackGroup, sectionData),
				_ => throw new ArgumentOutOfRangeException(nameof(fadeType), fadeType, null)
			};
		}
		#endregion
		
		#region Fade Handlers
		private PlaylistQueueItem HandleFadeAtEnd
		(
			double sectionStartTime, double fadeInTime, AudioAssetPlaylist playlist,
			AudioAssetPlaylistSectionData sectionData, AudioAssetPlaylistPlaybackGroup playbackGroup
		)
		{
			PlaylistQueueItem queueItem = new()
			{
				PlaybackGroup = playbackGroup
			};

			if (!playlist.Quantize)
			{
				queueItem.StartTime = sectionStartTime;
				return queueItem;
			}

			double length = QuantizationToSeconds
			(
				playlist.BeatsPerBar, playlist.Bpm, 
				sectionData.QuantizationType, sectionData.QuantizationLength
			);
			
			double startTimeTarget = sectionStartTime - fadeInTime;
			bool enoughTimeForFade = AudioSettings.dspTime < startTimeTarget;

			if (enoughTimeForFade)
			{
				queueItem.StartTime = startTimeTarget;
				queueItem.EndTime = sectionStartTime + length;
			}
			else
			{
				ScheduleNextPlaybackGroupForCurrentSection(playlist, false);

				double nextEndTime = currentlyPlayingQuantizedQueueItem.EndTime +
				                     currentlyPlayingQuantizedQueueItem.Length;

				queueItem.StartTime = nextEndTime - fadeInTime;
				queueItem.EndTime = queueItem.StartTime + length + fadeInTime;
			}

			return queueItem;
		}

		private PlaylistQueueItem HandleFadeAtStart
		(
			double sectionStartTime, AudioAssetPlaylist playlist,
			AudioAssetPlaylistSectionData sectionData, AudioAssetPlaylistPlaybackGroup playbackGroup
		)
		{
			SetCurrentlyPlayingQuantizedQueueItemLoop(true);
			
			PlaylistQueueItem queueItem = new()
			{
				PlaybackGroup = playbackGroup,
				StartTime = sectionStartTime
			};
			
			if (!playlist.Quantize)
			{
				return queueItem;
			}
			
			double length = QuantizationToSeconds
			(
				playlist.BeatsPerBar, playlist.Bpm,
				sectionData.QuantizationType, sectionData.QuantizationLength
			);
			
			double endTime = sectionStartTime + length;
			queueItem.EndTime = endTime;

			return queueItem;
		}

		private PlaylistQueueItem HandleFadeImmediate
		(
			double sectionStartTime, AudioAssetPlaylist playlist, 
			AudioAssetPlaylistPlaybackGroup playbackGroup, AudioAssetPlaylistSectionData sectionData
		)
		{
			PlaylistQueueItem queueItem = new()
			{
				PlaybackGroup = playbackGroup,
				StartTime = sectionStartTime
			};
			
			if (!playlist.Quantize)
			{
				return queueItem;
			}

			double length = QuantizationToSeconds
			(
				playlist.BeatsPerBar, playlist.Bpm,
				sectionData.QuantizationType, sectionData.QuantizationLength
			);
			
			double endTime = sectionStartTime + length;
			queueItem.EndTime = endTime;

			return queueItem;
		}
		#endregion
		
		#region Helpers
		private int GetNextSectionIndex(AudioAssetPlaylist playlist) => 
			(currentSectionIndex + 1) % playlist.NSections;

		private PlaylistQueueItem GetLastQueueItem() =>
			playlistQueue.Count > 0 ? playlistQueue.ToArray()[playlistQueue.Count - 1] : null;

		private AudioAssetPlaylistPlaybackGroup CreatePlaybackGroup
			(AudioAssetPlaylistSectionData sectionData, bool quantize, bool bypassReverbZones, bool playOneShots)
		{
			AudioAssetPlaylistSection section = sectionData.Section;
			AudioAssetPlaylistArrangement arrangement = section.GetPlaylistArrangement();

			GameObject playbackGroupObject = new()
			{
				transform = { parent = gameObject.transform },
				name = "AudioPlaylistPlaybackGroup"
			};

			AudioAssetPlaylistPlaybackGroup playbackGroup = 
				playbackGroupObject.AddComponent<AudioAssetPlaylistPlaybackGroup>();

			playbackGroup.CreateAudioSources
			(
				arrangement, playbackGroupObject, quantize, bypassReverbZones, playOneShots
			);
			
			return playbackGroup;
		}
		
		private double QuantizationToSeconds
		(
			int beatsPerBar, float bpm, AudioAssetPlaylistQuantizationType quantizationType, int quantizationLength
		)
		{
			double beats = quantizationType == AudioAssetPlaylistQuantizationType.Bars
				? quantizationLength * beatsPerBar
				: quantizationLength;

			double validBpm = bpm > 0 ? bpm : 1;
			
			double secondsPerBeat = 60.0f / validBpm;
			double seconds = beats * secondsPerBeat;

			return seconds;
		}
		
		private void SetCurrentlyPlayingQuantizedQueueItemLoop(bool loop)
		{
			if 
			(
				currentlyPlayingQuantizedQueueItem != null && 
				currentlyPlayingQuantizedQueueItem.PlaybackGroup != null
			)
			{
				currentlyPlayingQuantizedQueueItem.PlaybackGroup.Loop = loop;
			}
		}
		#endregion

		private IEnumerator StartPlaybackGroupWhenReady
		(
			int sectionIndex, double fadeInLength, AudioAssetPlaylist playlist,
			AudioAssetPlaylistSectionData sectionData, AudioAssetPlaylistFadeType fadeType, 
			AudioAssetPlaylistFadeCurve fadeCurve
		)
		{
			bool quantize = playlist.Quantize;
			bool bypassReverbZones = playlist.BypassReverbZones;
			bool playOneShots = sectionIndex != currentSectionIndex;
			
			AudioAssetPlaylistPlaybackGroup playbackGroup = CreatePlaybackGroup
				(sectionData, quantize, bypassReverbZones, playOneShots);
			
			while (!playbackGroup.IsReady)
			{
				yield return null;
			}

			PlaylistQueueItem queueItem = CreatePlaylistQueueItem
				(AudioSettings.dspTime, fadeInLength, playlist, sectionData, fadeType, playbackGroup);

			playlistQueue.Enqueue(queueItem);
			
			playbackGroup.PlayAtTime(queueItem.StartTime, fadeInLength, fadeType, fadeCurve);

			isStarted = true;
		}
		
		#region Cleanup
		private IEnumerator DestroySelf(AudioAssetPlaylistPlaybackGroup currentlyPlayingPlaybackGroup)
		{
			while (currentlyPlayingPlaybackGroup != null && currentlyPlayingPlaybackGroup.IsPlaying)
			{
				yield return null;
			}
			
			Destroy(gameObject);
		}
		#endregion
	}
}

