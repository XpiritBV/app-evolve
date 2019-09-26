namespace XamarinEvolve.Utils
{
	public interface IAppVersionProvider
	{
		string AppVersion { get; }
		bool SupportsWebRtc { get; }
	}
}
