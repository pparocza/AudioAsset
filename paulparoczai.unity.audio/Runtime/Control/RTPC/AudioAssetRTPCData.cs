using UnityEngine;

namespace AudioAsset
{
	[System.Serializable]
	public class AudioAssetRTPCData
	{
		[SerializeField] 
		private bool enable;
		public bool Enable => enable;
		
		[SerializeField] 
		private AudioAssetRTPC rTPC;
		public AudioAssetRTPC RTPC => rTPC;

		[SerializeField] 
		private AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);
		public AnimationCurve Curve => curve;
	}
}