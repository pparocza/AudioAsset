using UnityEngine;

namespace AudioAsset
{
	[System.Serializable]
	public abstract class AudioAssetParameterFilter : AudioAssetParameter
	{
		[SerializeField, Range(0, 100)]
		private int cutoff;
		
		[SerializeField, Range(-100, 100)]
		private int minOffset;
		
		[SerializeField, Range(-100, 100)]
		private int maxOffset;

		public override float GetValue()
		{
			if (!randomize)
			{
				return cutoff;
			}

			return GetRandomValue(cutoff, minOffset, maxOffset);
		}
	}
}