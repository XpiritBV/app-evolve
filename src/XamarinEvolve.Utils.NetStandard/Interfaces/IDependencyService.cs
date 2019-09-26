namespace XamarinEvolve.Utils
{
	public interface IDependencyService
	{
		T Get<T>() where T : class;
	}
}
