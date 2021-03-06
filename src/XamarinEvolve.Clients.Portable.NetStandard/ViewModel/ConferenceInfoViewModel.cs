﻿using System;
using System.Windows.Input;
using System.Threading.Tasks;
using Xamarin.Forms;
using FormsToolkit;
using Plugin.Share;
using System.Net.Http;
using Plugin.Connectivity;
using Newtonsoft.Json;
using XamarinEvolve.Utils;
using XamarinEvolve.Utils.Helpers;

namespace XamarinEvolve.Clients.Portable
{
    public class WiFiRoot
    {
        public string SSID { get; set; }
        public string Password { get; set; }
    }

    public class ConferenceInfoViewModel : ViewModelBase
    {
		public string PrivacyPolicyTitle => "Privacy policy";
        public string PrivacyPolicyLabelText => $"Read the {EventInfo.EventName} privacy policy";

		public string CodeOfConductPageTitle => AboutThisApp.CodeOfConductPageTitle;

        IWiFiConfig wiFiConfig;
        public ConferenceInfoViewModel()
        {
            wiFiConfig = Locator.Get<IWiFiConfig>();
        }

        protected override void UpdateCommandCanExecute()
        {
            configureWiFiCommand?.ChangeCanExecute();
            copyPasswordCommand?.ChangeCanExecute();
        }

        public async Task<bool> UpdateConfigs()
        {
			if (IsBusy || !FeatureFlags.WifiEnabled)
                return false;


            try 
            {
                IsBusy = true;
                try 
                {
                    if (CrossConnectivity.Current.IsConnected) 
                    {
						using (var client = HttpClientFactory.CreateClient(Locator))
						{
							client.Timeout = TimeSpan.FromSeconds(5);
							var json = await client.GetStringAsync(EventInfo.WifiUrl);
							var root = JsonConvert.DeserializeObject<WiFiRoot>(json);
							Settings.WiFiSSID = root.SSID;
							Settings.WiFiPass = root.Password;
						}
                    }
                } 
                catch (Exception ex)
                {
                    ex.Data["Method"] = "RetrieveWifiConfig";
					Logger.Report(ex);
				}

                try 
                {
                    if (wiFiConfig != null)
                    {
                        WiFiConfigured = wiFiConfig.IsConfigured(Settings.WiFiSSID);
                    }
                } 
                catch (Exception ex) 
                {
                    ex.Data ["Method"] = "UpdateConfigs";
                    Logger.Report (ex);
                    return false;
                }

            } 
            finally 
            {
                IsBusy = false;
            }

            return true;
        }

		public bool WiFiEnabled => FeatureFlags.WifiEnabled;

        bool wiFiConfigured;
        public bool WiFiConfigured
        {
            get { return wiFiConfigured; }
            set { SetProperty(ref wiFiConfigured, value); }
        }

        Command configureWiFiCommand;
        public ICommand ConfigureWiFiCommand =>
            configureWiFiCommand ?? (configureWiFiCommand = new Command(ExecuteConfigureWiFiCommand)); 

        void ExecuteConfigureWiFiCommand()
        {
            if(wiFiConfig == null)
                return;

            Logger.Track(EvolveLoggerKeys.WiFiConfig, "Type", "2.4Ghz");

            if (!wiFiConfig.ConfigureWiFi(Settings.WiFiSSID, Settings.WiFiPass))
            {
                WiFiConfigured = false;
                SendWiFiError();
            }
            else
            {
                WiFiConfigured = true;
            }
        }

        void SendWiFiError()
        {
            MessagingService.Current.SendMessage<MessagingServiceAlert>(MessageKeys.Message,
                new MessagingServiceAlert
                {
                    Title="Wi-Fi Configuration",
                    Message ="Unable to configure WiFi, you may have to configure manually or try again.",
                    Cancel = "OK"
                });
        }

        Command  copyPasswordCommand;
        public ICommand CopyPasswordCommand =>
            copyPasswordCommand ?? (copyPasswordCommand = new Command<string>((t) => ExecuteCopyPasswordAsync(t).IgnoreResult(ShowError))); 

        async Task ExecuteCopyPasswordAsync(string pass)
        {
            Logger.Track(EvolveLoggerKeys.CopyPassword);
            await CrossShare.Current.SetClipboardText(pass, "Password");
            Toast.SendToast("Password Copied");
        }
    }
}

