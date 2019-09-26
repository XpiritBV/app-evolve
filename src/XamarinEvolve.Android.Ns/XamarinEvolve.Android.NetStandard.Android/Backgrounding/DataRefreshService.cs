using System;
using Android.App;
using Android.Content;
using Android.Util;
using Android.OS;
using Android.Gms.Gcm;
using Xamarin.Forms;
using XamarinEvolve.DataStore.Abstractions;
using XamarinEvolve.Clients.UI;
using XamarinEvolve.Utils;

namespace XamarinEvolve.Droid
{
    [Service (Exported = true, Permission = "com.google.android.gms.permission.BIND_NETWORK_TASK_SERVICE")]
    [IntentFilter (new [] { "com.google.android.gms.gcm.ACTION_TASK_READY" })]
    public class DataRefreshService : GcmTaskService
    {
        IBinder binder;

        const string LOG_TAG = "OnRunTask";

        public DataRefreshService ()
        {
            Log.Debug (LOG_TAG, "Service constructed");
        }
		
        public override IBinder OnBind (Intent intent)
        {
            binder = new DataRefreshServiceBinder (this);
            return binder;
        }

        public override void OnInitializeTasks ()
        {
            base.OnInitializeTasks ();
            ScheduleRefresh (this);
        }

        public override StartCommandResult OnStartCommand (Intent intent, StartCommandFlags flags, int startId)
        {
            return base.OnStartCommand (intent, flags, startId);
        }

        // Logic for task execution
        public override int OnRunTask (TaskParams @params)
        {
            Log.Debug (LOG_TAG, "Starting");
            try
            {
                if (EventInfo.EndOfConference.AddDays(AppBehavior.NumberOfDaysAfterConferenceToStopSyncing) >= Clock.Now)
                {
                    System.Threading.Tasks.Task.Run(async () =>
                   {
                       try
                       {
                           App.Init();

                        // Download data
                        var manager = DependencyService.Get<IStoreManager>();
                           if (manager == null)
                               return;

                           await manager.SyncAllAsync(Settings.Current.IsLoggedIn);
                           Settings.Current.LastSync = Clock.Now;
                           Settings.Current.HasSyncedData = true;
                           Android.Util.Log.Debug(LOG_TAG, "Succeeded");
                       }
                       catch (Exception ex)
                       {
                           Android.Util.Log.Debug(LOG_TAG, ex.Message);
                       }
                   }).Wait(TimeSpan.FromSeconds(180));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Log.Debug (LOG_TAG, "Ending");

            return GcmNetworkManager.ResultSuccess;
        }

        public static void ScheduleRefresh (Context context)
        {
            try
            {
                Android.Util.Log.Debug ("app", "Start BackgroundDataRefreshService");
                var pt = new PeriodicTask.Builder ()
                    .SetPeriod (5400) // in seconds; 90 minutes
                    .SetFlex (600) // could be 10 mins before or after, that is cool
                    .SetService (Java.Lang.Class.FromType (typeof (DataRefreshService)))
                    .SetRequiredNetwork (Android.Gms.Gcm.Task.NetworkStateConnected)
                    .SetTag ($"{AboutThisApp.PackageName}.backgrounddatarefresh")
                    .SetPersisted (true)
                    .SetRequiresCharging (false)
                    .SetUpdateCurrent (true)
                    .Build ();

                GcmNetworkManager.GetInstance (context).Schedule (pt);
            }
            catch (Exception ex)
            {
                DependencyService.Get<ILogger>()?.Report(ex);
            }
        }
    }


}
