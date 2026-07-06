using UnityEngine;
using System.Collections.Generic;

namespace AudioAsset
{
	public class AudioAssetSwitch : AudioAssetPlayable
	{
		[System.Serializable]
		public class AudioAssetSwitchData
		{
			[SerializeField, HideInInspector]
			private int optionIndex = 0;
			public int OptionIndex => optionIndex;
			
			[SerializeField] 
			private AudioAssetPlayable assetPlayable;
			public AudioAssetPlayable AssetPlayable
			{
				get => assetPlayable;
				set => assetPlayable = value;
			}
		}
		
		[SerializeField] 
		private AudioAssetSwitchGroup switchGroup;
		public AudioAssetSwitchGroup SwitchGroup => switchGroup;

		[SerializeField] 
		private List<AudioAssetSwitchData> switchData = new();
		public List<AudioAssetSwitchData> SwitchData => switchData;
		
		private AudioAssetPlayable GetAsset()
		{
			if (switchGroup == null)
			{
				Debug.LogWarning($"No Switch Group specified for {name}");
				return null;
			}

			int currentSwitchSelectionIndex = SwitchGroup.CurrentOptionIndex;
			
			if (switchData.Count > 0)
			{
				AudioAssetPlayable assetPlayable = switchData.Find
					(data => data.OptionIndex == currentSwitchSelectionIndex).AssetPlayable;

				if (assetPlayable != null)
				{
					return assetPlayable;
				}
				else
				{
					Debug.LogWarning
						($"HLBAudioSwitch {name} has no asset associated with switch option {switchGroup.Options[currentSwitchSelectionIndex]}");
					return null;
				}
			}
			
			Debug.LogWarning($"No HLBAudio Assets Present in Assets List of {name}");
			return null;
		}

		public override AudioAssetPlaybackData GeneratePlaybackData()
		{
			AudioAssetPlayable assetPlayable = GetAsset();
			
			AudioAssetPlaybackData assetPlaybackData = assetPlayable.GeneratePlaybackData();
			
			assetPlaybackData.ApplyAudioPlaybackParameters(playbackParameters);

			return assetPlaybackData;
		}
	}
}