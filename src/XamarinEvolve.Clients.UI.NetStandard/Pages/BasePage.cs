using System;
using Xamarin.Forms;
using XamarinEvolve.Clients.Portable;
using XamarinEvolve.Utils;

namespace XamarinEvolve.Clients.UI
{
	public abstract class BasePage : ContentPage, IProvidePageInfo, IDisposable
	{
		private DateTime _appeared;

		public abstract AppPage PageType { get; }
		protected string ItemId { get; set; }

		public void Dispose()
		{
			Dispose(true);
		}

		protected void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (BindingContext is ViewModelBase vm)
				{
					vm.Dispose();
				}
			}
		}

		protected override void OnAppearing()
		{
			_appeared = Clock.Now;
			App.Logger.TrackPage(PageType.ToString(), ItemId);

            ((ViewModelBase)BindingContext)?.Activate();

			base.OnAppearing();
		}

		protected override void OnDisappearing()
		{
			App.Logger.TrackTimeSpent(PageType.ToString(), ItemId, Clock.Now - _appeared);
            ((ViewModelBase)BindingContext)?.Deactivate();
			base.OnDisappearing();
		}
	}
}

