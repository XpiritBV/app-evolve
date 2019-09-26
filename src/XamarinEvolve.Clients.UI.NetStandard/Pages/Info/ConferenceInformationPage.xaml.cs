using System;
using System.Collections.Generic;

using Xamarin.Forms;
using XamarinEvolve.Clients.Portable;
using XamarinEvolve.Utils;

namespace XamarinEvolve.Clients.UI
{
	public partial class ConferenceInformationPage : BasePage
    {
		public override AppPage PageType => AppPage.ConferenceInfo;

		ConferenceInfoViewModel vm; 
        public ConferenceInformationPage()
        {
            InitializeComponent();
            BindingContext = vm = new ConferenceInfoViewModel();
        }

		protected override void OnAppearing()
        {
            base.OnAppearing();

			CodeOfConductText.Text = CodeOfConductViewModel.CodeOfConductContent;
            vm.UpdateConfigs().IgnoreResult();
        }
    }
}

