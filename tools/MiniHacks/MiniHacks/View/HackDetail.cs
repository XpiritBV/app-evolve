using System;
using MiniHacks.Helpers;
using MiniHacks.Model;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;

namespace MiniHacks.View
{
    public class HackDetail : ContentPage
    {
        ZXingBarcodeImageView barcode;
        MiniHack _hack;
        Label _finishedCount;

        public HackDetail(MiniHack hack)
        {
            _hack = hack;

            Title = hack.Name;
            barcode = new ZXingBarcodeImageView
            {
                HeightRequest = 300,
                WidthRequest = 300,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                BackgroundColor = Color.Transparent
            };
            barcode.BarcodeFormat = ZXing.BarcodeFormat.QR_CODE;
            barcode.BarcodeOptions.Width = 300;
            barcode.BarcodeOptions.Height = 300;
            barcode.BarcodeOptions.Margin = 10;
            barcode.BarcodeValue = hack.UnlockCode;

            _finishedCount = new Label
            {
                FontSize = 20,
                HorizontalOptions = LayoutOptions.Center
            };

            //var button = new Button
            //{
            //    Text = "Increase Count"
            //};

            //button.Clicked += (sender, e) =>
            //{
            //    Settings.UpdateCount(hack.Id);
            //    label.Text = "Finished: " + Settings.GetCount(hack.Id);
            //};

            Content = new StackLayout
            {
                Padding = 10,
                Children =
                {
                    _finishedCount,
                    barcode
                }
            };
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            var count = await GetFinishedCount();

            if (count >= 0)
            {
                _finishedCount.Text = $"Finished {count} times";
            }
        }

        public async Task<int> GetFinishedCount()
        {
            if (IsBusy)
                return -1;

            try
            {
                IsBusy = true;

                var count = -1;

                using (var client = new HttpClient())
                {
                    var json = await client.GetStringAsync($"https://techdays-2016.azurewebsites.net/api/CompleteMiniHack/{_hack.Id}?ZUMO-API-VERSION=2.0.0").ConfigureAwait(false);

                    count = JsonConvert.DeserializeObject<int>(json);
                }

                return count;
            }
            catch (Exception e)
            {
                var inner = e;
                while (inner != null)
                {
                    System.Diagnostics.Debug.WriteLine(inner.ToString());
                    inner = inner.InnerException;
                }
                return -1;
            }
            finally
            {
                Device.BeginInvokeOnMainThread(() => IsBusy = false);
            }
        }
    }
}


