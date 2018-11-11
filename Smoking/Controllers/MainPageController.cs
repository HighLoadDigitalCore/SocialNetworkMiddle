using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Smoking.Extensions;
using Smoking.Extensions.Helpers;
using Smoking.Models;

namespace Smoking.Controllers
{
    public class MainPageController : Controller
    {
        private DB db = new DB();

        public PartialViewResult Index()
        {
            return PartialView();
        }

    
    }
}
