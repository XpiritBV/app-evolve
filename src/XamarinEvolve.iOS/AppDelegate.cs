﻿using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;
using XamarinEvolve.Clients.UI;
using Xamarin.Forms;
using FormsToolkit.iOS;
using Xamarin.Forms.Platform.iOS;
using Xamarin;
using FormsToolkit;
using XamarinEvolve.Clients.Portable;
using WindowsAzure.Messaging;
using Refractored.XamForms.PullToRefresh.iOS;
using Social;
using CoreSpotlight;
using XamarinEvolve.DataStore.Abstractions;
using System.Threading.Tasks;
using Google.AppIndexing;
using NotificationCenter;
using XamarinEvolve.Utils;
using System.Diagnostics;
using UserNotifications;

namespace XamarinEvolve.iOS
{
    [Register("AppDelegate")]
    public class AppDelegate : FormsApplicationDelegate
    {
        public static class ShortcutIdentifier
        {
            public const string Share = AboutThisApp.PackageName + ".share";
            public const string Announcements = AboutThisApp.PackageName + ".announcements";
            public const string Events = AboutThisApp.PackageName + ".events";
            public const string MiniHacks = AboutThisApp.PackageName + ".minihacks";
			public const string Sessions = AboutThisApp.PackageName + ".sessions";
		}

        internal static UIColor PrimaryColor;

        public override bool FinishedLaunching(UIApplication uiApplication, NSDictionary launchOptions)
        {

            Forms.Init();

            SetMinimumBackgroundFetchInterval();
			

			InitializeDependencies();

            LoadApplication(new App());

           
            // Process any potential notification data from launch
            ProcessNotification(launchOptions);

            NSNotificationCenter.DefaultCenter.AddObserver(UIApplication.DidBecomeActiveNotification, DidBecomeActive);

			var shouldPerformAdditionalDelegateHandling = true;

			//// Get possible shortcut item
			if (launchOptions != null)
			{
				LaunchedShortcutItem = launchOptions[UIApplication.LaunchOptionsShortcutItemKey] as UIApplicationShortcutItem;
				shouldPerformAdditionalDelegateHandling = (LaunchedShortcutItem == null);
			}

			RegisterForRemoteNotifications();
			return base.FinishedLaunching(uiApplication, launchOptions);// && shouldPerformAdditionalDelegateHandling;
        }

        void DidBecomeActive(NSNotification notification)
        {
            ((XamarinEvolve.Clients.UI.App)Xamarin.Forms.Application.Current).SecondOnResume();
        }

        static void InitializeDependencies()
        {
            FormsMaps.Init();
            Toolkit.Init();

            AppIndexing.SharedInstance.RegisterApp(PublicationSettings.iTunesAppId);

            ZXing.Net.Mobile.Forms.iOS.Platform.Init();

            //Random Inits for Linking out.
            Microsoft.WindowsAzure.MobileServices.CurrentPlatform.Init();
            SQLitePCL.CurrentPlatform.Init();
            Plugin.Share.ShareImplementation.ExcludedUIActivityTypes = new List<NSString>
            {
                UIActivityType.PostToFacebook,
                UIActivityType.AssignToContact,
                UIActivityType.OpenInIBooks,
                UIActivityType.PostToVimeo,
                UIActivityType.PostToFlickr,
                UIActivityType.SaveToCameraRoll
            };
            ImageCircle.Forms.Plugin.iOS.ImageCircleRenderer.Init();
            ZXing.Net.Mobile.Forms.iOS.Platform.Init();
            NonScrollableListViewRenderer.Initialize();
            SelectedTabPageRenderer.Initialize();
            TextViewValue1Renderer.Init();
            PullToRefreshLayoutRenderer.Init();
        }

        public override void WillEnterForeground(UIApplication uiApplication)
        {
            base.WillEnterForeground(uiApplication);
            ((XamarinEvolve.Clients.UI.App)Xamarin.Forms.Application.Current).SecondOnResume();
        }

