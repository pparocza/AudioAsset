using System;
using UnityEngine;
using System.Collections.Generic;

namespace AudioAsset
{
	[CreateAssetMenu(fileName = "AudioPlaylist", menuName = "Hololabs/Audio/Playlist")]
	public class AudioAssetPlaylist : ScriptableObject
	{
		public enum PlaylistCommand
		{
			Start,
			Stop,
			Advance,
			PlaySectionNext,
			QueueSection
		}

		private AudioAssetPlaylistComponent activePlaylistComponent;
		
		#region Inspector Fields
		[Header("Fade Settings")]
		[SerializeField]
		private float startFadeInLength;
		public float StartFadeInLength => startFadeInLength;

		[SerializeField] 
		private float stopFadeOutLength;
		public float StopFadeOutLength => stopFadeOutLength;

		[SerializeField] 
		private AudioAssetPlaylistFadeCurve startStopFadeCurve = AudioAssetPlaylistFadeCurve.Linear;
		public AudioAssetPlaylistFadeCurve StartStopFadeCurve => startStopFadeCurve;
		
		[SerializeField]
		private float changeSectionFadeInLength;
		public float ChangeSectionFadeInLength => changeSectionFadeInLength;

		[SerializeField]
		private float changeSectionFadeOutLength;
		public float ChangeSectionFadeOutLength => changeSectionFadeOutLength;

		[SerializeField] 
		private AudioAssetPlaylistFadeType changeSectionFadeType = AudioAssetPlaylistFadeType.Immediate;
		public AudioAssetPlaylistFadeType ChangeSectionFadeType => changeSectionFadeType;
		
		[SerializeField]
		private AudioAssetPlaylistFadeCurve changeSectionFadeCurve = AudioAssetPlaylistFadeCurve.EqualPower;
		public AudioAssetPlaylistFadeCurve ChangeSectionFadeCurve => changeSectionFadeCurve;
		
		[Header("Effects")]
		[SerializeField] 
		private bool bypassReverbZones = true;
		public bool BypassReverbZones => bypassReverbZones;

		[Header("Quantization")]
		[SerializeField]
		private bool quantize = true;
		public bool Quantize => quantize;
		
		[SerializeField]
		private float bpm = 120;
		public float Bpm => bpm;

		[SerializeField]
		private int beatsPerBar = 4;
		public int BeatsPerBar => beatsPerBar;

		[Header("Sections")]	
		[SerializeField]
		private int defaultSection;
		public int DefaultSection => defaultSection;
		
		[SerializeField] 
		private List<AudioAssetPlaylistSectionData> playlistSections = new();
		public List<AudioAssetPlaylistSectionData> PlaylistSections => playlistSections;
		public int NSections => playlistSections.Count;
		#endregion

		#region Playback
		public void Start()
		{
			if (activePlaylistComponent != null)
			{
				Debug.LogWarning
					($"AudioPlaylist {name} already started - will continue playing unchanged");
				
				return;
			}
			
			AudioAssetPlaylistComponent playlistComponent = InstantiateAudioPlaylist();
			playlistComponent.StartPlaylist();
		}
		
		public void Advance(bool overrideCurrentSchedule = false)
		{
			if (activePlaylistComponent == null)
			{
				Debug.LogWarning
					($"Attempted to advance {name} but it has not yet started. Attempting to start from default section.");
				
				Start();
				return;
			}

			if (activePlaylistComponent == null)
			{
				return;
			}

			activePlaylistComponent.AdvancePlaylist(overrideCurrentSchedule);
		}

		public void AdvanceNext()
		{
			Advance(true);
		}
		
		public void StartSection(int sectionIndex, bool overrideCurrentSchedule = false)
		{
			if (GetPlaylistSection(sectionIndex) == null)
			{
				Debug.LogWarning($"AudioPlaylist {name} does not have a section {sectionIndex}. Unable to start playlist section.");
				return;
			}

			if (activePlaylistComponent == null)
			{
				AudioAssetPlaylistComponent playlistComponent = InstantiateAudioPlaylist();
				playlistComponent.StartSection(sectionIndex, overrideCurrentSchedule);
			}
			else
			{
				if (activePlaylistComponent == null)
				{
					return;
				}
				
				activePlaylistComponent.StartSection(sectionIndex, overrideCurrentSchedule);
			}
		}

		public void StartSection(int sectionIndex)
		{
			StartSection(sectionIndex, false);
		}

		public void StartSectionNext(int sectionIndex)
		{
			StartSection(sectionIndex, true);
		}

		public void Stop()
		{
			if (activePlaylistComponent == null)
			{
				Debug.LogWarning
					($"AudioPlaylist {name} Stop method was called, but is not currently playing.");
				
				return;
			}
			
			activePlaylistComponent.StopPlaylist(disposePlaylist: true);
		}
		#endregion

		#region Helpers
		public void HandlePlaylistCommand(PlaylistCommand playlistCommand, int sectionIndex = 0)
		{
			switch (playlistCommand)
			{
				case PlaylistCommand.Start:
					Start();
					break;
				case PlaylistCommand.Advance:
					Advance();
					break;
				case PlaylistCommand.Stop:
					Stop();
					break;
				case PlaylistCommand.PlaySectionNext:
					StartSectionNext(sectionIndex);
					break;
				case PlaylistCommand.QueueSection:
					StartSection(sectionIndex);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		
		public AudioAssetPlaylistSectionData GetPlaylistSection(int sectionIndex)
		{
			if (sectionIndex >= 0 && sectionIndex < playlistSections.Count)
			{
				return playlistSections[sectionIndex];
			}

			Debug.LogWarning
				($"AudioPlaylistConfig.playlistSections has no item at index {sectionIndex}. Returning null");
			return null;
		}
		
		private AudioAssetPlaylistComponent InstantiateAudioPlaylist()
		{
			GameObject playlistObject = new()
			{
				name = $"AudioPlaylist_{name}"
			};

			AudioAssetPlaylistComponent playlistComponent = playlistObject.AddComponent<AudioAssetPlaylistComponent>();
			playlistComponent.Playlist = this;

			activePlaylistComponent = playlistComponent;

			return playlistComponent;
		}
		#endregion

		#region Validaton
		private void OnValidate()
		{
			ValidateDefaultSection();
			ValidateQuantization();
		}

		private void ValidateDefaultSection()
		{
			if (defaultSection < 0)
			{
				defaultSection = 0;
			}
			else if (defaultSection > NSections - 1)
			{
				defaultSection = NSections - 1;
			}
		}

		private void ValidateQuantization()
		{
			if (bpm <= 0)
			{
				bpm = 1;
			}
			
			if (beatsPerBar < 1)
			{
				beatsPerBar = 1;
			}
			
			foreach (AudioAssetPlaylistSectionData sectionData in playlistSections)
			{
				if (sectionData.QuantizationLength <= 0)
				{
					sectionData.QuantizationLength = 1;
				}
			}

			if (quantize && changeSectionFadeType == AudioAssetPlaylistFadeType.Immediate)
			{
				changeSectionFadeInLength = 0;
				changeSectionFadeOutLength = 0;
			}
		}
		#endregion
	}
}