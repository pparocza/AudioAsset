namespace AudioAsset
{
	public class AudioAssetRTPCBinding
	{
		public AudioAssetParameterType ParameterType;
		public AudioAssetRTPCData rTPCData;

		public float Value => rTPCData.Curve.Evaluate(rTPCData.RTPC.GetValue());
	}
}