using System.Collections.Generic;
using UnityEngine;

namespace AudioAsset
{
	public class AudioAssetEmitter : MonoBehaviour
	{
		[SerializeField]
		private List<AudioAssetPlayable> assets;

		private void Awake()
		{
			if (assets == null || assets.Count <= 0)
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