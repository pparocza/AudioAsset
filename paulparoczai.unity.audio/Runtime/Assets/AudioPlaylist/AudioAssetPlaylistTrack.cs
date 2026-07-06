using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

namespace AudioAsset
{
	[System.Serializable]
	public class AudioAssetPlaylistTrack
	{
		[SerializeField]
		private List<UnityEngine.AudioClip> audioClips = new();

		[SerializeField, Range(-96, 0)]
		private float volume;
		public float Volume => volume;
		
		[SerializeField]
		private AudioMixerGroup audioMixerGroup;
		public AudioMixerGroup AudioMixerGroup => audioMixerGroup;
		
		public UnityEngine.AudioClip GetAudioClip() => audioClips[Random.Range(0, audioClips.Count)];
	}
}