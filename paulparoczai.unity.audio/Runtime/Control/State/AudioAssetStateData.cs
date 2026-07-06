using UnityEngine;
using System.Collections.Generic;

namespace AudioAsset
{
	[System.Serializable]
	public class AudioAssetStateData
	{
		[SerializeField]
		private AudioAssetStateGroup stateGroup;
		public AudioAssetStateGroup StateGroup => stateGroup;
		
		[SerializeField] 
		private List<AudioAssetStateSetting> stateSettings;
		public List<AudioAssetStateSetting> StateSettings
		{
			get => stateSettings;
			set => stateSettings = value;
		}
	}
}