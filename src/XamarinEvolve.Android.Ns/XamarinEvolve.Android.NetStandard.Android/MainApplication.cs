using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Plugin.CurrentActivity;
using XamarinEvolve.Clients.Portable;

namespace XamarinEvolve.Droid
{
	//You can specify additional application information in this attribute
    [Application]
    public class MainApplication : Application, Application.IActivityLifecycleCallbacks
    {
		internal static Context ActivityContext { get; private set; }
		public MainApplication(IntPtr handle, JniHandleOwnership transer)
          :base(handle, transer)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();
            RegisterActivityLifecycleCallbacks(this);
            Microsoft.WindowsAzure.MobileServices.CurrentPlatform.Init();

        }

        public override void OnTerminate()
        {
            base.OnTerminate();
            UnregisterActivityLifecycleCallbacks(this);
        }
		public void OnActivityCreated(Activity activity, Bundle savedInstanceState)
		{
			ActivityContext = activity;
		}

		public void OnActivityResumed(Activity activity)
		{
			ActivityContext = activity;
		}

		public void OnActivityStarted(Activity activity)
		{
			ActivityContext = activity;
		}
		

		public void OnActivityDestroyed(Activity activity) { }
		public void OnActivityPaused(Activity activity) { }
		public void OnActivitySaveInstanceState(Activity activity, Bundle outState) { }
		
		

        public void OnActivityStopped(Activity activity)
        {
        }
    }
}
