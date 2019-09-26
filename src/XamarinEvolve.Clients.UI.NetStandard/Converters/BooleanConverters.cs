using System;
using System.Globalization;
using Xamarin.Forms;

namespace XamarinEvolve.Clients.UI
{
	class NotConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return !(bool)value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return !(bool)value;
		}
	}

	class NullConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return !(value == null);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
