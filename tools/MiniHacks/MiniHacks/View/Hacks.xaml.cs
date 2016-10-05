using System;
using System.Collections.Generic;
using MiniHacks.Model;
using MiniHacks.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace MiniHacks.View
{
    public partial class Hacks : ContentPage
    {
        MiniHacksViewModel vm;
        public Hacks()
        {
            InitializeComponent();
            BindingContext = vm = new MiniHacksViewModel(this);
            ListViewMiniHacks.Effects.Add(Effect.Resolve("XamarinHacks.ListViewSelectionOnTopEffect"));

            ListViewMiniHacks.ItemTapped += (sender, e) => ListViewMiniHacks.SelectedItem = null;

            ListViewMiniHacks.ItemSelected += async (sender, e) =>
            {
                var hack = e.SelectedItem as MiniHack;

                if (hack == null)
                    return;

                await Navigation.PushAsync(new HackDetail(hack));

                ListViewMiniHacks.SelectedItem = null;
            };
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (vm.MiniHacks.Count == 0)
                vm.LoadMiniHacksCommand.Execute(false);
        }
    }
}

