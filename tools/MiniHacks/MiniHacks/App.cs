using System;
using MiniHacks.View;
using Xamarin.Forms;

namespace MiniHacks
{
    public class App : Application
    {
        public App()
        {

            MainPage = new NavigationPage(new Hacks());
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}

