using ColoredConsole;
using Newtonsoft.Json;
using PhotoSizeChecker.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PhotoSizeChecker
{
    class Program
    {
        static HttpClient _client = new HttpClient();
        const string SpeakerListUrl = "https://techdays-2016.azurewebsites.net/tables/Speaker";
        const string ZumoHeaderName = "ZUMO-API-VERSION";
        const string ZumoHeaderValue = "2.0.0";

        static void Main(string[] args)
        {
            DoWork().GetAwaiter().GetResult();
        }

        static async Task DoWork()
        {
            var getSpeakersRequest = new HttpRequestMessage(HttpMethod.Get, SpeakerListUrl);
            getSpeakersRequest.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            getSpeakersRequest.Headers.Add(ZumoHeaderName, ZumoHeaderValue);

            ColorConsole.WriteLine("Check Image Size of Speakers".White());

            ColorConsole.WriteLine("Retrieving speakers...".DarkGray());

            var response = await _client.SendAsync(getSpeakersRequest);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var speakers = JsonConvert.DeserializeObject<List<AugmentedSpeaker>>(json);

                ColorConsole.WriteLine($"{speakers?.Count ?? 0} speakers were found".DarkGray());

                speakers = speakers.OrderBy(s => s.FullName).ToList();

                foreach (var speaker in speakers)
                {
                    ColorConsole.WriteLine("==================================================".DarkGray());

                    if (!string.IsNullOrEmpty(speaker.PhotoUrl) && !string.IsNullOrEmpty(speaker.AvatarUrl))
                    {
                        ColorConsole.WriteLine($"{speaker.FullName} - {speaker.PhotoUrl}".Gray());

                        var url = speaker.PhotoUrl;
                        speaker.PhotoFileSize = await CheckSize(speaker.PhotoUrl);
                        speaker.AvatarFileSize = await CheckSize(speaker.AvatarUrl);

                        ColorConsole.WriteLine($"Photo".Gray(), " ", speaker.PhotoFileSizeText);
                        ColorConsole.WriteLine($"Avatar".Gray(), " ", speaker.AvatarFileSizeText);
                    }
                    else
                    {
                        ColorConsole.WriteLine($"{speaker.FullName} - has no photo - SKIP".DarkGray());
                    }
                }

                var jsonResult = JsonConvert.SerializeObject(speakers);
                using (var f = new FileStream("output.json", FileMode.Create))
                using (var sw = new StreamWriter(f))
                {
                    await sw.WriteAsync(jsonResult);
                    await sw.FlushAsync();
                }

                ColorConsole.WriteLine("Done".White());
                Console.ReadKey();
            }
        }

        private static async Task<long?> CheckSize(string url)
        {
            var headRequest = new HttpRequestMessage(HttpMethod.Head, url);

            var headResponse = await _client.SendAsync(headRequest);
            if (headResponse.IsSuccessStatusCode)
            {
                return headResponse.Content.Headers.ContentLength;
            }
            return -1;
        }
    }
}
