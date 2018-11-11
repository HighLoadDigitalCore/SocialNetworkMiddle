using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Smoking;
using Smoking.Extensions;
using Smoking.Extensions.Helpers;
using Smoking.Models;
namespace Smoking.Controllers
{
    public class LentaController : Controller
    {
        private DB db = new DB();

        public ActionResult SendComment()
        {
            return PartialView();
        }
        [ClientTemplate("Блок самых комментируемых новостей или событий")]
        public ActionResult MostCommented()
        {
            var model = new LastAndPopularViewModel();
            return PartialView(model);
        }
        [ClientTemplate("Блок самых популярных за неделю новостей или событий")]
        public ActionResult BestPerWeek()
        {
            var model = new LastAndPopularViewModel();
            return PartialView(model);
        }
        [ClientTemplate("Блок последних и самых популярных новостей или событий")]
        public ActionResult LastAndPopular()
        {
            var model = new LastAndPopularViewModel();
            return PartialView(model);
        }

        [ClientTemplate("Выбор категорий новостей или событий")]
        public ActionResult Categories(string category, int? newsid)
        {
            ViewBag.SelectedCatName = category.IsNullOrEmpty() ? "Без категории" : category;
            int pageID = AccessHelper.CurrentPageInfo.CurrentPage.ID;
            var catList =
                db.Lentas.Where(x => x.PageID == pageID && x.Visible)
                  .Select(x => x.CategoryName.ToLower())
                  .Distinct()
                  .OrderBy(x => x)
                  .ToList()
                  .Select(x => x.ToNiceForm()).Select(x => x.IsNullOrEmpty() ? "Без категории" : x)
                  .Select(
                      x =>
                      new KeyValuePair<string, string>(x,
                                                       HttpContext.CreateURL(new object[]
                                                           {
                                                               "category", x.Replace("Без категории", "")/*, "newsid",
                                                                   newsid*/
                                                           })));

            return PartialView(catList);
        }

        [ClientTemplate("Лента новостей в профиле")]
        public ActionResult ProfileLenta(Guid? uid)
        {

            var settings = new[] {new {URL = "mylenta", Target = "events"}, new {URL = "mynews", Target = "news"}};

            var pageID =
                CMSPage.FullPageTable.First(
                    x => x.URL == settings.First(z => z.URL == AccessHelper.CurrentPageInfo.CurrentPage.URL).Target).ID;

            var lenta =
                db.UserFavoriteLentas.Where(x => x.UserID == (uid ?? AccessHelper.CurrentUserKey))
                  .Select(x => x.Lenta).Where(x => x.ShowInfo && x.PageID == pageID)
                  .OrderByDescending(x => x.CreateDate)
                  .ToList()
                  .Select((x, index) => new KeyValuePair<int, Lenta>(index % 3, x))
                  .GroupBy(x => x.Key);
            return PartialView(lenta);
        }

        [HttpPost]
        public ActionResult Index(string Message, string category, int newsId, FormCollection collection)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                ModelState.AddModelError("", "Для добавления комментария необходимо авторизоваться на сайте");
                return Index(category, newsId, null);
            }
            if (Message.IsNullOrEmpty())
            {
                ModelState.AddModelError("", "Необходимо указать текст сообщения");
                return Index(category, newsId, null);
            }


            var comment = new Comment()
                {
                    Date = DateTime.Now,
                    CommentText = Message,
                    UserID = AccessHelper.CurrentUserKey
                };

            var rel = new LentaComment()
                {
                    Comment = comment,
                    LentaID = newsId,
                };

            db.LentaComments.InsertOnSubmit(rel);
            db.SubmitChanges();



            MailingList.Get("AddNotification")
                       .WithReplacements(
                           new[]
                                           {
                                               new MailReplacement("{OBJTYPE}", "комментарий"),
                                               new MailReplacement("{LINK}", AccessHelper.SiteUrl + comment.CommentedObjectLink)
                                           }
                               .
                               ToList()).Send();


            return Index(category, newsId, null);
        }

        [ClientTemplate("Лента новостей и событий")]
        [HttpGet]
        public ActionResult Index(string category, int? newsId, int? pageID)
        {
            var model = new LentaViewModel();
            model.SelectedCategory = category;
            if (newsId.HasValue)
            {
                var lentaSelected = db.Lentas.FirstOrDefault(x => x.ID == newsId.Value);
                if (lentaSelected != null)
                {
                    lentaSelected.ViewAmount++;
                    db.SubmitChanges();
                    model.IsFullText = true;
                    model.SelectedEvent = lentaSelected;
                    return PartialView(model);
                }
            }
            var cells = new List<string> { "BodyLine1Left", "BodyLine1Right", "BodyLine2", "BodyLine3Left", "BodyLine3Center", "BodyLine3Right" };


            var pid = pageID.HasValue ? pageID.Value : AccessHelper.CurrentPageInfo.CurrentPage.ID;

            var ds =
                db.Lentas.Where(x => x.PageID == pid && x.Visible);


            if (!category.IsNullOrEmpty())
            {
                ds =
                    ds.Where(
                        x =>
                        x.CategoryName.ToLower().Trim() == (category.Trim().IsNullOrEmpty()
                                                                ? "Без категории"
                                                                : category.ToLower().Trim()));
            }


            var lenta = ds.OrderByDescending(x => x.CreateDate)
                          .Take(200)
                          .ToList();


            model.Lenta =
                lenta.OrderBy(x => cells.IndexOf(x.CMSPageCell.ColumnName)).ToList().GroupBy(x => x.CMSPageCell);

            return PartialView(model);
        }

        public ContentResult ToggleFavorite(int id)
        {
            var user = Membership.GetUser();
            if (!HttpContext.User.Identity.IsAuthenticated || user == null)
            {
                return new ContentResult()
                    {
                        Content = "Для добавления точки в избанное, необходимо авторизоваться на сайте"
                    };
            }

            var exist = db.UserFavoriteLentas.Where(x => x.UserID == (Guid)user.ProviderUserKey && x.LentaID == id);
            if (exist.Any())
            {
                db.UserFavoriteLentas.DeleteAllOnSubmit(exist);
            }
            else
            {
                db.UserFavoriteLentas.InsertOnSubmit(new UserFavoriteLenta()
                    {
                        UserID = (Guid)user.ProviderUserKey,
                        LentaID = id
                    });
            }
            db.SubmitChanges();
            return new ContentResult();

        }

    }


}