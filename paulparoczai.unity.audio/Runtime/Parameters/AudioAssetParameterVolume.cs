using UnityEngine;

namespace AudioAsset
{
	[System.Serializable]
	public class AudioAssetParameterVolume : AudioAssetParameter
	{
		[SerializeField, Range(-96, 6)]
		private int volume;

		[SerializeField, Range(-36, 36)]
		private int minOffset;
		
		[SerializeField, Range(-36, 36)]
		private int maxOffset;
		
		[SerializeField] 
		private float fadeInTime;
		public float FadeInTime => fadeInTime;

		[SerializeField] 
		private float fadeOutTime;
		public float FadeOutTime => fadeOutTime;
		
		public override AudioAssetParameterType ParameterType => AudioAssetParameterType.Volume;
		
		public override float GetValue()
		{
			if (!randomize)
			{
				return volume;
			}

			return GetRandomValue(volume, minOffset, maxOffset);
		}
	}
}