using UnityEngine;
using System.Collections.Generic;

namespace AudioAsset
{
	[System.Serializable]
	public class AudioAssetPlaylistSection
	{
		[SerializeField]
		private List<AudioAssetPlaylistArrangement> possibleArrangements = new();

		public AudioAssetPlaylistArrangement GetPlaylistArrangement() =>
			possibleArrangements[Random.Range(0, possibleArrangements.Count)];
	}
}