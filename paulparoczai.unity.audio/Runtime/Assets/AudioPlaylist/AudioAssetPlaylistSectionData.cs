using Sirenix.OdinInspector;
using UnityEngine;

namespace AudioAsset
{
	[System.Serializable]
	public class AudioAssetPlaylistSectionData
	{
		[SerializeField] 
		private AudioAssetPlaylistSection section;
		public AudioAssetPlaylistSection Section
		{
			get => section;
			set => section = value;
		}

		[SerializeField]
		private AudioAssetPlaylistQuantizationType quantizationType = AudioAssetPlaylistQuantizationType.Bars;
		public AudioAssetPlaylistQuantizationType QuantizationType => quantizationType;
		
		[SerializeField]
		private int quantizationLength = 1;
		public int QuantizationLength
		{
			get => quantizationLength;
			set => quantizationLength = value;
		}
	}
}