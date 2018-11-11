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
    public class DictionaryController : Controller
    {
        [HttpGet]
        [AllowAnonymous]
        public ContentResult Translate(int langID, string key)
        {
            var dict = LabelDictionary.GetDictionary(langID);
            return new ContentResult() {Content = LabelDictionary.Translate(key, dict, langID)};
        }

        [HttpGet]
        [AuthorizeMaster]
        public ActionResult Index(int? page)
        {
            LabelDictionary.SynchronizeDicts();
            var db = new DB();
            var dict = db.LabelDictionaries.OrderBy(x => x.TextKey);
            return View(new PagedData<LabelDictionary>(dict, page ?? 0, 30));

        }

        [HttpPost]
        [AuthorizeMaster]
        public ActionResult Index(int? page, FormCollection collection)
        {
            var textKeys = collection.AllKeys.Where(x => x.StartsWith("Text_"));
            var pairs =
                textKeys.Select(
                    x =>
                    new {ID = x.Split<string>("_").ToArray()[1].ToInt(), Text = collection.GetValue(x).AttemptedValue});
            var db = new DB();
            foreach (var pair in pairs)
            {
                var dict =
                    db.LabelDictionaryLangs.FirstOrDefault(
                        x => x.LanguageID == AccessHelper.CurrentLang.ID && x.LabelID == pair.ID);
                if (dict == null)
                {
                    dict = new LabelDictionaryLang()
                        {
                            LabelID = pair.ID,
                            LanguageID = AccessHelper.CurrentLang.ID,
                            TranslatedLabel = pair.Text
                        };
                    db.LabelDictionaryLangs.InsertOnSubmit(dict);
                }
                else
                {
                    dict.TranslatedLabel = pair.Text;
                }
            }
            db.SubmitChanges();
            ModelState.AddModelError("", "Данные успешно сохранены.");
            var d = db.LabelDictionaries.OrderBy(x => x.TextKey);
            return View(new PagedData<LabelDictionary>(d, page ?? 0, 30));
        }

    }
}
