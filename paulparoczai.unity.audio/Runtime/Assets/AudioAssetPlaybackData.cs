using System.Collections.Generic;
using UnityEngine;

namespace AudioAsset
{
	public class AudioAssetPlaybackData
	{
		private AudioClip audioClip;
		public AudioClip AudioClip => audioClip;
		
		private bool loop;
		public bool Loop => loop;

		private float volume;
		public float Volume => volume;
		
		private float pitch;
		public float Pitch => pitch;

		private int highpassAmount;
		public  int HighpassAmount => highpassAmount;

		private int lowpassAmount;
		public int LowpassAmount => lowpassAmount;

		private float fadeInTime;
		public float FadeInTime => fadeInTime;

		private float fadeOutTime;
		public float FadeOutTime => fadeOutTime;

		private float delay;
		public float Delay => delay;

		private List<AnimationCurve> envelopes = new ();

		public void SetAudioClip(AudioClip clip)
		{
			audioClip = clip;
		}

		public void ApplyAudioPlaybackParameters(AudioAssetPlaybackParameters playbackParameters)
		{
			loop = playbackParameters.Loop;
			
			volume += playbackParameters.Volume;
			pitch += playbackParameters.Pitch;
			lowpassAmount += playbackParameters.Lowpass;
			highpassAmount += playbackParameters.Highpass;

			fadeInTime = playbackParameters.VolumeParameter.FadeInTime;
			fadeOutTime = playbackParameters.VolumeParameter.FadeOutTime;
			delay = playbackParameters.Delay;
		}
	}
}