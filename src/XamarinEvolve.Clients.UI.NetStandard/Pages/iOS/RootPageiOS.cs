using Xamarin.Forms;
using XamarinEvolve.Clients.UI;
using FormsToolkit;
using XamarinEvolve.Clients.Portable;
using XamarinEvolve.DataStore.Abstractions;
using XamarinEvolve.Utils;
using System.Linq;
using System;

namespace XamarinEvolve.Clients.UI
{
    public class RootPageiOS : TabbedPage
    {
        public RootPageiOS()
        {

            NavigationPage.SetHasNavigationBar(this, false);
			

			this.UnselectedTabColor = ((Color)Xamarin.Forms.Application.Current.Resources["Accent"]);
			this.SelectedTabColor = ((Color)Xamarin.Forms.Application.Current.Resources["SecondaryAccent"]);
			this.BarTextColor = ((Color)Xamarin.Forms.Application.Current.Resources["Accent"]);
			this.BarBackgroundColor = ((Color)Xamarin.Forms.Application.Current.Resources["Primary"]);

			Children.Add(new EvolveNavigationPage(new FeedPage()));
			Children.Add(new EvolveNavigationPage(new SessionsPage()));
			if (FeatureFlags.SpeakersEnabled)
			{
				Children.Add(new EvolveNavigationPage(new SpeakersPage()));
			}
			if (FeatureFlags.EventsEnabled)
			{
				Children.Add(new EvolveNavigationPage(new EventsPage()));
			}
			if (FeatureFlags.SponsorsOnTabPage)
			{
				Children.Add(new EvolveNavigationPage(new SponsorsPage()));
			}
			Children.Add(new EvolveNavigationPage(new AboutPage()));

			MessagingService.Current.Subscribe<DeepLinkPage>("DeepLinkPage", async (m, p) =>
				{
					switch (p.Page)
					{
						case AppPage.Notification:
							Navigate(AppPage.Feed);
							Navigate(AppPage.Notification);
							await CurrentPage.Navigation.PopToRootAsync();
							await CurrentPage.Navigation.PushAsync(new NotificationsPage());
							break;
						case AppPage.Sessions:
							Navigate(AppPage.Sessions);
							await CurrentPage.Navigation.PopToRootAsync();
							break;
						case AppPage.Events:
							Navigate(AppPage.Events);
							await CurrentPage.Navigation.PopToRootAsync();
							break;
						case AppPage.MiniHacks:
							Navigate(AppPage.MiniHacks);
							await CurrentPage.Navigation.PopToRootAsync();
							break;
						case AppPage.Session:
							Navigate(AppPage.Sessions);
							await CurrentPage.Navigation.PopToRootAsync();
							var session = await DependencyService.Get<ISessionStore>().GetAppIndexSession(p.Id);
							if (session == null)
								break;
							await CurrentPage.Navigation.PushAsync(new SessionDetailsPage(session));
							break;
						case AppPage.Speaker:
							Navigate(AppPage.Speakers);
							await CurrentPage.Navigation.PopToRootAsync();
							var speaker = await DependencyService.Get<ISpeakerStore>().GetAppIndexSpeaker(p.Id);
							if (speaker == null)
								break;

							ContentPage destination;
							destination = new SpeakerDetailsPage(speaker);
							await CurrentPage.Navigation.PushAsync(destination);
							break;
					}
				});
		}

        public void Navigate(AppPage menuId)
        {
            var page = Children
                        .OfType<EvolveNavigationPage>()
                        .FirstOrDefault(n => n.CurrentPage is IProvidePageInfo && ((IProvidePageInfo)n.CurrentPage).PageType == menuId);

            if (page != null)
            {
                CurrentPage = page;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (Settings.Current.FirstRun && !FeatureFlags.LoginEnabled)
            {
                App.CurrentApplication.PerformAnonymousLogin().IgnoreResult(ReportError);
            }
        }

        void ReportError(Exception e)
        {
            DependencyService.Get<ILogger>()?.Report(e);
        }
    }
}


