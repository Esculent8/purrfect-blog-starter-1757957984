using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using purrfect_blog_starter_1757957984.Models;

namespace purrfect_blog_starter_1757957984.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            using (var db = new ApplicationDbContext())
            {
                var recent = db.Posts
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(3)
                    .ToList();
                return View(recent);
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Register()
        {
            ViewBag.Message = "Your registration page.";
            return View();
        }

    }
}