using UnityEngine;

namespace AudioAsset
{
	public interface IAudioAssetPlayable
	{
		void Play(Transform parent = null);
		void Stop(Transform parent = null);
		AudioAssetPlaybackData GeneratePlaybackData();

#if UNITY_EDITOR
		void PlayInEditor(AudioAssetPlaybackData playbackData);
#endif
	}
}