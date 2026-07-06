using UnityEngine;

namespace AudioAsset
{
	public static class AudioAssetUtils
	{
		#region Volume
		public static float AmplitudeToDecibels(float amplitudeValue) => 20.0f * Mathf.Log10(amplitudeValue);
		public static float DecibelsToAmplitude(float decibelValue) => Mathf.Pow(10.0f, decibelValue / 20.0f);
		#endregion

		#region Pitch
		public static int RatioToCents(float ratioValue) => (int) (1200 * ( Mathf.Log(ratioValue) / Mathf.Log(2)));
		public static float CentsToRatio(int centsValue) => Mathf.Pow(2, (float) centsValue / 1200);
		#endregion
		
		#region Filter
		public static int FilterAmountToCutoffFrequency(int filterAmount)
		{
			int lookupIndex = Mathf.Clamp(filterAmount, 0, 100);
			return AudioAssetCutoffFrequencyLookup.Table[lookupIndex];
		}
		#endregion
	}
}