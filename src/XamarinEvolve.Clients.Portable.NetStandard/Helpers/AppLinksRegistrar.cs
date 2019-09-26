using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using XamarinEvolve.Clients.Portable;
using XamarinEvolve.DataObjects;
using XamarinEvolve.DataStore.Abstractions;
using XamarinEvolve.Utils;

[assembly:Dependency(typeof(AppLinksRegistrar))]

namespace XamarinEvolve.Clients.Portable
{
    public class AppLinksRegistrar : IDataIndexer
    {
        private readonly IDependencyService _locator;
        private readonly IImageChecker _imageChecker;

        public AppLinksRegistrar(IDependencyService locator)
        {
            _locator = locator;
            _imageChecker = locator.Get<IImageChecker>();
        }

        public AppLinksRegistrar() : this(new DependencyServiceWrapper()) { }

        public Task RegisterMiniHack(MiniHack hack)
        {
			Device.BeginInvokeOnMainThread(() =>
			{
				if (IsSupported() && FeatureFlags.AppLinksEnabled)
				{
					try
					{
						var appLink = hack.GetAppLink(_imageChecker);
						Application.Current.AppLinks.RegisterLink(appLink);
					}
					catch (Exception applinkException)
					{
						// don't crash the app
						_locator.Get<ILogger>()?.Report(applinkException, "AppLinks.RegisterLink", hack.Id);
					}
				}
			});
			return Task.CompletedTask;
		}

        public Task RegisterSession(Session session)
        {
			Device.BeginInvokeOnMainThread(() =>
			{
				if (IsSupported())
				{
					try
					{
						// data migration: older applinks are removed so the index is rebuilt again
						Application.Current.AppLinks.RegisterLink(session.GetAppLink());
					}
					catch (Exception applinkException)
					{
						// don't crash the app
						_locator.Get<ILogger>()?.Report(applinkException, "AppLinks.RegisterLink", session.Id);
					}
				}
			});
			return Task.CompletedTask;
		}

        public Task RegisterSpeaker(Speaker speaker)
        {
			Device.BeginInvokeOnMainThread(() =>
			{
				if (IsSupported() && FeatureFlags.SpeakersEnabled)
				{
					try
					{
						var appLink = speaker.GetAppLink(_imageChecker);
						Application.Current.AppLinks.RegisterLink(appLink);
					}
					catch (Exception applinkException)
					{
						// don't crash the app
						_locator.Get<ILogger>()?.Report(applinkException, "AppLinks.RegisterLink", speaker.Id);
					}
				}
			});
			return Task.CompletedTask;
		}

        public bool IsSupported()
        {
            return FeatureFlags.AppLinksEnabled
               && Device.RuntimePlatform != Device.WPF
               && Device.RuntimePlatform != Device.UWP;
        }

		
	}
}
