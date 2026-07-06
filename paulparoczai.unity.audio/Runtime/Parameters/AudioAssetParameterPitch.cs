using UnityEngine;

namespace AudioAsset
{
	[System.Serializable]
	public class AudioAssetParameterPitch : AudioAssetParameter
	{
		[SerializeField, Range(-3600, 3600)]
		private int pitch;
		
		[SerializeField, Range(-1200, 1200)]
		private int minOffset;
		
		[SerializeField, Range(-1200, 1200)]
		private int maxOffset;
		
		public override AudioAssetParameterType ParameterType => AudioAssetParameterType.Pitch;

		public override float GetValue()
		{
			if (!randomize)
			{
				return pitch;
			}

			return GetRandomValue(pitch, minOffset, maxOffset);
		}
	}
}