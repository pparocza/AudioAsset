using UnityEngine;
using System.Collections.Generic;

namespace AudioAsset
{
	public class AudioAssetGroup : AudioAssetPlayable
	{
		[SerializeField] private AudioAssetSelectionType selectionType;
		[SerializeField] private List<AudioAssetPlayable> assets = new ();
		public List<AudioAssetPlayable> Assets => assets;

		private AudioAssetPlayable GetAssetRandom() => assets[Random.Range(0, assets.Count)];
		
		private int sequenceIndex = 0;
		private AudioAssetPlayable GetAssetSequence()
		{
			AudioAssetPlayable asset = assets[sequenceIndex];
			sequenceIndex = (sequenceIndex + 1) % assets.Count;
			return asset;
		}
		
		private void OnEnable()
		{
			sequenceIndex = 0;
		}

		private AudioAssetPlayable GetAsset()
		{
			if (assets.Count > 0)
			{
				return SelectAsset();
			}
			
			Debug.LogWarning($"No HLBAudio Assets Present in Assets List of {name}");
			return null;
		}

		private AudioAssetPlayable SelectAsset() => selectionType switch
		{
			AudioAssetSelectionType.Random => GetAssetRandom(),
			AudioAssetSelectionType.Sequence => GetAssetSequence(),
			_ => GetAssetRandom(),
		};

		public override AudioAssetPlaybackData GeneratePlaybackData()
		{
			AudioAssetPlayable asset = GetAsset();

			AudioAssetPlaybackData playbackData = asset.GeneratePlaybackData();
			
			playbackData.ApplyAudioPlaybackParameters(playbackParameters);

			return playbackData;
		}
	}
}