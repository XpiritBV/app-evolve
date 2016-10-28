using DataManager.Helpers;
using DataManager.Repository;
using DataManager.ViewModels;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DataManager.Controllers
{
    [Authorize]
    public class AppRatingsController : Controller
    {
        private Techdays2016Repository db = new Techdays2016Repository();

        const string queryName = "DataManager.Repository.Queries.AppRatings.sql";

        // GET: AppRatingsController
        public async Task<ActionResult> Index()
        {
            var query = await ResourceHelper.GetResourceString(queryName);
            var scores = await db.Database.SqlQuery<AppRatingViewModel>(query).ToListAsync();

            return View(scores);
        }
    }
}