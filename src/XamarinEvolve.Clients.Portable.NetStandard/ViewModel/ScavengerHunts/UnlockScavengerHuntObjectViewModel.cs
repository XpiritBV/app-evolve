using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using FormsToolkit;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugin.Connectivity;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Xamarin.Forms;
using XamarinEvolve.DataObjects;
using XamarinEvolve.Utils;
using XamarinEvolve.Utils.Helpers;

namespace XamarinEvolve.Clients.Portable
{
    public class UnlockScavengerHuntObjectViewModel : ViewModelBase
    {
        public ScavengerHuntViewModel Hunt { get; set; }
        public ObjectToFindViewModel ObjectToFind { get; set; }
        bool queued = false;
        CancellationTokenSource cts;

        public UnlockScavengerHuntObjectViewModel(ScavengerHuntViewModel hunt, ObjectToFindViewModel item)
        {
            ObjectToFind = item;
            Hunt = hunt;

            MessagingService.Current.Subscribe(MessageKeys.LoggedIn, async (s) =>
            {
				MessagingService.Current.Unsubscribe(MessageKeys.LoggedIn);

				if (!queued)
                    return;

                await ExecuteCheckImageAsync();
            });
        }

		protected override void UpdateCommandCanExecute()
		{
			UnlockObjectCommand?.ChangeCanExecute();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				MessagingService.Current.Unsubscribe(MessageKeys.LoggedIn);
			}
		}

        public void Cancel()
        {
            cts?.Cancel();
        }

        private ImageSource photo;
        public ImageSource Photo
        {
            get => photo;
            set => SetProperty(ref photo, value);
        }

		Command unlockObjectCommand;
		public Command UnlockObjectCommand => unlockObjectCommand ?? (unlockObjectCommand = new Command(() => ExecuteCheckImageAsync().IgnoreResult(ShowError), () => !IsBusy && !ObjectToFind.IsCompleted));

		public string PointsLabel => ObjectToFind.IsCompleted ? "points earned" : "points available";

        private string GenerateHash(DateTime timeStamp, int attempts)
        {
			var codePlain = $"{ObjectToFind.UnlockCode}-{attempts}-{timeStamp.ToString("HH:mm")}-{Hunt.Id}-{Settings.Current.UserIdentifier}";
            return MD5Core.GetMD5String(codePlain);
        }

		SemaphoreSlim _blocker = new SemaphoreSlim(1);

