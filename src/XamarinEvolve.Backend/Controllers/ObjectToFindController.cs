using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.Azure.Mobile.Server;
using XamarinEvolve.Backend.Models;
using XamarinEvolve.DataObjects;

namespace XamarinEvolve.Backend.Controllers
{
    public class ObjectToFindController : TableController<ObjectToFind>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            XamarinEvolveContext context = new XamarinEvolveContext();
            DomainManager = new EntityDomainManager<ObjectToFind>(context, Request);
        }

        // GET tables/ObjectToFind
        public IQueryable<ObjectToFind> GetAllObjectToFind()
        {
            return Query(); 
        }

        // GET tables/ObjectToFind/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<ObjectToFind> GetObjectToFind(string id)
        {
            return Lookup(id);
        }
    }
}
