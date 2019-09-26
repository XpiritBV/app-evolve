﻿using System;
using Xamarin.Forms;
using XamarinEvolve.Clients.Portable;
using System.Text.RegularExpressions;
using XamarinEvolve.Utils;

namespace XamarinEvolve.Clients.UI
{
	public partial class LoginPage : BasePage
	{
		public override AppPage PageType => AppPage.Login;

        ImageSource placeholder;
        LoginViewModel vm;
        public LoginPage()
        {
            InitializeComponent();
            BindingContext = vm = new LoginViewModel(Navigation);

            if (!Settings.Current.FirstRun)
            {
                Title = "My Account";
                var cancel = new ToolbarItem
                {
                    Text = "Cancel",
                    Command = new Command(async() => 
                            {
                                if(vm.IsBusy)
                                    return;
                                await Navigation.PopModalAsync();
                            })
                };
                ToolbarItems.Add(cancel);

                if (Device.RuntimePlatform != Device.iOS)
                    cancel.IconImageSource = "toolbar_close.png";
            }
            
            CircleImageAvatar.Source = placeholder = ImageSource.FromFile("profile_generic_big.png");
            EntryEmail.TextChanged += (sender, e) => 
                {
					var isValid = (Regex.IsMatch(e.NewTextValue, AppBehavior.EmailRegex, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250)));
                    if(isValid)
                    {
                        Device.BeginInvokeOnMainThread(()=>
                            {
                                CircleImageAvatar.BorderThickness = 3;
                                CircleImageAvatar.Source = ImageSource.FromUri(new Uri(Gravatar.GetURL(EntryEmail.Text)));
                            });

                    }
                    else if(CircleImageAvatar.Source != placeholder)
                    {
                        Device.BeginInvokeOnMainThread(()=>
                            {
                                CircleImageAvatar.BorderThickness = 0;
                                CircleImageAvatar.Source = placeholder;
                            });
                    }
                };
        }

        protected override bool OnBackButtonPressed()
        {
            if(Settings.Current.FirstRun)
                return true;

            return base.OnBackButtonPressed();
        }
    }
}

