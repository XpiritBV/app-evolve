
namespace XamarinEvolve.DataObjects
{
    public class ObjectToFind : BaseDataObject
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string MatchTag { get; set; }
        public int Score { get; set; }
        public string UnlockCode { get; set; }
        public string AreaWhereToFind { get; set; }
        public string SmallPhotoUrl { get; set; }
        public string LargePhotoUrl { get; set; }
    }
}
