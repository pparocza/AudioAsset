using UnityEngine;

namespace AudioAsset
{
	[System.Serializable]
	public class AudioAssetStateTransition
	{
		[SerializeField] 
		private int state1Index;
		public int State1Index => state1Index;

		[SerializeField] 
		private int state2Index;
		public int State2Index => state2Index;
		
		[SerializeField]
		private float transitionTime = 0.0f;
		public float TransitionTime => transitionTime;
	}
}