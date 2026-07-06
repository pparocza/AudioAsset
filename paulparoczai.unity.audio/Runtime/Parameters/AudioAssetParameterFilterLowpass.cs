namespace AudioAsset
{
	[System.Serializable]
	public class AudioAssetParameterFilterLowpass : AudioAssetParameterFilter
	{
		public override AudioAssetParameterType ParameterType => AudioAssetParameterType.FilterLowpass;
	}
}