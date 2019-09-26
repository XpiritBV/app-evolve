using System;
using System.Collections.Generic;

using Xamarin.Forms;
using XamarinEvolve.Clients.Portable;
using FormsToolkit;
using XamarinEvolve.DataObjects;
using XamarinEvolve.Utils;

namespace XamarinEvolve.Clients.UI
{
	public partial class FeedPage : BasePage
	{
		public override AppPage PageType => AppPage.Feed;

        FeedViewModel ViewModel => vm ?? (vm = BindingContext as FeedViewModel);

		FeedViewModel vm;
        DateTime favoritesTime;
        string loggedIn;
        public FeedPage()
        {
            InitializeComponent();
            loggedIn = Settings.Current.UserIdentifier;
            BindingContext = new FeedViewModel();
            
            favoritesTime = Settings.Current.LastFavoriteTime;
            ViewModel.Tweets.CollectionChanged += (sender, e) => 
                {
                    var adjust = Device.RuntimePlatform != Device.Android ? 1 : -ViewModel.Tweets.Count + 2;
                    ListViewSocial.HeightRequest = (ViewModel.Tweets.Count * ListViewSocial.RowHeight)  - adjust;
                };

            ViewModel.Sessions.CollectionChanged += (sender, e) => 
                {
                    var adjust = Device.RuntimePlatform != Device.Android ? 1 : -ViewModel.Sessions.Count + 1;
                    ListViewSessions.HeightRequest = (ViewModel.Sessions.Count * ListViewSessions.RowHeight) - adjust;
                };

            ListViewSessions.ItemTapped += (sender, e) => ListViewSessions.SelectedItem = null;
            ListViewSessions.ItemSelected += async (sender, e) => 
                {
                    var session = ListViewSessions.SelectedItem as Session;
                    if(session == null)
                        return;
                    var sessionDetails = new SessionDetailsPage(session);

                    await NavigationService.PushAsync(Navigation, sessionDetails);
                    ListViewSessions.SelectedItem = null;
                }; 

            NotificationStack.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(() => NavigationService.PushAsync(Navigation, new NotificationsPage()).IgnoreResult(vm.ShowError))
                });
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            UpdatePage();

            MessagingService.Current.Subscribe<string>(MessageKeys.NavigateToImage, (m, image) =>
                {
                    NavigationService.PushModalAsync(Navigation, new EvolveNavigationPage(new TweetImagePage(image))).IgnoreResult(vm.ShowError);
                });

			MessagingService.Current.Subscribe(MessageKeys.NavigateToConferenceFeedback, (m) =>
				{
                    NavigationService.PushModalAsync(Navigation, new EvolveNavigationPage(new ConferenceFeedbackPage())).IgnoreResult(vm.ShowError);
				});
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MessagingService.Current.Unsubscribe<string>(MessageKeys.NavigateToImage);
        }

        bool firstLoad = true;
        private void UpdatePage()
        {
            bool forceRefresh = (Clock.Now > (ViewModel?.NextForceRefresh ?? Clock.Now)) ||
                    loggedIn != Settings.Current.UserIdentifier;

            loggedIn = Settings.Current.UserIdentifier;

			vm.EvaluateVisualState();

            if (forceRefresh)
            {
                ViewModel.RefreshCommand.Execute(null);
                favoritesTime = Settings.Current.LastFavoriteTime;
            }
            else
            {

                if (ViewModel.Tweets.Count == 0)
                {

                    ViewModel.LoadSocialCommand.Execute(null);
                }

                if ((firstLoad && ViewModel.Sessions.Count == 0) || favoritesTime != Settings.Current.LastFavoriteTime)
                {
                    if (firstLoad)
                        Settings.Current.LastFavoriteTime = Clock.Now;
                    
                    firstLoad = false;
                    favoritesTime = Settings.Current.LastFavoriteTime;
                    ViewModel.LoadSessionsCommand.Execute(null);
                }

                if (ViewModel.Notification == null)
                    ViewModel.LoadNotificationsCommand.Execute(null);
            }

        }


        public void OnResume()
        {
            UpdatePage();
        }

    }
}

