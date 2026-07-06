using UnityEngine;
using System.Collections.Generic;

namespace AudioAsset
{
	[System.Serializable]
	public class AudioAssetPlaylistArrangement
	{
		[SerializeField] 
		private List<AudioAssetPlaylistArrangementTrack> arrangementTracks = new();
		public List<AudioAssetPlaylistArrangementTrack> ArrangementTracks => arrangementTracks;
	}
}