using UIKit;
using Xamarin.Forms;
using XamarinEvolve.Clients.Portable;
using XamarinEvolve.DataObjects;
using XamarinEvolve.iOS;

[assembly: Dependency(typeof(UnlockScavengerHuntObjectFeedbackProvider))]

namespace XamarinEvolve.iOS
{
	public class UnlockScavengerHuntObjectFeedbackProvider : IPlatformActionWrapper<ObjectToFind>
	{
		UINotificationFeedbackGenerator _feedback;

		public void Before(ObjectToFind contextEntity)
		{
			if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
			{
				_feedback = new UINotificationFeedbackGenerator();
				_feedback.Prepare();
			}
		}

		public void Success(ObjectToFind contextEntity)
		{
			_feedback?.NotificationOccurred(UINotificationFeedbackType.Success);
		}

		public void Error(ObjectToFind contextEntity)
		{
			_feedback?.NotificationOccurred(UINotificationFeedbackType.Error);
		}
	}
}
