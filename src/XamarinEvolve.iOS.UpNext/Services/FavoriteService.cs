﻿using System;
using System.IO;
using System.Threading.Tasks;
using Foundation;
using XamarinEvolve.DataObjects;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using XamarinEvolve.Utils;
using MvvmHelpers;

namespace UpNext.Services
{
	public static class FavoriteService
	{
		public static async Task<IEnumerable<Grouping<string, Session>>> GetFavorites()
		{
			var tcs = new TaskCompletionSource<IEnumerable<Grouping<string, Session>>>();

			await Task.Run(() =>
			{
				try
				{
					var fileManager = new NSFileManager();
					var appGroupContainer = fileManager.GetContainerUrl($"group.{AboutThisApp.PackageName}");
					if (appGroupContainer != null)
					{
						var appGroupContainerPath = appGroupContainer.Path;
						var sessionsFilePath = Path.Combine(appGroupContainerPath, "sessions.json");

						if (!fileManager.FileExists(sessionsFilePath))
						{
							tcs.SetResult(Enumerable.Empty<Grouping<string, Session>>());
						}
						else
						{
							var data = File.ReadAllText(sessionsFilePath);
							var sessions = JsonConvert.DeserializeObject<List<Session>>(data);

							Console.WriteLine($"there are {sessions.Count()} sessions in total");

                            // filter sessions that are relevant (between now and ~30 minutes)
                            sessions = sessions.Where(s => (s.StartTime.HasValue && !s.StartTime.Value.IsTBA())
						                          		&& (s.StartTime.Value.ToUniversalTime().AddMinutes(30) >= Clock.Now
							                                || (s.EndTime.HasValue && s.EndTime.Value.ToUniversalTime().AddMinutes(-15) >= Clock.Now)))
                                                   .OrderBy(s => s.StartTime.Value)
                                                   .Take(4)
                                                   .ToList();

							Console.WriteLine($"there are {sessions.Count()} sessions after filtering by time");

							var sessionsGrouped = from s in sessions
                                                  orderby s.StartTimeOrderBy
                                                  group s by s.StartTime.Value.GetSortName()
                                                  into sessionGroup
                                                  select new Grouping<string, Session>(sessionGroup.Key, sessionGroup);

							Console.WriteLine($"there are {sessionsGrouped.Count()} session groups");

							tcs.SetResult(sessionsGrouped);
						}
					}
					else
					{
						tcs.SetResult(null);
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
					tcs.SetException(e);
				}
			}).ConfigureAwait(false);

			return await tcs.Task;
		}
	}
}