		async Task ExecuteCheckImageAsync()
		{
			if (IsBusy)
			{
				return;
			}

			await _blocker.WaitAsync();

			try
			{
				queued = false;

				if (!Hunt.IsOpen)
				{
					MessagingUtils.SendAlert("Oops", $"This treasure hunt is not currently open. Please try again after {Hunt.OpenString}");
					return;
				}
				if (Hunt.TotalAttemptsLeft == 0)
				{
					MessagingUtils.SendAlert("Oops", $"You've used up all of your attempts today. Better luck next time!");
					return;
				}
				if (ObjectToFind.IsCompleted)
				{
					MessagingUtils.SendAlert("Oops", $"You've already unlocked this object. Go find one of the other objects in this treasure hunt :)");
					return;
				}

				if (!CrossConnectivity.Current.IsConnected)
				{
					MessagingUtils.SendAlert("Offline", "You are currently offline. Please go online in order to unlock this object.");
					return;
				}

				if (!Settings.Current.IsLoggedIn)
				{
					queued = true;
					MessagingService.Current.SendMessage(MessageKeys.NavigateLogin);
					return;
				}

				try
				{
					var platformAction = Locator.Get<IPlatformActionWrapper<ObjectToFind>>();
					platformAction?.Before(ObjectToFind.Model);

					await CrossMedia.Current.Initialize();

					//if (!await HandleCameraPermission())
					//{
					//	return;
					//}

					if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
					{
						MessagingUtils.SendAlert("No camera", "Access to the camera is required in order to play the treasure hunt");
						return;
					}

					var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
					{
						PhotoSize = Plugin.Media.Abstractions.PhotoSize.Small
					});

					if (file == null)
						return;

					Photo = ImageSource.FromStream(() =>
					{
						var stream = file.GetStream();
						return stream;
					});

					IsBusy = true;
					cts = new CancellationTokenSource();

					var attemptSuccessful = false;
					using (var stream = file.GetStream())
					{
						var binaryReader = new BinaryReader(stream);
						var imagesBytes = binaryReader.ReadBytes((int)stream.Length);
						attemptSuccessful = await CheckImageForDescription(imagesBytes);
					}

					file.Dispose();

					if (!cts.IsCancellationRequested)
					{
						if (attemptSuccessful)
						{
							if (await UnlockObject())
							{
								platformAction?.Success(ObjectToFind.Model);
								ObjectToFind.Complete();

								OnPropertyChanged(nameof(PointsLabel));
								UnlockObjectCommand.ChangeCanExecute();
								MessagingService.Current.SendMessage<ObjectToFindViewModel>("object_unlocked", ObjectToFind);
							}
						}
						else
						{
							ObjectToFind.FailedAttempt();

							platformAction?.Error(ObjectToFind.Model);
							MessagingUtils.SendAlert("Oops", $"That doesn't look like '{ObjectToFind.Name}'. Better luck next time!");
						}
					}
				}
				catch (MediaPermissionException ex)
				{
					Logger.Report(ex);
					MessagingUtils.SendAlert("No camera access", "Access to the camera is required in order to play the treasure hunt. Please grant this app access to your camera.");
				}
				catch (Exception ex)
				{
					Logger.Report(ex);
					MessagingUtils.SendAlert("Oops", "Something went terribly wrong. Please try again.");
				}
				finally
				{
					IsBusy = false;
				}
			}
			finally
			{
				_blocker.Release();
			}
		}

        public async Task<bool> UnlockObject()
		{
			try
			{
				var mobileClient = DataStore.Azure.StoreManager.MobileService;
                if (mobileClient == null)
                    throw new InvalidOperationException("DataStore.Azure.StoreManager.MobileService could not be resolved");

				var timeStamp = Clock.Now;
				var attempts = Settings.Current.GetScavengerHuntAttempts(ObjectToFind.Id) + 1;
				var attempt = new UnlockScavengerHuntObject
				{
					ObjectId = ObjectToFind.Id,
					ScavengerHuntId = Hunt.Id,
					Attempts = attempts,
					UnlockCode = GenerateHash(timeStamp, attempts),
					TimeStamp = timeStamp
				};

                if (cts.IsCancellationRequested)
                    return false;

				Logger.Track(EvolveLoggerKeys.UnlockScavengerHuntObject, "Name", ObjectToFind.Name);

				var body = JsonConvert.SerializeObject(attempt);
				await mobileClient.InvokeApiAsync("FoundScavengerHuntItem", body, HttpMethod.Post, null, cts.Token);
			}
			catch (OperationCanceledException) { }
			catch (MobileServiceInvalidOperationException badRequest)
			{
				if (badRequest.Response != null)
				{
					var body = await badRequest.Response.Content.ReadAsStringAsync();
					var responseContent = JObject.Parse(body);
					MessagingUtils.SendAlert("Oops", responseContent.Value<string>("message"));
				}
                return false;
			}
			catch (Exception ex)
			{
				Logger.Report(ex);
                return false;
			}
            return true;
		}

		private async Task<bool> CheckImageForDescription(byte[] imagesBytes)
		{
            try
            {
                if (cts.IsCancellationRequested)
                    return false;

                using (ByteArrayContent content = new ByteArrayContent(imagesBytes))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

					using (var client = HttpClientFactory.CreateClient(Locator))
					{
						client.DefaultRequestHeaders.Add("Prediction-Key", AboutThisApp.CustomVisionPredictionKey);

						var response = await client.PostAsync(AboutThisApp.CustomVisionUrl, content, cts.Token);
						string contentString = await response.Content.ReadAsStringAsync();
						Debug.WriteLine(contentString);

						var apiResponse = JsonConvert.DeserializeObject<CustomVisionResponse>(contentString);
						if (apiResponse != null)
						{
							if (apiResponse.Predictions?.Any() ?? false)
							{
								var mostConfidence = apiResponse.Predictions.OrderByDescending(p => p.Probability).First();
								Logger.Track(EvolveLoggerKeys.ScavengerHuntObjectDetected, "Tag", mostConfidence.Tag);
								if (mostConfidence.Probability > AboutThisApp.CustomVisionPredictionTreshold && mostConfidence.Tag == ObjectToFind.MatchTag)
								{
									return true;
								}
							}
						}
					}
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Logger.Report(ex);
				MessagingUtils.SendAlert("Oops", $"Got an unexepected response from the Custom Vision API. Please try again!");
                throw;
			}
            return false;
		}
    }
}