		void RegisterForRemoteNotifications()
		{
			// register for remote notifications based on system version
			if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
			{
				UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Alert |
					UNAuthorizationOptions.Sound |
					UNAuthorizationOptions.Sound,
					(granted, error) =>
					{
						if (granted)
							InvokeOnMainThread(UIApplication.SharedApplication.RegisterForRemoteNotifications);
					});
			}
			else if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
			{
				var pushSettings = UIUserNotificationSettings.GetSettingsForTypes(
				UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound,
				new NSSet());

				UIApplication.SharedApplication.RegisterUserNotificationSettings(pushSettings);
				UIApplication.SharedApplication.RegisterForRemoteNotifications();
			}
			else
			{
				UIRemoteNotificationType notificationTypes = UIRemoteNotificationType.Alert | UIRemoteNotificationType.Badge | UIRemoteNotificationType.Sound;
				UIApplication.SharedApplication.RegisterForRemoteNotificationTypes(notificationTypes);
			}
		}

		public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
		{
			Debug.WriteLine($"Failed to regsiter for remote notifications {error}");
			//base.FailedToRegisterForRemoteNotifications(application, error);
		}
		public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
		{
			var Hub = new SBNotificationHub(ApiKeys.ListenConnectionString, ApiKeys.NotificationHubName);

			// update registration with Azure Notification Hub
			Hub.UnregisterAllAsync(deviceToken, (error) =>
			{
				if (error != null)
				{
					Debug.WriteLine($"Unable to call unregister {error}");
					return;
				}
			});
			var tags = new NSSet(ApiKeys.NotificationHubTags.ToArray());
			Hub.RegisterNativeAsync(deviceToken, tags, (errorCallback) =>
			{
				if (errorCallback != null)
				{
					Debug.WriteLine($"RegisterNativeAsync error: {errorCallback}");
				}
			});

			var templateExpiration = DateTime.Now.AddDays(120).ToString(System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
			Hub.RegisterTemplateAsync(deviceToken, "defaultTemplate", ApiKeys.APNTemplateBody, templateExpiration, tags, (errorCallback) =>
			{
				if (errorCallback != null)
				{
					if (errorCallback != null)
					{
						Debug.WriteLine($"RegisterTemplateAsync error: {errorCallback}");
					}
				}
			});
		}
		public override void ReceivedRemoteNotification(UIApplication application, NSDictionary userInfo)
        {
            // Process a notification received while the app was already open
            ProcessNotification(userInfo);
        }

        public override bool HandleOpenURL(UIApplication application, NSUrl url)
        {
            if (!string.IsNullOrEmpty(url.AbsoluteString))
            {
                ((App)Xamarin.Forms.Application.Current).SendOnAppLinkRequestReceived(new Uri(url.AbsoluteString));
                return true;
            }
            return false;
        }

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            if (!string.IsNullOrEmpty(url.AbsoluteString))
            {
                ((XamarinEvolve.Clients.UI.App)Xamarin.Forms.Application.Current).SendOnAppLinkRequestReceived(new Uri(url.AbsoluteString));
                return true;
            }
            return false;
        }

        public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
        {
            if (!string.IsNullOrEmpty(url.AbsoluteString))
            {
                ((XamarinEvolve.Clients.UI.App)Xamarin.Forms.Application.Current).SendOnAppLinkRequestReceived(new Uri(url.AbsoluteString));
                return true;
            }
            return false;
        }

        void ProcessNotification(NSDictionary userInfo)
        {
            if (userInfo == null)
                return;

            Console.WriteLine("Received Notification");

            var apsKey = new NSString("aps");

            if (userInfo.ContainsKey(apsKey))
            {
                var alertKey = new NSString("alert");

                var aps = (NSDictionary)userInfo.ObjectForKey(apsKey);

                if (aps.ContainsKey(alertKey))
                {
                    var alert = (NSString)aps.ObjectForKey(alertKey);

                    try
                    {

                        var alertController = UIAlertController.Create($"{EventInfo.EventName} Update", alert, UIAlertControllerStyle.Alert);
                        UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(alertController, true, null);

                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }

                    Console.WriteLine("Notification: " + alert);
                }
            }
        }

