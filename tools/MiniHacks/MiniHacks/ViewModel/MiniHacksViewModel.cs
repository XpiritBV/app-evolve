using System;
using MvvmHelpers;
using MiniHacks.Model;
using System.Windows.Input;
using System.Threading.Tasks;
using Xamarin.Forms;
using MiniHacks.Helpers;
using System.Net.Http;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MiniHacks.ViewModel
{
    public class MiniHacksViewModel : BaseViewModel
    {
        Page page;
        public MiniHacksViewModel(Page page)
        {
            this.page = page;
        }

        public ObservableRangeCollection<MiniHack> MiniHacks { get; } = new ObservableRangeCollection<MiniHack>();

        bool noHacksFound;
        public bool NoHacksFound
        {
            get { return noHacksFound; }
            set { SetProperty(ref noHacksFound, value); }
        }


        ICommand forceRefreshCommand;
        public ICommand ForceRefreshCommand =>
            forceRefreshCommand ?? (forceRefreshCommand = new Command(async () => await ExecuteForceRefreshCommandAsync()));

        async Task ExecuteForceRefreshCommandAsync()
        {
            await ExecuteLoadMiniHacksAsync(true);
        }



        ICommand loadMiniHacksCommand;
        public ICommand LoadMiniHacksCommand =>
        loadMiniHacksCommand ?? (loadMiniHacksCommand = new Command<bool>(async (f) => await ExecuteLoadMiniHacksAsync()));

        async Task<bool> ExecuteLoadMiniHacksAsync(bool force = false)
        {
            if (IsBusy)
                return false;

            try
            {
                IsBusy = true;
                NoHacksFound = false;


                var json = Settings.JsonFile;

                if (force || string.IsNullOrWhiteSpace(json))
                {
                    using (var client = new HttpClient())
                    {
						json = await client.GetStringAsync("https://techdays-2016.azurewebsites.net/tables/minihack?ZUMO-API-VERSION=2.0.0").ConfigureAwait(false);
                    }
                }

                var finalHacks = JsonConvert.DeserializeObject<List<MiniHack>>(json);

                Device.BeginInvokeOnMainThread(() =>
                {
                    MiniHacks.ReplaceRange(finalHacks);

                    NoHacksFound = MiniHacks.Count == 0;
				});
            }
            catch (Exception ex)
            {
                var inner = ex;
                while (inner != null)
                {
                    System.Diagnostics.Debug.WriteLine(inner.ToString());
                    inner = inner.InnerException;
                }
                await page.DisplayAlert("Error", "Unable to load hacks, please check internet. Error: " + ex.Message, "OK");
            }
            finally
            {
                Device.BeginInvokeOnMainThread(() => IsBusy = false);
            }

            return true;
        }
    }
}

