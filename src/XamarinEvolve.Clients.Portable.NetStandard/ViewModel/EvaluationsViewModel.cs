﻿using System;
using System.Collections.Generic;
using System.Windows.Input;
using FormsToolkit;
using MvvmHelpers;
using Xamarin.Forms;
using XamarinEvolve.DataObjects;
using System.Linq;
using System.Threading.Tasks;
using XamarinEvolve.Utils;

namespace XamarinEvolve.Clients.Portable
{
    public class EvaluationsViewModel: ViewModelBase
    {
        public EvaluationsViewModel (INavigation navigation) : base(navigation)
        {
        }

        protected override void UpdateCommandCanExecute()
        {
            loadSessionsCommand?.ChangeCanExecute();
        }

        bool sync = true;

        public static bool ForceRefresh { get; set; }

        public ObservableRangeCollection<Session> Sessions { get; } = new ObservableRangeCollection<Session> ();

        bool noSessionsFound;
        public bool NoSessionsFound {
            get { return noSessionsFound; }
            set { SetProperty (ref noSessionsFound, value); }
        }

        string noSessionsFoundMessage;
        public string NoSessionsFoundMessage {
            get { return noSessionsFoundMessage; }
            set { SetProperty (ref noSessionsFoundMessage, value); }
        }

        Command loadSessionsCommand;
        public ICommand LoadSessionsCommand =>
            loadSessionsCommand ?? (loadSessionsCommand = new Command (() => ExecuteLoadSessionsAsync ().IgnoreResult(ShowError),() => !IsBusy));

        async Task<bool> ExecuteLoadSessionsAsync ()
        {
            if (IsBusy)
                return false;

            try 
            {
                NextForceRefresh = Clock.Now.AddMinutes (AppBehavior.RefreshIntervalInMinutes);
                IsBusy = true;
                NoSessionsFound = false;

                if (!Settings.IsLoggedIn) 
                {
                    NoSessionsFoundMessage = "Please sign in\nto leave feedback";
                    NoSessionsFound = true;
                    return true;
                }

                var sessions = (await StoreManager.SessionStore.GetItemsAsync ()).ToList();
                var feedback = (await StoreManager.FeedbackStore.GetItemsAsync (sync)).ToList();

                sync = false;

                var finalSessions = new List<Session> ();
                foreach (var session in sessions) 
                {
                    if (!session.IsFavorite)
                        continue;
                    
                    //if TBA
                    if (!session.StartTime.HasValue)
                        continue;
#if !DEBUG

                    //if it hasn't started yet
					if (!session.HasStarted)
                        continue;
#endif
                    if (feedback.Any (f => f.SessionId == session.Id))
                        continue;

                    finalSessions.Add (session);
                }

                Sessions.ReplaceRange (finalSessions);

                if (Sessions.Count == 0) 
                {
                    NoSessionsFoundMessage = "No Pending\nEvaluations Found";
                    NoSessionsFound = true;
                } 
                else 
                {
                    NoSessionsFound = false;
                }
            } 
            catch (Exception ex) 
            {
                Logger.Report (ex, "Method", "ExecuteLoadSessionsAsync");
                MessagingService.Current.SendMessage (MessageKeys.Error, ex);
            } 
            finally 
            {
                IsBusy = false;
            }

            return true;
        }
    }
}

