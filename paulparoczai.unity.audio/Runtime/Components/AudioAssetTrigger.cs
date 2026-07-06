using UnityEngine;
using System.Collections.Generic;

namespace AudioAsset
{
	public class AudioAssetTrigger : MonoBehaviour
	{
		[SerializeField]
		private List<AudioAssetPlayable> assets;

		public void TriggerAudioEvent()
		{
			if (assets == null)
			{
				return;
			}
			
			foreach (AudioAssetPlayable asset in assets)
			{
				asset.Play(transform);
			}
		}
	}
}