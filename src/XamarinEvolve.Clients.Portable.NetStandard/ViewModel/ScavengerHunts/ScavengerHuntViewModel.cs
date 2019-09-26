using System;
using System.Collections.Generic;
using System.Linq;
using XamarinEvolve.DataObjects;
using XamarinEvolve.Utils;

namespace XamarinEvolve.Clients.Portable
{
	public class ScavengerHuntViewModel : ViewModelBase
	{
		ScavengerHunt _model;

		public ScavengerHuntViewModel(ScavengerHunt model)
		{
			_model = model;
			ObjectsToFind = new List<ObjectToFindViewModel>();
			ObjectsToFind.AddRange(model.ObjectsToFind.Select(o => new ObjectToFindViewModel(o)));
			foreach (var o in ObjectsToFind)
			{
				o.PropertyChanged += (sender, e) => {
					if (e.PropertyName == nameof(o.Attempts))
					{
						OnPropertyChanged(nameof(TotalAttemptsLeft));
						OnPropertyChanged(nameof(TotalScore));
					}
					if (e.PropertyName == nameof(o.IsCompleted))
					{
						OnPropertyChanged(nameof(IsCompleted));
						OnPropertyChanged(nameof(IsOpen));
						OnPropertyChanged(nameof(IsOpenAndNotCompleted));

						if (IsCompleted)
						{
							MessagingUtils.SendAlert("Congratulations", $"Yay! You've found all the objects in this treasure hunt! We'll let you know if you win one of our prizes. Fingers crossed!");
							return;
						}
					}
				};
			}
		}

		public string Id => _model.Id;
		public string Name => _model.Name;
		public string Description => _model.Description;
		public DateTime? OpenFrom => _model.OpenFrom;
		public DateTime? OpenUntil => _model.OpenUntil;

		public List<ObjectToFindViewModel> ObjectsToFind { get; }

		public string OpenString
		{
			get
			{
				if (!OpenFrom.HasValue || !OpenUntil.HasValue || OpenFrom.Value.IsTBA())
					return "To be announced";

				var start = OpenFrom.Value.ToEventTimeZone();
				var startString = start.ToString("t", EventInfo.Culture);

				var end = OpenUntil.Value.ToEventTimeZone();
				var endString = end.ToString("t", EventInfo.Culture);

				if (Clock.Now.Year == start.Year)
				{
					if (Clock.Now.DayOfYear == start.DayOfYear)
						return $"Today {startString} - {endString}";

					if (Clock.Now.DayOfYear + 1 == start.DayOfYear)
						return $"Tomorrow {startString} - {endString}";
				}
				var day = start.ToString("M");
				return $"{day}, {startString} - {endString}";
			}
		}

		public bool IsOpen => OpenFrom.HasValue && OpenFrom.Value.ToUniversalTime() <= Clock.Now && OpenUntil.HasValue && OpenUntil.Value.ToUniversalTime() >= Clock.Now;

		public bool IsOpenAndNotCompleted => IsOpen && !IsCompleted;

		public int TotalScore => ObjectsToFind.Where(o => o.IsCompleted).Sum(o => o.ScoreLeft);

		public int MaxScore => ObjectsToFind.Sum(o => o.MaxScore);

		public bool IsCompleted => ObjectsToFind.Any() && !ObjectsToFind.Any(o => !o.IsCompleted);

		public int TotalAttemptsLeft => AppBehavior.MaxTotalAttemptsPerDay - Settings.Current.GetScavengerHuntAttemptsToday();
	}
}
