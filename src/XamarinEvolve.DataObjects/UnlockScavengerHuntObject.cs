using System;

namespace XamarinEvolve.DataObjects
{
    public class UnlockScavengerHuntObject
    {
		public string ScavengerHuntId { get; set; }
		public string ObjectId { get; set; }
		public string UnlockCode { get; set; }
		public int Attempts { get; set; }
		public DateTime TimeStamp { get; set; }
    }
}