        #region Quick Action

        public UIApplicationShortcutItem LaunchedShortcutItem { get; set; }

        public override void OnActivated(UIApplication uiApplication)
        {
            Console.WriteLine("OnActivated");

            // Handle any shortcut item being selected
            HandleShortcutItem(LaunchedShortcutItem);

            // Clear shortcut after it's been handled
            LaunchedShortcutItem = null;
        }

        void CheckForAppLink(NSUserActivity userActivity)
        {
            var link = string.Empty;

            switch (userActivity.ActivityType)
            {
                case "NSUserActivityTypeBrowsingWeb":
                    link = userActivity.WebPageUrl.AbsoluteString;
                    break;
                case "com.apple.corespotlightitem":
                    if (userActivity.UserInfo.ContainsKey(CSSearchableItem.ActivityIdentifier))
                        link = userActivity.UserInfo.ObjectForKey(CSSearchableItem.ActivityIdentifier).ToString();
                    break;
                default:
                    if (userActivity.UserInfo.ContainsKey(new NSString("link")))
                        link = userActivity.UserInfo[new NSString("link")].ToString();
                    break;
            }

            if (!string.IsNullOrEmpty(link))
            {
                ((App)Xamarin.Forms.Application.Current).SendOnAppLinkRequestReceived(new Uri(link));
            }
        }

        // if app is already running
        public override void PerformActionForShortcutItem(UIApplication application, UIApplicationShortcutItem shortcutItem, UIOperationHandler completionHandler)
        {
            Console.WriteLine("PerformActionForShortcutItem");
            // Perform action
            var handled = HandleShortcutItem(shortcutItem);
            completionHandler(handled);
        }

        public bool HandleShortcutItem(UIApplicationShortcutItem shortcutItem)
        {
            Console.WriteLine("HandleShortcutItem ");
            var handled = false;

            // Anything to process?
            if (shortcutItem == null)
                return false;

            // Take action based on the shortcut type
            switch (shortcutItem.Type)
            {
                case ShortcutIdentifier.Share:
                    Console.WriteLine("QUICKACTION: Share");
                    Plugin.Share.CrossShare.Current.Share(new Plugin.Share.Abstractions.ShareMessage { Text = EventInfo.HashTag, Title = EventInfo.EventName});

					// TODO: direct Twitter integration is not supported anymore in iOS 11

					//var slComposer = SLComposeViewController.FromService(SLServiceType.Twitter);
					//if (slComposer == null)
					//{
					//    var alert = UIAlertController.Create("Unavailable", "Twitter is not available, please sign in on your devices settings screen.", UIAlertControllerStyle.Alert);
					//    UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(alert, true, null);
					//}
					//else
					//{
					//    slComposer.SetInitialText(EventInfo.HashTag);
					//    if (slComposer.EditButtonItem != null)
					//    {
					//        slComposer.EditButtonItem.TintColor = PrimaryColor;
					//    }
					//    slComposer.CompletionHandler += (result) =>
					//    {
					//        InvokeOnMainThread(() => UIApplication.SharedApplication.KeyWindow.RootViewController.DismissViewController(true, null));
					//    };

					//    UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewControllerAsync(slComposer, true);
					//}
					handled = true;
                    break;
                case ShortcutIdentifier.Announcements:
                    Console.WriteLine("QUICKACTION: Accouncements");
                    ContinueNavigation(AppPage.Notification);
                    handled = true;
                    break;
                case ShortcutIdentifier.MiniHacks:
                    Console.WriteLine("QUICKACTION: MiniHacks");
                    ContinueNavigation(AppPage.MiniHacks);
                    handled = true;
                    break;
                case ShortcutIdentifier.Events:
                    Console.WriteLine("QUICKACTION: Events");
                    ContinueNavigation(AppPage.Events);
                    handled = true;
                    break;
				case ShortcutIdentifier.Sessions:
					Console.WriteLine("QUICKACTION: Sessions");
                    ContinueNavigation(AppPage.Sessions);
					handled = true;
					break;
			}

            Console.Write(handled);
            // Return results
            return handled;
        }

