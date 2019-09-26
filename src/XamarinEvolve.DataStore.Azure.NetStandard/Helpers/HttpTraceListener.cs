using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Globalization;

namespace HttpTracing
{
	public class HttpEventListener : EventListener
	{
		class HttpEvent
		{
			public Stopwatch Stopwatch { get; set; }

			public string Url { get; set; }

			public DateTimeOffset RequestedAt { get; set; }
		}

		private const int HttpBeginResponse = 140;
		private const int HttpEndResponse = 141;
		private const int HttpBeginGetRequestStream = 142;
		private const int HttpEndGetRequestStream = 143;

		private readonly ConcurrentDictionary<long, HttpEvent> _trackedEvents = new ConcurrentDictionary<long, HttpEvent>();

		protected override void OnEventSourceCreated(EventSource eventSource)
		{
			if (eventSource != null && eventSource.Name == "System.Diagnostics.Eventing.FrameworkEventSource")
			{
				EnableEvents(eventSource, EventLevel.Informational, (EventKeywords)4);
			}
			base.OnEventSourceCreated(eventSource);
		}

		protected override void OnEventWritten(EventWrittenEventArgs eventData)
		{
			if (eventData?.Payload == null)
				return;

			try
			{
				switch (eventData.EventId)
				{
					case HttpBeginResponse:
					case HttpBeginGetRequestStream:
						OnBeginHttpResponse(eventData);
						break;
					case HttpEndResponse:
					case HttpEndGetRequestStream:
						OnEndHttpResponse(eventData);
						break;
				}
			}
			catch (Exception)
			{
				// don't let the tracer break due to frailities underneath, you might want to consider unbinding it
			}
		}

		private void OnBeginHttpResponse(EventWrittenEventArgs httpEventData)
		{
			if (httpEventData.Payload.Count < 2)
			{
				return;
			}
#if NET46
            int indexOfId = httpEventData.PayloadNames.IndexOf("id");
            int indexOfUrl = httpEventData.PayloadNames.IndexOf("uri");
#else
			int indexOfId = 0;
			int indexOfUrl = 1;
#endif

			if (indexOfId == -1 || indexOfUrl == -1)
			{
				return;
			}
			long id = Convert.ToInt64(httpEventData.Payload[indexOfId], CultureInfo.InvariantCulture);
			string url = Convert.ToString(httpEventData.Payload[indexOfUrl], CultureInfo.InvariantCulture);
			_trackedEvents[id] = new HttpEvent
			{
				Url = url,
				Stopwatch = new Stopwatch(),
				RequestedAt = DateTimeOffset.UtcNow
			};
			_trackedEvents[id].Stopwatch.Start();
		}

		private void OnEndHttpResponse(EventWrittenEventArgs httpEventData)
		{
			if (httpEventData.Payload.Count < 1)
			{
				return;
			}
#if NET46
            int indexOfId = httpEventData.PayloadNames.IndexOf("id");
            if (indexOfId == -1)
            {
                return;
            }
#else
			int indexOfId = 0;
#endif
			long id = Convert.ToInt64(httpEventData.Payload[indexOfId], CultureInfo.InvariantCulture);
			HttpEvent trackedEvent;
			if (_trackedEvents.TryRemove(id, out trackedEvent))
			{
				trackedEvent.Stopwatch.Stop();
#if NET46
                int indexOfSuccess = httpEventData.PayloadNames.IndexOf("success");
                int indexOfSynchronous = httpEventData.PayloadNames.IndexOf("synchronous");
                int indexOfStatusCode = httpEventData.PayloadNames.IndexOf("statusCode");
#else
				int indexOfSuccess = httpEventData.Payload.Count > 1 ? 1 : -1;
				int indexOfSynchronous = httpEventData.Payload.Count > 2 ? 2 : -1;
				int indexOfStatusCode = httpEventData.Payload.Count > 3 ? 3 : -1;
#endif

				bool? success = indexOfSuccess > -1 ? new bool?(Convert.ToBoolean(httpEventData.Payload[indexOfSuccess])) : null;
				bool? synchronous = indexOfSynchronous > -1 ? new bool?(Convert.ToBoolean(httpEventData.Payload[indexOfSynchronous])) : null;
				int? statusCode = indexOfStatusCode > -1 ? new int?(Convert.ToInt32(httpEventData.Payload[indexOfStatusCode])) : null;

				Trace.WriteLine($"Url: {trackedEvent.Url}\r\nElapsed Time: {trackedEvent.Stopwatch.ElapsedMilliseconds}ms\r\nSuccess: {success}\r\nStatus Code: {statusCode}\r\nSynchronus: {synchronous}");
			}
		}
	}
}
