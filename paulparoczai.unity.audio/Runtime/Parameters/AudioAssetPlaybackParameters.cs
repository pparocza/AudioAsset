using UnityEngine;

namespace AudioAsset
{
	[System.Serializable]
	public class AudioAssetPlaybackParameters
	{		
		[SerializeField] 
		private bool loop;
		public bool Loop => loop;

		[SerializeField]
		private AudioAssetParameterVolume volume;
		public float Volume => volume.GetValue();
		public AudioAssetParameterVolume VolumeParameter => volume;

		[SerializeField] private AudioAssetParameterPitch pitch;
		public float Pitch => pitch.GetValue();
		public AudioAssetParameterPitch PitchParameter => pitch;
		
		[SerializeField] private AudioAssetParameterFilterHighpass highpass;
		public int Highpass => (int) highpass.GetValue();
		public AudioAssetParameterFilterHighpass HighpassParameter => highpass;
		
		[SerializeField] private AudioAssetParameterFilterLowpass lowpass;
		public int Lowpass => (int) lowpass.GetValue();
		public AudioAssetParameterFilterLowpass LowpassParameter => lowpass;

		[SerializeField] private float delay;
		public float Delay => delay;

		public AudioAssetPlaybackData GetPlaybackData()
		{
			AudioAssetPlaybackData playbackData = new ();
			playbackData.ApplyAudioPlaybackParameters(this);

			return playbackData;
		}
	}
}