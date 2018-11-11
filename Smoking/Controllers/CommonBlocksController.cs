using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Smoking.Extensions;
using Smoking.Extensions.Helpers;
using Smoking.Models;

namespace Smoking.Controllers
{
    public class CommonBlocksController : Controller
    {
        [ClientTemplate("Блок комментариев пользователя")]
        public PartialViewResult CommentsLenta(Guid? uid)
        {
            return
                PartialView(
                    new DB().Comments.Where(
                        x => x.UserID == (uid ?? AccessHelper.CurrentUserKey) && !x.ParentCommentID.HasValue)
                            .OrderByDescending(x => x.Date));

        }


        [HttpPost]
        [Authorize]
        public PartialViewResult Messeges(Guid UID, Guid Sender, string Message)
        {
            if (Sender != AccessHelper.CurrentUserKey)
            {
                ModelState.AddModelError("", "Пользователь не найден");

            }
            else if (Message.IsNullOrEmpty())
            {
                ModelState.AddModelError("", "Необходимо заполнить текст сообщения");
            }
            else
            {
                var msg = new UserMessage() { Date = DateTime.Now, Message = Message, Sender = Sender, UserID = UID };
                var db = new DB();
                db.UserMessages.InsertOnSubmit(msg);
                db.SubmitChanges();
            }

            return Messeges(UID);
        }

        [ClientTemplate("Блок информации о пользователе")]
        public ActionResult ProfileHeader(Guid? uid)
        {

            var profile = new DB().UserProfiles.FirstOrDefault(x => x.UserID == (uid ?? AccessHelper.CurrentUserKey)) ?? new UserProfile();
            return PartialView(profile);
        }

        [ClientTemplate("Меню в профиле")]
        public ActionResult ProfileMenu()
        {
            var parent = CMSPage.FullPageTable.First(x => x.URL == "userprofile");
            return
                PartialView(
                    CMSPage.FullPageTable.Where(x => x.ParentID == parent.ID && x.Visible && x.ViewMenu).OrderBy(x => x.OrderNum)
                           .Select(x => x.LoadLangValues()));
        }


        [ClientTemplate("Верхнее меню")]
        public ActionResult Header()
        {
            return PartialView(CMSPage.FullPageTable.Where(x => x.TreeLevel == 0 && x.ViewMenu && x.Visible));
        }

        [ClientTemplate("Личные сообщения")]
        public PartialViewResult Messeges(Guid? author)
        {
            var db = new DB();
            var chats = db.UserMessages.Where(
                x => x.UserID == AccessHelper.CurrentUserKey || x.Sender == AccessHelper.CurrentUserKey);


            var users =
                chats.Select(x => x.Poster)
                     .Concat(chats.Select(z => z.Recipient)).GroupBy(x => x.UserId).Select(x => x.First())
                     .ToList()
                     .OrderBy(x => x.UserProfile.FullName).Where(x => x.UserId != AccessHelper.CurrentUserKey);



            if (!author.HasValue && users.Any())
                author = users.First().UserId;

            if (!author.HasValue)
                author = new Guid();

            ViewBag.Authors = users;
            ViewBag.Target = author;

            var msgList = db.UserMessages.Where(x => (x.UserID == author && x.Sender == AccessHelper.CurrentUserKey) || (x.Sender == author && x.UserID == AccessHelper.CurrentUserKey)).OrderBy(x => x.Date);

            return PartialView(msgList);
        }


        [ClientTemplate("Выбор города")]
        public ActionResult MapSelect(int ViewID)
        {
            var db = new DB();
            var data = db.MapSelects.Where(x => x.Visible).OrderBy(x => x.OrderNum);
            return
                PartialView(data);
        }
        [ClientTemplate("Слайдер")]
        public ActionResult Slider(int ViewID)
        {
            var db = new DB();
            var info = AccessHelper.CurrentPageInfo;
            var data = db.CMSPageSliders.Where(
                x =>
                x.CMSPageID == info.CurrentPage.ID && x.LangID == info.CurrentLang.ID &&
                x.ViewID == ViewID).Where(x => x.Visible).OrderBy(x => x.OrderNum);
            return
                PartialView(data);
        }




        [ClientTemplate("Блок авторизации через соцсети")]
        public ActionResult AuthBtns()
        {
            return PartialView();
        }

        [ClientTemplate("Всплывающее окно авторизации, регистрации и восст. пароля")]
        public ActionResult AuthPopup()
        {
            return PartialView();
        }

        [ClientTemplate("Оранжевый блок с приглашением")]
        public ActionResult OrangeBlock()
        {
            return PartialView();
        }
    }
}