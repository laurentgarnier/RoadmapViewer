using System.Web.Mvc;
using RoadmapViewer.DAL;

namespace RoadmapViewer.Controllers
{
	public class HomeController : Controller
	{



		

		// GET: Home
		public ActionResult Index()
		{
			var dal = new DalExcelExtractSciforma(Server.MapPath("/DataSources.xml"));;
			ViewData["Roadmap"] = dal.Roadmap;
			return View("Index");
		}
	}
}