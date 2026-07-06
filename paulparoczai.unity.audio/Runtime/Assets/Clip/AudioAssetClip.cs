using UnityEngine;

namespace AudioAsset
{
	public class AudioAssetClip : AudioAssetPlayable
	{
		[SerializeField] private AudioClip audioClip;
		public AudioClip AudioClip
		{
			get => audioClip;
			set => audioClip = value;
		}

		public override AudioAssetPlaybackData GeneratePlaybackData()
		{
			AudioAssetPlaybackData assetPlaybackData = new();
			
			assetPlaybackData.SetAudioClip(audioClip);
			assetPlaybackData.ApplyAudioPlaybackParameters(playbackParameters);

			return assetPlaybackData;
		}
	}
}