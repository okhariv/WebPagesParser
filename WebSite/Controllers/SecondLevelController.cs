using System.Web.Mvc;

namespace WebSite.Controllers
{
    public class SecondLevelController : Controller
    {
        public ActionResult First()
        {
            return View("~/Views/SecondLevel/FirstSl.cshtml");
        }

        public ActionResult Second()
        {
            return View();
        }     
    }
}