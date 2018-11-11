using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using Smoking.Extensions.Helpers;
using BotDetect.Web.UI.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using System.Web;
using Smoking.Models;
using Smoking.Extensions;

namespace Smoking.Controllers
{

    [Authorize]
    public class AccountController : Controller
    {

        [HttpPost]
        public ActionResult SendLetter(Guid uid, string msg)
        {
            var sender = Membership.GetUser();
            if(sender==null)
                return new ContentResult(){Content = "Для отправки сообщений нужно авторизоваться на сайте"};

            var m = new UserMessage()
                {
                    Date = DateTime.Now,
                    Message = msg,
                    Sender = (Guid) sender.ProviderUserKey,
                    UserID = uid
                };

            var db = new DB();
            db.UserMessages.InsertOnSubmit(m);
            db.SubmitChanges();

            return new ContentResult();
        }


        [ClientTemplate("Редактирование профиля")]
        [HttpGet]
        public ActionResult SimpleProfile(Guid? uid)
        {
            var p = uid.HasValue
                        ? (new DB().UserProfiles.FirstOrDefault(x => x.UserID == uid) ?? AccessHelper.CurrentUserProfile)
                        : AccessHelper.CurrentUserProfile;

            return PartialView(p);
        }

        [HttpPost]
        public ActionResult SimpleProfile(Guid? uid, UserProfile profile, FormCollection collection)
        {
            var db = new DB();
            UserProfile p = null;
            if (uid.HasValue)
                p = db.UserProfiles.FirstOrDefault(x => x.UserID == uid);

            if (p == null)
                p = db.UserProfiles.FirstOrDefault(x => x.UserID == AccessHelper.CurrentUserKey);

            if (p == null)
            {
                ModelState.AddModelError("", "Пользователь не найден");
                return SimpleProfile(uid);
            }

            if (profile.NewPassword.Length < 6 && profile.NewPassword != p.Password)
            {
                ModelState.AddModelError("", "Длина пароля должна быть не менее 6 символов");
                PartialView(p);
            }
            if (!profile.Email.IsMailAdress())
            {
                ModelState.AddModelError("", "Необходимо указать корректный email адрес");
                PartialView(p);
            }



            p.LoadPossibleProperties(profile, new string[]{"ID"});
            if (p.Password != p.NewPassword)
                p.MembershipUser.ChangePassword(p.Password, p.NewPassword);

            profile.MembershipUser.Email = profile.Email;
            Membership.UpdateUser(profile.MembershipUser);

            db.SubmitChanges();
            ModelState.AddModelError("", "Данные успешно сохранены");
            return PartialView(p);
        }


