using System.Web.Mvc;

namespace WebSite.Controllers
{
    public class ThirdLevelController : Controller
    {
        public ActionResult First()
        {
            return View("~/Views/ThirdLevel/FirstTl.cshtml");
        }

        public ActionResult Second()
        {
            return View("~/Views/ThirdLevel/SecondTl.cshtml");
        }
    }
}