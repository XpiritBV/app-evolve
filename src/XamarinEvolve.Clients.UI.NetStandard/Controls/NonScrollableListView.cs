using System;
using Xamarin.Forms;

namespace XamarinEvolve.Clients.UI
{
    public class NonScrollableListView : ListView
    {
        public NonScrollableListView()
            :base(ListViewCachingStrategy.RecycleElement)
        {

        }
    }
}

