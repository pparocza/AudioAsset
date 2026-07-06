using UnityEngine;

namespace AudioAsset
{
	[System.Serializable]
	public class AudioAssetPlaylistArrangementTrack
	{
		[SerializeField]
		private AudioAssetPlaylistTrack playlistTrack;
		public AudioAssetPlaylistTrack PlaylistTrack
		{
			get => playlistTrack;
			set => playlistTrack = value;
		}

		[SerializeField, Range(0, 100)]
		private int probability = 100;
		public int Probability => probability;
		
		[SerializeField]
		private bool oneShot;
		public bool OneShot => oneShot;
		
		[SerializeField]
		private bool ignoreSectionChanges;
		public bool IgnoreSectionChanges => ignoreSectionChanges;
	}
}