        void ContinueNavigation(AppPage page, string id = null)
        {
            Console.WriteLine("ContinueNavigation");

            // TODO: display UI in Forms somehow
            System.Console.WriteLine("Show the page for " + page);
            MessagingService.Current.SendMessage<DeepLinkPage>("DeepLinkPage", new DeepLinkPage
            {
                Page = page,
                Id = id
            });
        }

        public override void UserActivityUpdated(UIApplication application, NSUserActivity userActivity)
        {
            CheckForAppLink(userActivity);
        }

        public override bool ContinueUserActivity(UIApplication application, NSUserActivity userActivity, UIApplicationRestorationHandler completionHandler)
        {
            CheckForAppLink(userActivity);
            return true;
        }

        #endregion

        #region Background Refresh

        private void SetMinimumBackgroundFetchInterval()
        {
            UIApplication.SharedApplication.SetMinimumBackgroundFetchInterval(600);
        }

        // Called whenever your app performs a background fetch
        public override async void PerformFetch(UIApplication application, Action<UIBackgroundFetchResult> completionHandler)
        {            
            nint taskID = UIApplication.BackgroundTaskInvalid;
            taskID = application.BeginBackgroundTask(AboutThisApp.PackageName, () =>
            {
                completionHandler(UIBackgroundFetchResult.Failed);
                application.EndBackgroundTask(taskID);
            });

            Console.WriteLine($"Started background task for PerformFetch with ID {taskID}");
            var stopWatch = new Stopwatch();
            var result = UIBackgroundFetchResult.NoData;

            try
            {
                Xamarin.Forms.Forms.Init(); //need for dependency services

                if (EventInfo.EndOfConference.AddDays(AppBehavior.NumberOfDaysAfterConferenceToStopSyncing) < Clock.Now)
                {
                    completionHandler(result);
                    return;
                }

                var logger = DependencyService.Get<ILogger>();
                logger?.Track("StartPerformFetch");
                Console.WriteLine("StartPerformFetch");

                // Do Background Fetch
                var downloadSuccessful = false;
                stopWatch.Start();

                // Download data
                var manager = DependencyService.Get<IStoreManager>();
                downloadSuccessful = await manager.SyncAllAsync(Settings.Current.IsLoggedIn);

                if (downloadSuccessful)
                {
                    result = UIBackgroundFetchResult.NewData;
                    Settings.Current.HasSyncedData = true;
                    Settings.Current.LastSync = Clock.Now;
                    logger?.Track("PerformFetchSucces", "DurationInMilliseconds", stopWatch.ElapsedMilliseconds.ToString());
                }
                else
                {
                    logger?.Track("PerformFetchFailed", "DurationInMilliseconds", stopWatch.ElapsedMilliseconds.ToString());
                }
                Console.WriteLine($"PerformFetch with succes={downloadSuccessful} in {stopWatch.ElapsedMilliseconds}ms");
            }
            catch (Exception ex)
            {
                result = UIBackgroundFetchResult.Failed;
				Console.WriteLine($"Exception during StartPerformFetch: {ex}");
				ex.Data["Method"] = "PerformFetch";
				DependencyService.Get<ILogger>()?.Report(ex);
			}
            finally
            {
				completionHandler(result);
				stopWatch.Stop();
                Console.WriteLine($"PerformFetch ended with {result} in {stopWatch.ElapsedMilliseconds}ms");
                if (taskID != UIApplication.BackgroundTaskInvalid)
                {
                    application.EndBackgroundTask(taskID);
                    Console.WriteLine($"Finished background task for PerformFetch with ID {taskID}");
                }
            }
        }

        #endregion
    }
}

