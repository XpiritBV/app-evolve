using Microsoft.Azure.Mobile.Server.Config;
using Newtonsoft.Json.Linq;
using System;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using XamarinEvolve.Backend.Helpers;
using XamarinEvolve.Backend.Models;
using XamarinEvolve.DataObjects;

namespace XamarinEvolve.Backend.Controllers
{
	[MobileAppController]
    public class FoundScavengerHuntItemController : ApiController
    {
        [Authorize]
        public async Task<IHttpActionResult> Post([FromBody] JObject data)
        {
			var attempt = data.ToObject<UnlockScavengerHuntObject>();
            var userId = AuthenticationHelper.GetAuthenticatedUserId(RequestContext);

            using (var context = new XamarinEvolveContext())
            {
				var hunt = await context.ScavengerHunts
					.Include(h => h.ObjectsToFind)
					.FirstOrDefaultAsync(h => h.Id == attempt.ScavengerHuntId);

				if (hunt.OpenFrom > DateTime.UtcNow || hunt.OpenUntil < DateTime.UtcNow)
				{
					return BadRequest("Scavenger hunt is not currently open");
				}

				var objectToFind = hunt.ObjectsToFind.FirstOrDefault(o => o.Id == attempt.ObjectId);
				if (objectToFind == null)
				{
					return BadRequest("Found object is not part of this scavenger hunt");
				}

				if (!VerifyUnlockCode(attempt, objectToFind, userId))
				{
					return BadRequest("Invalid unlock code");
				}

				var getCountQuery = @"SELECT COUNT(1) FROM [dbo].[ScavengerHuntCompletions] WHERE UserId = {0} AND HuntId = {1} AND ObjectId = {2}";
                var count = await context.Database.SqlQuery<int>(getCountQuery, userId, attempt.ScavengerHuntId, attempt.ObjectId).SingleAsync();

                if (count == 0)
                {
                    var insertQuery = @"INSERT [dbo].[ScavengerHuntCompletions] (UserId, HuntId, ObjectId, Attempts) VALUES ({0}, {1}, {2}, {3})";
                    context.Database.ExecuteSqlCommand(insertQuery, userId, attempt.ScavengerHuntId, attempt.ObjectId, attempt.Attempts);
                }
            }

            return Ok();
        }

		private bool VerifyUnlockCode(UnlockScavengerHuntObject attempt, ObjectToFind objectToFind, string userId)
		{
			var verification = $"{objectToFind.UnlockCode}-{attempt.Attempts}-{attempt.TimeStamp.ToString("HH:mm")}-{attempt.ScavengerHuntId}-{userId}";
			var calculatedHash = MD5Hash(verification);
			return string.Compare(calculatedHash, attempt.UnlockCode, StringComparison.Ordinal) == 0;
		}

		public static string MD5Hash(string input)
		{
			StringBuilder hash = new StringBuilder();
			var md5provider = new MD5CryptoServiceProvider();
			byte[] bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(input));

			for (int i = 0; i < bytes.Length; i++)
			{
				hash.Append(bytes[i].ToString("x2"));
			}
			return hash.ToString();
		}

		public async Task<int> Get(string id)
        {
            int count = 0;

            using (var context = new XamarinEvolveContext())
            {
                var getCountQuery = @"SELECT COUNT(1) FROM [dbo].[ScavengerHuntCompletions] WHERE HuntId = {0}";

                count = await context.Database.SqlQuery<int>(getCountQuery, id).SingleAsync();
            }

            return count;
        }
    }
}
