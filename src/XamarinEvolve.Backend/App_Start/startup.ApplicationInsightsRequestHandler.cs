using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Owin;
using Owin;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace XamarinEvolve.Backend.App_Start
{
	//azure mobile app services block module loading
	// this class generates events like the http handler will, so we can still picku the events
	public class ApplicationInsightsMobileAppRequestHandler : OwinMiddleware
	{
		private readonly TelemetryClient telemetryClient;

		public ApplicationInsightsMobileAppRequestHandler(OwinMiddleware next) : base(next)
		{
			try
			{
				// The call initializes TelemetryConfiguration that will create and Intialize modules
				TelemetryConfiguration configuration = TelemetryConfiguration.Active;
				telemetryClient = new TelemetryClient(configuration);
			}
			catch (Exception exc)
			{
				Trace.WriteLine("Error initializing Handler");
				Trace.WriteLine(exc.Message);
			}
		}

		public override async Task Invoke(IOwinContext context)
		{
			var operation = telemetryClient.StartOperation<RequestTelemetry>(context.Request.Path.Value);
			try
			{
				var requestTelemetry = operation.Telemetry;
				if (context.Request.Method == "POST")
				{


					using (var reader = new StreamReader(context.Request.Body))
					{
						try
						{
							var position = context.Request.Body.Position;

							context.Request.Body.Position = 0;
							string requestBody = await reader.ReadToEndAsync().ConfigureAwait(false);

							//replace the request, because after we read it would otherwise be empty
							byte[] requestData = Encoding.UTF8.GetBytes(requestBody);
							context.Request.Body = new MemoryStream(requestData);
							telemetryClient.TrackTrace("requestBody: " + requestBody);
							requestTelemetry.Properties.Add("body", requestBody);
							context.Request.Body.Position = position;
						}
						catch (Exception e)
						{
							requestTelemetry.Properties.Add("collectionerror", e.Message);
						}

					}
				}
				requestTelemetry.Url = context.Request.Uri;
				requestTelemetry.ResponseCode = context.Response.StatusCode.ToString();
				requestTelemetry.Success = context.Response.StatusCode >= 200 && context.Response.StatusCode < 300;
				await this.Next.Invoke(context);
			}

			catch (Exception exc)
			{
				var telemetry = new ExceptionTelemetry(exc);
				telemetryClient.TrackException(telemetry);
			}
			finally
			{
				telemetryClient.StopOperation(operation);
			}
		}
	}



	public static class AppBuilderExtensions
	{
		public static IAppBuilder UseMobileAppRequestHandler(this IAppBuilder app)
		{
			return app.Use<ApplicationInsightsMobileAppRequestHandler>();
		}
	}
}
