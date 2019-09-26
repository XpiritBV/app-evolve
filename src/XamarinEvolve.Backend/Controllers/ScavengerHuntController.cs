using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.Azure.Mobile.Server;
using XamarinEvolve.Backend.Models;
using XamarinEvolve.DataObjects;
using XamarinEvolve.Backend.Helpers;

namespace XamarinEvolve.Backend.Controllers
{
    public class ScavengerHuntController : TableController<ScavengerHunt>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            XamarinEvolveContext context = new XamarinEvolveContext();
            DomainManager = new EntityDomainManager<ScavengerHunt>(context, Request);
        }

		[QueryableExpand("ObjectsToFind")]
		// GET tables/ScavengerHunt
		public IQueryable<ScavengerHunt> GetAllScavengerHunt()
        {
            return Query(); 
        }

		[QueryableExpand("ObjectsToFind")]
		// GET tables/ScavengerHunt/48D68C86-6EA6-4C25-AA33-223FC9A27959
		public SingleResult<ScavengerHunt> GetScavengerHunt(string id)
        {
            return Lookup(id);
        }
    }
}
