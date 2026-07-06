using UnityEngine;

namespace AudioAsset
{
	[System.Serializable]
	public struct AudioAssetStateAudioParameters
	{
		[SerializeField, Range(-96, 6)] 
		private int volume;
		public int Volume => volume;

		[SerializeField, Range(-3600, 3600)] 
		private int pitch;
		public int Pitch => pitch;

		[SerializeField, Range(-100, 100)] 
		private int highpass;
		public int Highpass => highpass;
		
		[SerializeField, Range(-100, 100)] 
		private int lowpass;
		public int Lowpass => lowpass;
	}
}