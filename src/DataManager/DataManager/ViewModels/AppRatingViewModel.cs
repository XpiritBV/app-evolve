namespace DataManager.ViewModels
{
    public class AppRatingViewModel
    {
        public string Id { get; set; }
        public string DeviceOS { get; set; }
        public int NumberOfVotes { get; set; }
        public decimal Score { get; set; }
        public int OneStar { get; set; }
        public int TwoStar { get; set; }
        public int ThreeStar { get; set; }
        public int FourStar { get; set; }
        public int FiveStar { get; set; }
    }
}