using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Smoking.Extensions;
using Smoking.Models;

namespace Smoking.Controllers
{
    public class MasterHomeController : Controller
    {
        [HttpGet]
        [AuthorizeMaster]
        public ActionResult Index()
        {
            return View();
        }

    }
}
