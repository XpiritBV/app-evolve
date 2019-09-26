using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.Azure.Mobile.Server;
using XamarinEvolve.Backend.Models;
using XamarinEvolve.DataObjects;
using XamarinEvolve.Backend.Helpers;
using System;

namespace XamarinEvolve.Backend.Controllers
{
    public class AttendeeController : TableController<Attendee>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            XamarinEvolveContext context = new XamarinEvolveContext();
            DomainManager = new EntityDomainManager<Attendee>(context, Request);
        }

		[Authorize]
		// GET tables/Attendee
		public IQueryable<Attendee> GetAllAttendee()
        {
			var items = Query();

			var userId = AuthenticationHelper.GetAuthenticatedUserId(RequestContext);

			var final = items.Where(item => item.UserId == userId);

			return final;
		}

		[Authorize]
		// GET tables/Attendee/48D68C86-6EA6-4C25-AA33-223FC9A27959
		public SingleResult<Attendee> GetAttendee(string id)
        {
			var currentUser = AuthenticationHelper.GetAuthenticatedUserId(RequestContext);

			if (string.Compare(currentUser, id, StringComparison.OrdinalIgnoreCase) != 0)
			{
				return SingleResult.Create(Enumerable.Empty<Attendee>().AsQueryable());
			}
			return Lookup(id);
        }

		[Authorize]
		// PATCH tables/Attendee/48D68C86-6EA6-4C25-AA33-223FC9A27959
		public Task<Attendee> PatchAttendee(string id, Delta<Attendee> patch)
        {
			var entity = patch.GetEntity();
			entity.UpdatedAt = DateTime.UtcNow;
			return UpdateAsync(id, patch);
        }

		[Authorize]
        // POST tables/Attendee
        public async Task<IHttpActionResult> PostAttendee(Attendee item)
        {
			var attendee = item;
			attendee.CreatedAt = DateTime.UtcNow;
			attendee.UpdatedAt = DateTime.UtcNow;

			attendee.UserId = AuthenticationHelper.GetAuthenticatedUserId(RequestContext);
			var current = await InsertAsync(attendee);
			return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

		[Authorize]
		// DELETE tables/Attendee/48D68C86-6EA6-4C25-AA33-223FC9A27959
		public Task DeleteAttendee(string id)
        {
             return DeleteAsync(id);
        }
    }
}
