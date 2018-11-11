using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using Smoking.Extensions;
using Smoking.Extensions.Helpers;
using Smoking.Models;

namespace Smoking.Controllers
{
    public class LanguagesController : Controller
    {
        private DB db = new DB();
        [HttpGet]
        [AuthorizeMaster]
        public ActionResult Index()
        {
            return View(db.Languages.OrderBy(x => x.Ordernum));
        }

        [HttpGet]
        [AuthorizeMaster]
        public ActionResult Edit(int? id)
        {
            Language lang = null;
            if (id.HasValue)
                lang = db.Languages.FirstOrDefault(x => x.ID == id);

            if (lang == null)
                lang = new Language() { ByDef = false, Enabled = true };

            return View(lang);
        }

        [HttpPost]
        [AuthorizeMaster]
        public ActionResult Edit(int ID, FormCollection collection, HttpPostedFileBase Icon)
        {
            Language exist = null;
            if (ID == 0)
            {
                exist = new Language();
                db.Languages.InsertOnSubmit(exist);
                exist.Ordernum = db.Languages.Count() + 1;
            }
            else
            {
                exist = db.Languages.First(x => x.ID == ID);
            }
            TryUpdateModel(exist, new[] { "Name", "ShortName", "Enabled", "ByDef" });
            if (exist.ByDef)
            {
                var another = db.Languages.Where(x => x.ID != exist.ID);
                foreach (var l in another)
                {
                    l.ByDef = false;
                }
            }

            if (Icon != null && Icon.ContentLength > 0)
            {
                var image = new byte[Icon.InputStream.Length];
                Icon.InputStream.Read(image, 0, (int)Icon.InputStream.Length);
                exist.Icon = image;
            }
            db.SubmitChanges();
            return RedirectToAction("Index");

        }

        [HttpPost]
        [AuthorizeMaster]
        public PartialViewResult changeOrder(int id, int? value, int page)
        {
            if (!value.HasValue)
                return PartialView();

            var lang = db.Languages.FirstOrDefault(x => x.ID == id);
            if (lang == null)
                return PartialView();
            var all = db.Languages.Where(x => x.ID != id).OrderBy(x => x.Ordernum).ToList();
            if (all.Any())
            {

                int pos = value.Value - 1;
                if (pos <= 0) pos = 0;
                int max = all.Max(x => x.Ordernum);
                if (pos >= max)
                    all.Add(lang);
                else
                    all.Insert(pos, lang);
                int counter = 1;
                foreach (var language in all)
                {
                    language.Ordernum = counter;
                    counter++;
                }
            }
            else
            {
                lang.Ordernum = 1;
            }
            db.SubmitChanges();

            return PartialView("List", all);
        }


        [AuthorizeMaster]
        [HttpGet]
        public ActionResult Delete(int ID)
        {
            var lang = db.Languages.FirstOrDefault(x => x.ID == ID);
            if (lang == null) return RedirectToAction("Index");
            return View(lang);
        }


        [AuthorizeMaster]
        [HttpPost]
        public ActionResult Delete(int ID, FormCollection collection)
        {
            var lang = db.Languages.FirstOrDefault(x => x.ID == ID);
            if (lang == null) return RedirectToAction("Index");

            if (lang.ByDef)
            {
                ModelState.AddModelError("",
                                         "Нельзя удалить язык, который используется по умолчанию в системе. Перед удалением необходимо выбрать другой язык для использования по умолчанию");
                return View(lang);
            }

            db.Languages.DeleteOnSubmit(lang);
            db.SubmitChanges();
            return RedirectToAction("Index");
        }


        [AllowAnonymous]
        [HttpGet]
        public FileResult Flag(int ID, int? width)
        {
            var cached = HttpRuntime.Cache.Get(string.Format("Flag_{0}_{1}", ID, width));
            if (cached is byte[])
            {
                return new FileStreamResult(new MemoryStream((byte[])cached), 
                                             MIMETypeWrapper.GetMIME("jpeg"));
            }

            var cover = db.Languages.First(x => x.ID == ID);
            FileStreamResult result;
            if (cover == null || cover.Icon == null)
            {
                var fs = new FileStream(Server.MapPath("/Content/nopic.gif"), FileMode.Open, FileAccess.Read);
                result = new FileStreamResult(fs, "image/gif");
            }
            else
            {

                var ms = new MemoryStream(cover.Icon.ToArray());
                ms.Seek(0L, SeekOrigin.Begin);
                var bmpIn = new Bitmap(ms);
                ImageFormat loFormat = bmpIn.RawFormat;

                Bitmap bmpOut = bmpIn.CreateThumbnail(width ?? cover.getProperWidth(cover.Width),
                                                      cover.getProperHeight(width ?? cover.Width), false);
                ms.Close();
                var res = new MemoryStream();
                bmpOut.Save(res, loFormat);
                res.Seek(0L, SeekOrigin.Begin);
                HttpRuntime.Cache.Insert(string.Format("Flag_{0}_{1}", ID, width), res.ToArray(), null,
                         DateTime.Now.AddDays(1D),
                         Cache.NoSlidingExpiration);

                result = new FileStreamResult(res,
                                              MIMETypeWrapper.GetMIME(loFormat.ToString()));

            }
            return result;
        }
    }
}
