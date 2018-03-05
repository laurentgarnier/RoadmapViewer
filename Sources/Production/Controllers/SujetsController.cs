using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RoadmapViewer.DAL;

namespace RoadmapViewer.Controllers
{
    public class SujetsController : Controller
    {
        // GET: Sujets
        public ActionResult Index()
        {
            var dal = new DalExcelExtractSciforma(Server.MapPath("/DataSources.xml")); ;
            ViewData["Roadmap"] = dal.Roadmap;
            return View();
        }
    }
}