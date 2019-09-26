using System;

namespace XamarinEvolve.Clients.Portable
{
	public class CustomVisionResponse
	{
		public string Id { get; set; }
		public string Project { get; set; }
		public string Iteration { get; set; }
		public DateTime Created { get; set; }
		public Prediction[] Predictions { get; set; }
	}

	public class Prediction
	{
		public string TagId { get; set; }
		public string Tag { get; set; }
		public float Probability { get; set; }
	}
}
