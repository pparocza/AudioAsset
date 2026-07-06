using UnityEngine;

namespace AudioAsset
{
	[System.Serializable]
	public abstract class AudioAssetParameter : IAudioAssetParameter
	{
		[SerializeField]
		private AudioAssetRTPCData rTPC;
		public AudioAssetRTPCData Rtpc => rTPC;
		
		[SerializeField]
		protected bool randomize;

		protected float parameterValue;
		
		public abstract AudioAssetParameterType ParameterType { get; }
		
		public abstract float GetValue();
		protected float GetRandomValue(float value, float minOffset, float maxOffset) => 
			value + Random.Range(minOffset, maxOffset);

		public void SetValue(float value)
		{
			parameterValue = value;
		}
	}
}