using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.Azure.Mobile.Server;
using XamarinEvolve.DataObjects;
using XamarinEvolve.Backend.Models;
using XamarinEvolve.Backend.Helpers;
using System.Web.Http.OData;
using System;

namespace XamarinEvolve.Backend.Controllers
{
	public class FavoriteController : TableController<Favorite>
    {
		private XamarinEvolveContext _context;

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            _context = new XamarinEvolveContext();
            DomainManager = new EntityDomainManager<Favorite>(_context, Request, true);
            
        }
        [Authorize]
        [EnableQuery(MaxTop = 500, PageSize = 200)]
        public IQueryable<Favorite> GetAllFavorite()
        {
			var userId = AuthenticationHelper.GetAuthenticatedUserId(RequestContext);

			return Query()
				.Where(favorite => favorite.UserId == userId);
		}

        [Authorize]
        public SingleResult<Favorite> GetFavorite(string id)
        {
            return Lookup(id);
        }

        [Authorize]
        public Task<Favorite> PatchFavorite(string id, Delta<Favorite> patch)
        {
            return UpdateAsync(id, patch);
        }

        [Authorize]
        public async Task<IHttpActionResult> PostFavorite(Favorite item)
        {
            item.UserId = AuthenticationHelper.GetAuthenticatedUserId(RequestContext);
			item.UpdatedAt = DateTime.UtcNow;
            var current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        [Authorize]
        public Task DeleteFavorite(string id)
        {
            return DeleteAsync(id);
        }

    }
}