        [HttpGet]
        [AllowAnonymous]
        public ActionResult SimpleLogin()
        {
            return PartialView(new AuthModel());
        }
        [HttpPost]
        [AllowAnonymous]
        public ActionResult SimpleLogin(AuthModel model)
        {
            if (model.Email.IsNullOrEmpty() || model.Password.IsNullOrEmpty())
            {
                model.ErrorText = "Необходимо указать Email и пароль";
            }
            else
            {
                var user = Membership.GetUser(model.Email);
                if (user == null)
                    model.ErrorText = "Пользователь с таким Email не зарегистрирован";
                else
                {
                    if (Membership.ValidateUser(model.Email, model.Password))
                    {
                        if (Roles.IsUserInRole(model.Email, "Client"))
                        {
                            FormsAuthentication.SetAuthCookie(model.Email, true);
                            model.NeedRedirect = true;
                            model.RedirectURL = CMSPage.Get(model.PageURL).FullUrl;
                        }
                        else
                        {
                            model.ErrorText = "Доступ через эту форму ограничен для администраторов сайта в целях безопасности";
                        }
                    }
                    else
                    {
                        model.ErrorText = "Неверный логин или пароль";
                    }
                }
            }
            return PartialView(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult SimpleRestore()
        {
            return PartialView(new RestoreModel());
        }
        [HttpPost]
        [AllowAnonymous]
        public ActionResult SimpleRestore(RestoreModel model)
        {
            if (model.Email.IsNullOrEmpty() || !model.Email.IsMailAdress())
            {
                model.ErrorText = "Для восстановления пароля необходимо указать Email, использованный при регистрации";
            }
            else
            {
                var user = Membership.FindUsersByEmail(model.Email);
                if (user.Count == 0)
                {
                    model.ErrorText = "Пользователь с таким Email не регистрировался на сайте";
                }
                else
                {
                    foreach (MembershipUser u in user)
                    {
                        var res = MailingList.Get("RestorePassLetter")
                                             .To(model.Email)
                                             .WithReplacement(new MailReplacement("{PASSWORD}", u.GetPassword())).Send();

                        model.ErrorText = res.IsFilled()
                                              ? res
                                              : "Пароль для доступа к сайту был успешно отправлен на указанный Email";

                    }
                }
            }

            return PartialView(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult SimpleRegister()
        {
            return PartialView(new AuthModel());
        }
        [HttpPost]
        [AllowAnonymous]
        public ActionResult SimpleRegister(AuthModel model)
        {
            if (model.Email.IsNullOrEmpty() || !model.Email.IsMailAdress())
            {
                model.ErrorText = "Для регистрации необходимо указать Email и пароль";
            }
            else
            {
                var users = Membership.FindUsersByEmail(model.Email);

                if (users.Count > 0)
                {
                    model.ErrorText = "Пользователь с таким Email уже регистрировался на сайте";
                }
                else
                {
                    var user = Membership.GetUser(model.Email);
                    if (user != null)
                    {
                        model.ErrorText = "Пользователь с таким Email уже регистрировался на сайте";
                    }
                    else
                    {
                        MembershipCreateStatus createStatus;

                        if (!model.Email.IsMailAdress())
                            createStatus = MembershipCreateStatus.InvalidEmail;
                        else
                            user = Membership.CreateUser(model.Email, model.Password, model.Email, null, null, true,
                                                         null, out createStatus);
                        if (createStatus == MembershipCreateStatus.Success)
                        {
                            Roles.AddUserToRole(user.UserName, "Client");
                            var profile = new UserProfile()
                                {
                                    UserID = (Guid)user.ProviderUserKey,
                                    FromIP = HttpContext.Request.GetRequestIP().ToIPInt(),
                                    RegDate = DateTime.Now,
                                    Email = model.Email,
                                };

                            var db = new DB();
                            db.UserProfiles.InsertOnSubmit(profile);
                            db.SubmitChanges();

                            MailingList.Get("RegisterLetter")
                                       .WithReplacement(
                                           new MailReplacement("{PASSWORD}", model.Password)
                                ).To(model.Email).Send();

                            FormsAuthentication.SetAuthCookie(model.Email, true);
                            model.RedirectURL = CMSPage.Get("profile").FullUrl;
                            model.NeedRedirect = true;

                        }
                        else model.ErrorText = ErrorCodeToString(createStatus);
                    }
                }
            }
            return PartialView(model);
        }

        private ActionResult ContextDependentView()
        {
            string actionName = ControllerContext.RouteData.GetRequiredString("action");
            if (Request.QueryString["content"] != null)
            {
                ViewBag.FormAction = "Json" + actionName;
                return PartialView();
            }
            ViewBag.FormAction = actionName;
            return View();
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            switch (createStatus)
            {
                case MembershipCreateStatus.InvalidUserName:
                    return "Логин (Email) указан некорректно. Исправьте его и повторите попытку.";

                case MembershipCreateStatus.InvalidPassword:
                    return "Пароль некорректный. Укажите другой пароль. Минимальная длина пароля - 6 символов";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidEmail:
                    return "E-mail указан некорректно. Проверьте e-mail адрес и попробуйте снова.";

                case MembershipCreateStatus.DuplicateUserName:
                    return "Пользователь с таким E-mail уже существует. Укажите другой E-mail.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "Пользователь с таким E-mail уже существует. Укажите другой E-mail.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
            return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
        }


        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            return Redirect("/Master");
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult LogOn()
        {
            if (HttpContext.User.Identity.IsAuthenticated && AccessHelper.IsMaster)
            {
                if (!Request["ReturnURL"].IsNullOrEmpty())
                {
                    return new RedirectResult(Request["ReturnURL"]);
                }
                return RedirectToAction("Index", AccessHelper.getStartUserController(HttpContext.User.Identity.Name), new { lang = AccessHelper.CurrentLang.ShortName });
            }
            return ContextDependentView();
        }

        [CaptchaValidation("CaptchaCode", "RegistrationCaptcha", "Вы должны указать символы, изображенные на картинке."), AllowAnonymous, HttpPost]
        public ActionResult LogOn(LogOnModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelErrorCollection captchaErr = ModelState["CaptchaCode"].Errors;
                if (captchaErr.Any())
                {
                    captchaErr.Clear();
                    captchaErr.Add("Вы должны указать символы, изображенные на картинке.");
                }
                return ContextDependentView();
            }
            if (Membership.ValidateUser(model.UserName, model.Password))
            {
                FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                if (!Request["ReturnURL"].IsNullOrEmpty())
                {
                    return new RedirectResult(Request["ReturnURL"]);
                }
                return RedirectToAction("Index", AccessHelper.getStartUserController(model.UserName), new { lang = AccessHelper.CurrentLang.ShortName });
            }
            return ContextDependentView();
        }


        [AuthorizeClient]
        [HttpGet]
        public new PartialViewResult Profile()
        {
            var db = new DB();
            var user = Membership.GetUser();
            var profile =
                db.UserProfiles.FirstOrDefault(x => x.UserID == (Guid)user.ProviderUserKey) ??
                new UserProfile();

            var model = new RegisterModel();
            model.LoadPossibleProperties(profile);
            model.NewPassword = user.GetPassword();
            return PartialView(model);
        }

        [AuthorizeClient]
        [HttpPost]
        public new PartialViewResult Profile(RegisterModel model)
        {
            /*          if (model.NewPassword.IsFilled() && model.NewPassword.Length < 6)
                      {
                          ModelState.AddModelError("", LabelDictionary.Translate("Пароль некорректный. Укажите другой пароль."));
                      }
                      else
                      {
                          var db = new DB();
                          var user = Membership.GetUser();
                          var profile = db.UserProfiles.FirstOrDefault(x => x.UserID == (Guid)user.ProviderUserKey);
                          if (profile == null)
                          {
                              profile = new UserProfile()
                                  {
                                      BonusSum = 0,
                                      ChipsSum = SiteSetting.Get<int>("StartChips"),
                                      MoneySum = 0,
                                      Email = user.Email,
                                      FromIP = HttpContext.Request.GetRequestIP().ToIPInt(),
                                      LastChipsAutoAdd = DateTime.Now
                                  };
                              db.UserProfiles.InsertOnSubmit(profile);
                          }
                          profile.LoadPossibleProperties(model);

                          if (model.NewPassword.IsFilled() && model.NewPassword != user.GetPassword())
                          {
                              user.ChangePassword(user.GetPassword(), model.NewPassword);
                          }
                          db.SubmitChanges();
                          ModelState.AddModelError("", LabelDictionary.Translate("Данные успешно сохранены"));
                      }
                      return PartialView(model);*/
            return PartialView();
        }

        [AllowAnonymous]
        [HttpGet]
        public PartialViewResult Register()
        {
            return PartialView();
            /*return PartialView(new RegisterModel());*/
        }

        [AllowAnonymous]
        [HttpPost]
        public PartialViewResult Register(RegisterModel model)
        {
            /* if (Session["LoginError"] != null)
             {
                 ModelState.AddModelError("", Session["LoginError"].ToString());
                 Session["LoginError"] = null;
             }
             if (ModelState.IsValid)
             {
                 if (!model.Agreed)
                 {
                     ModelState.AddModelError("", LabelDictionary.Translate("Для регистрации необходимо принять условия соглашения"));
                     return PartialView(model);
                 }
                 MembershipCreateStatus createStatus;
                 MembershipUser user = null;
                 if (!model.Email.IsMailAdress())
                     createStatus = MembershipCreateStatus.InvalidEmail;
                 else
                     user = Membership.CreateUser(model.Email, model.Password, model.Email, null, null, true, null, out createStatus);
                 if (createStatus == MembershipCreateStatus.Success)
                 {
                     Roles.AddUserToRole(user.UserName, "Client");
                     var profile = new UserProfile()
                         {
                             BonusSum = 0,
                             MoneySum = 0,
                             ChipsSum = SiteSetting.Get<int>("StartChips"),
                             LastChipsAutoAdd = DateTime.Now,
                             UserID = (Guid)user.ProviderUserKey,
                             FromIP = HttpContext.Request.GetRequestIP().ToIPInt(),
                             RegDate = DateTime.Now,
                             Email = model.Email,
                             MobilePhone = model.MobilePhone,
                             Nick = model.Nick,
                             Surname = model.Surname,
                             Name = model.Name,
                             Patrinomic = model.Patrinomic,
                             Rating = 0

                         };

                     var db = new DB();
                     db.UserProfiles.InsertOnSubmit(profile);
                     db.SubmitChanges();

                     MailingList.Get("RegisterLetter")
                                .WithReplacement(
                                    new MailReplacement("{PASSWORD}", model.Password)
                         ).To(model.Email).Send();

                     FormsAuthentication.SetAuthCookie(model.Email, false);
                     model.RedirectURL = HeaderInfo.StartPageForAuth.FullUrl;

                 }
                 else
                 {
                     ModelState.AddModelError("", LabelDictionary.Translate(ErrorCodeToString(createStatus)));
                 }
             }
             return PartialView(model);*/
            return PartialView();
        }

        [HttpGet]
        [AllowAnonymous]
        public PartialViewResult Restore()
        {
            return PartialView(new RestoreModel());
        }


        public ActionResult Exit()
        {
            FormsAuthentication.SignOut();
            Response.Redirect(CMSPage.Get("main").FullUrl);
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult Restore(RestoreModel model)
        {
            if (model.Email.IsNullOrEmpty() || !model.Email.IsMailAdress())
            {
                ModelState.AddModelError("",
                                         LabelDictionary.Translate(
                                             "Для восстановления пароля необходимо указать Email, использованный при регистрации"));
            }
            else
            {
                var user = Membership.GetUser(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("",
                                             LabelDictionary.Translate(
                                                 "Пользователь с таким Email не регистрировался на сайте"));
                }
                else
                {
                    var res = MailingList.Get("RestorePassLetter")
                                         .To(model.Email)
                                         .WithReplacement(new MailReplacement("{PASSWORD}", user.GetPassword())).Send();
                    ModelState.AddModelError("",
                                             res.IsFilled()
                                                 ? res
                                                 : LabelDictionary.Translate(
                                                     "Пароль для доступа к сайту был успешно отправлен на указанный Email"));
                }
            }
            return PartialView(model);
        }


    }
}

