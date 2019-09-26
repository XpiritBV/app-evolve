using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using XamarinEvolve.DataObjects;
using XamarinEvolve.Backend.Models;
using XamarinEvolve.Backend.Helpers;
using System.Net;
using System;
using Microsoft.Azure.Mobile.Server.Config;
using System.Data.Entity;

namespace XamarinEvolve.Backend.Controllers
{
    [MobileAppController]
    public class MobileToWebSyncController : ApiController
    {
		private const string AutoMarkFavoritesAsNewQuery = "UPDATE dbo.Favorites SET UpdatedAt = GETUTCDATE() FROM dbo.Favorites WITH (NOLOCK) WHERE UserId = {0} AND Deleted = 0";

		private RandomStringGenerator _codeGenerator;

        public MobileToWebSyncController()
        {
            _codeGenerator = new RandomStringGenerator(
                UseUpperCaseCharacters: true,
                UseLowerCaseCharacters: false,
                UseNumericCharacters: true,
                UseSpecialCharacters: false);
        }

        [Authorize]
        public async Task<IHttpActionResult> GetMobileToWebSync(string id)
        {
            using (var context = new XamarinEvolveContext())
            {
                var items = context.MobileToWebSyncs.Where(i => i.TempCode == id);

                if (items.Count() == 0)
                    throw new HttpResponseException(HttpStatusCode.NotFound);

                if (items.Count() > 1)
                    throw new HttpResponseException(HttpStatusCode.Conflict);

                var item = items.FirstOrDefault();
                if (item.Expires < DateTime.UtcNow)
                {
                    context.MobileToWebSyncs.Remove(item);
                    await context.SaveChangesAsync();
                    throw new HttpResponseException(HttpStatusCode.Gone);
                }

				var userId = AuthenticationHelper.GetAuthenticatedUserId(RequestContext);
				await context.Database.ExecuteSqlCommandAsync(AutoMarkFavoritesAsNewQuery, userId);

				return Content(HttpStatusCode.OK, item);
            }
        }

        [Authorize]
        public async Task<IHttpActionResult> PostMobileToWebSync(MobileToWebSync item)
        {
            var userId = AuthenticationHelper.GetAuthenticatedUserId(RequestContext);

            MobileToWebSync result;

            using (var context = new XamarinEvolveContext())
            {
                // First look if there's an existing code for this user
                result = await context.MobileToWebSyncs.Where(i => i.UserId == userId).FirstOrDefaultAsync();
                if (result != null)
                {
                    // If it's expired, remove it so we can create a new one
                    if (result.Expires < DateTime.UtcNow)
                    {
                        context.MobileToWebSyncs.Remove(result);
                        result = null;
                    }
                }

                if (result == null)
                {
                    // Make a new server generated entity
                    var newItem = new MobileToWebSync
                    {
                        UserId = userId,
                        TempCode = _codeGenerator.Generate(FixedLength: 5),
                        Expires = DateTime.UtcNow.AddHours(1)
                    };

                    result = context.MobileToWebSyncs.Add(newItem);
                }

                await context.SaveChangesAsync();

				await context.Database.ExecuteSqlCommandAsync(AutoMarkFavoritesAsNewQuery, userId);
			}

			return Ok(result);
        }
    }
}
