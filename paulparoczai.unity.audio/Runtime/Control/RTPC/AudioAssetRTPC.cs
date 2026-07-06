using UnityEditor;
using UnityEngine;

namespace AudioAsset
{
	[CreateAssetMenu(fileName = "AudioRTPC", menuName = "Hololabs/Audio/Control/RTPC"), CanEditMultipleObjects]
	public class AudioAssetRTPC : ScriptableObject
	{
		[SerializeField]
		private float value;

		public float GetValue() => value;
		public void SetValue(float inputValue) => value = inputValue;
	}
}