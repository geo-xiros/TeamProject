using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TeamProject.Dal;

namespace TeamProject.Areas.Admin.Controllers
{
    public class ReportsController : Controller
    {
        private ProjectDbContext db = new ProjectDbContext();

        // GET: Reports
        [Authorize(Roles = "Admin, Owner")]
        public ActionResult Index(int id)
        {
            var courts = db.Courts.Get("BranchId=@id", new { id });

            return View(courts);
        }

    }

}
