using Android.Util;
using Firebase.Messaging;
using Android.Support.V4.App;
using WindowsAzure.Messaging;
using Android.App;
using System.Linq;
using Android.Content;
using System.Collections.Generic;

namespace XamarinEvolve.Droid.Notifications
{
	[Service]
	[IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
	[IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
	public class EvolveFirebaseMessagingService : FirebaseMessagingService
	{
		const string TAG = "EvolveFirebaseMessagingService";
		NotificationHub hub;

		public override void OnMessageReceived(RemoteMessage message)
		{
			Log.Debug(TAG, "From: " + message.From);
			if (message.GetNotification() != null)
			{
				//These is how most messages will be received
				Log.Debug(TAG, "Notification Message Body: " + message.GetNotification().Body);
				SendNotification(message.GetNotification().Body);
			}
			else
			{
				//Only used for debugging payloads sent from the Azure portal
				SendNotification(message.Data.Values.First());

			}
		}

		void SendNotification(string messageBody)
		{
			var intent = new Intent(this, typeof(MainActivity));
			intent.AddFlags(ActivityFlags.ClearTop);
			var pendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.OneShot);

			var notificationBuilder = new NotificationCompat.Builder(this, MainActivity.CHANNEL_ID);

			notificationBuilder.SetContentTitle("Techorama Notification")
						.SetSmallIcon(Resource.Drawable.ic_launcher)
						.SetContentText(messageBody)
						.SetAutoCancel(true)
						.SetShowWhen(false)
						.SetContentIntent(pendingIntent);

			var notificationManager = NotificationManager.FromContext(this);

			notificationManager.Notify(0, notificationBuilder.Build());
		}

		public override void OnNewToken(string token)
		{
			Log.Debug(TAG, "FCM token: " + token);
			SendRegistrationToServer(token);
		}

		void SendRegistrationToServer(string token)
		{
			// Register with Notification Hubs
			hub = new NotificationHub(Constants.NotificationHubName,
										Constants.ListenConnectionString, this);

			var tags = new List<string>() { };
			var regID = hub.Register(token, tags.ToArray()).RegistrationId;

			Log.Debug(TAG, $"Successful registration of ID {regID}");
		}
	}
}
