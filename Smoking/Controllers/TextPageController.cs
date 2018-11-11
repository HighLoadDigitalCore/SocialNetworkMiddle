using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Smoking.Extensions;
using Smoking.Extensions.Helpers;
using Smoking.Models;

namespace Smoking.Controllers
{

    public class TextPageController : Controller
    {
        private DB db = new DB();

        [ClientTemplate("Текстовый блок")]
        public ActionResult Index(int ViewID)
        {
            var info = AccessHelper.CurrentPageInfo;
            var data = db.CMSPageTextDatas.Where(
                x =>
                x.CMSPageID == info.CurrentPage.ID && x.LangID == info.CurrentLang.ID &&
                x.ViewID == ViewID).Where(x => x.Visible).OrderBy(x => x.OrderNum);
            return
                PartialView(data);
        }



        [HttpGet]
        [AuthorizeMaster]
        public ActionResult Editor(int? pageID)
        {
            var targetType = db.PageTypes.First(x => x.TypeName == "TextPage").ID;

            var pages =
                CMSPage.FullPageTable.Where(x => x.Type == targetType).OrderBy(x => x.OrderNum).AsEnumerable().ToList
                    ();
            foreach (var page in pages)
            {
                page.LoadLangValues();
            }
            pages.Insert(0, new CMSPage() {ID = 0, PageName = "Выберите страницу в списке"});

            ViewBag.TextPages = new SelectList(pages, "ID", "PageName", pageID ?? 0);

            var current = db.CMSPages.FirstOrDefault(x => x.ID == pageID);
            if (current != null)
                current.LoadLangValues();
            return View(current);
        }


        [HttpPost]
        [AuthorizeMaster]
        [ValidateInput(false)]
        public ActionResult Editor(int? pageID, FormCollection collection)
        {
            var current = db.CMSPages.FirstOrDefault(x => x.ID == pageID);
            if (current != null)
                current.LoadLangValues();
            if (current == null)
            {
                return RedirectToAction("Editor");
            }
            var targetType = db.PageTypes.First(x => x.TypeName == "TextPage").ID;

            var pages =
                CMSPage.FullPageTable.Where(x => x.Type == targetType).OrderBy(x => x.OrderNum).AsEnumerable().ToList
                    ();
            pages.Insert(0, new CMSPage() {ID = 0, PageName = "Выберите страницу в списке"});

            ViewBag.TextPages = new SelectList(pages, "ID", "PageName", pageID ?? 0);



            CMSPageTextData data;
            if (current.CMSPageTextDatas.Any(z => z.LangID == AccessHelper.CurrentLang.ID))
                data = current.CMSPageTextDatas.First(z => z.LangID == AccessHelper.CurrentLang.ID);
            else
            {
                data = new CMSPageTextData() {CMSPage = current, LangID = AccessHelper.CurrentLang.ID};
                db.CMSPageTextDatas.InsertOnSubmit(data);
            }
            data.Text = (string) collection.ToValueProvider().GetValue("Text").ConvertTo(typeof (string));
            db.SubmitChanges();

            ModelState.AddModelError("", "Данные успешно сохранены");
            return View(current);
        }

        [ClientTemplate("Шаблон страницы 404", false)]
        public ActionResult NotFound()
        {

            Response.StatusCode = 404;
            Response.TrySkipIisCustomErrors = true;
            return PartialView();

        }

        [ClientTemplate("Шаблон страницы 403", false)]
        public ActionResult AccessDenied()
        {

            Response.StatusCode = 403;
            Response.TrySkipIisCustomErrors = true;
            return PartialView();

        }
    }
}
