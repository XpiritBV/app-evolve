using System;
using Xamarin.Forms;
using XamarinEvolve.DataObjects;
using XamarinEvolve.Utils;

namespace XamarinEvolve.Clients.Portable
{
    public class ObjectToFindViewModel : ViewModelBase
    {
        private ObjectToFind _model;

        public ObjectToFindViewModel(ObjectToFind model)
        {
            _model = model;
        }

        public string Id => _model.Id;
        public string Name => _model.Name;
        public string Description => _model.Description;
        public string AreaWhereToFind => _model.AreaWhereToFind;
        public string MatchTag => _model.MatchTag;
		public string UnlockCode => _model.UnlockCode;
		public int MaxScore => _model.Score;
		public int ScoreLeft => CalculateSoreLeft();
		public int Attempts => Settings.Current.GetScavengerHuntAttempts(Id);

		private int CalculateSoreLeft()
		{
			var attemptsMade = Attempts;

			if (attemptsMade == 0)
			{
				return _model.Score;
			}
			if (IsCompleted)
			{
				// if the last attempt was successful, 
				// then we don't want to subtract that from the score
				attemptsMade--;
			}

			var scoreLeft = _model.Score - (AppBehavior.ScavengerHuntCostPerAttempt * attemptsMade);
			return scoreLeft >= 0 ? scoreLeft : 0;
		}

		public bool IsCompleted
        {
            get
            {
                return Settings.Current.IsScavengerHuntObjectUnlocked(Id);
            }
            set
            {
                Settings.Current.UnlockScavengerHuntObject(Id, value);
                OnPropertyChanged(nameof(Image));
                OnPropertyChanged();
            }
        }

        public ImageSource Image => IsCompleted ? ImageSource.FromFile("checkmark_green.png") : (string.IsNullOrWhiteSpace(_model.SmallPhotoUrl) ? ImageSource.FromFile("questionmark.png") : ImageSource.FromUri(new Uri(_model.SmallPhotoUrl)));
		public ObjectToFind Model => _model;

		private void Attempt()
		{
			Settings.Current.IncrementScavengerHuntAttempt(Id);
			OnPropertyChanged(nameof(Attempts));
			OnPropertyChanged(nameof(ScoreLeft));
		}

		internal void Complete()
		{
			IsCompleted = true;
			Attempt();
		}

		internal void FailedAttempt()
		{
			Attempt();
		}
	}
}
