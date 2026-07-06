using Unity.Collections;
using UnityEngine;

namespace AudioAsset
{
	[System.Serializable]
	public class AudioAssetStateSetting
	{
		[SerializeField, ReadOnly, HideInInspector]
		public string State;
		
		[SerializeField]
		private AudioAssetStateAudioParameters offsets;
		public AudioAssetStateAudioParameters Offsets => offsets;
	}
}