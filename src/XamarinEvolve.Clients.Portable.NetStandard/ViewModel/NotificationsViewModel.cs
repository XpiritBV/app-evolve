using System;
using Xamarin.Forms;
using MvvmHelpers;
using XamarinEvolve.DataObjects;
using System.Linq;
using System.Windows.Input;
using System.Threading.Tasks;
using FormsToolkit;
using XamarinEvolve.Utils;

namespace XamarinEvolve.Clients.Portable
{
	public class NotificationsViewModel : ViewModelBase
	{
		public NotificationsViewModel() : base()
		{

		}

        protected override void UpdateCommandCanExecute()
        {
            forceRefreshCommand?.ChangeCanExecute();
            loadNotificationsCommand?.ChangeCanExecute();
        }

		public ObservableRangeCollection<Notification> Notifications { get; } = new ObservableRangeCollection<Notification>();
		public ObservableRangeCollection<Grouping<string, Notification>> NotificationsGrouped { get; } = new ObservableRangeCollection<Grouping<string, Notification>>();

		void SortNotifications()
		{

			var groups = from notification in Notifications
						 orderby notification.Date descending
						 group notification by notification.Date.GetSortName()
				         into notificationGroup
						 select new Grouping<string, Notification>(notificationGroup.Key, notificationGroup);

			NotificationsGrouped.ReplaceRange(groups);
		}

		Command forceRefreshCommand;
		public ICommand ForceRefreshCommand =>
            forceRefreshCommand ?? (forceRefreshCommand = new Command(() => ExecuteForceRefreshCommandAsync().IgnoreResult(ShowError), () => !IsBusy));

		async Task ExecuteForceRefreshCommandAsync()
		{
			await ExecuteLoadNotificationsAsync(true);
		}

		Command loadNotificationsCommand;
		public ICommand LoadNotificationsCommand =>
            loadNotificationsCommand ?? (loadNotificationsCommand = new Command<bool>((f) => ExecuteLoadNotificationsAsync().IgnoreResult(ShowError), (arg) => !IsBusy));

		async Task<bool> ExecuteLoadNotificationsAsync(bool force = false)
		{
			if (IsBusy)
				return false;

			try
			{
				IsBusy = true;
				Notifications.ReplaceRange(await StoreManager.NotificationStore.GetItemsAsync(force));
				SortNotifications();
			}
			catch (Exception ex)
			{
				Logger.Report(ex, "Method", "ExecuteLoadNotificationsAsync");
				MessagingService.Current.SendMessage(MessageKeys.Error, ex);
			}
			finally
			{
				IsBusy = false;
			}

			return true;
		}
	}
}

