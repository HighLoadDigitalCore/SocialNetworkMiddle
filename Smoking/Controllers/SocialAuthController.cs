using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;
using Smoking.Extensions;
using Smoking.Models;

namespace Smoking.Controllers
{
    [Authorize]
    public class SocialAuthController : Controller
    {
        private string DecodeEncodedNonAsciiCharacters(string value)
        {
            return Regex.Replace(
                value,
                @"\\u(?<Value>[a-zA-Z0-9]{4})",
                m => ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString());
        }

        [AllowAnonymous]
        public ActionResult Socials()
        {
            var from = Request["from"];
            if (from.IsNullOrEmpty())
                return PartialView();

            var target = String.Format("http://ulogin.ru/token.php?token={0}&host={1}", Request["token"],
                                       Request.Url.Host);

            var wc = new WebClient();
            byte[] data = null;
            try
            {
                data = wc.DownloadData(target);
            }
            catch (Exception exxxx)
            {
                Response.Redirect(CMSPage.Get("register").FullUrl);
            }
            var js = Encoding.UTF8.GetString(data);
            js = DecodeEncodedNonAsciiCharacters(js);
            var serializer = new JavaScriptSerializer();
            var jsData = serializer.Deserialize<UserDataFromNetwork>(js);

            if (string.IsNullOrEmpty(jsData.email))
            {
                Session["LoginError"] = "Для регистрации через соцсеть, в соцсети должен быть указан email";
                Response.Redirect(CMSPage.Get("register").FullUrl);
            }


            try
            {

                MembershipUser user = null;
                var exist = Membership.GetUserNameByEmail(jsData.email);
                if (!string.IsNullOrEmpty(exist))
                {
                    user = Membership.GetUser(exist);
                }

                //нет такого
                if (user == null)
                {
                    var pass = new Random(DateTime.Now.Millisecond).GeneratePassword(6);
                    //SiteExceptionLog.WriteToLog("Creating user = "+jsData.email);
                    user = Membership.CreateUser(jsData.email, pass, jsData.email);
                    Roles.AddUserToRole(user.UserName, "Client");

                    var profile = new UserProfile()
                    {
                        UserID = (Guid)user.ProviderUserKey,
                        FromIP = HttpContext.Request.GetRequestIP().ToIPInt(),
                        RegDate = DateTime.Now,
                        Email = jsData.email,

                    };

                    profile.Name = jsData.first_name ?? "";
                    profile.Surname = jsData.last_name ?? "";
                    profile.Nick = jsData.nickname;
                    byte[] avatar;
                    try
                    {
                        avatar = wc.DownloadData(jsData.photo_big.IsNullOrEmpty() ? jsData.photo : jsData.photo_big);
                    }
                    catch
                    {
                        avatar = null;
                    }
                    profile.Avatar = avatar;

                    var db = new DB();
                    db.UserProfiles.InsertOnSubmit(profile);
                    db.SubmitChanges();


                    MailingList.Get("RegisterLetter")
                               .WithReplacement(
                                   new MailReplacement("{PASSWORD}", pass)
                        ).To(jsData.email).Send();

                    FormsAuthentication.SetAuthCookie(jsData.email, true);
                }
                //есть чувак
                else
                {
                    //мыло подтверждено и совпало, логин совпал
                    if ((/*jsData.verified_email == 1 && */jsData.email.ToLower() == user.Email.ToLower()))
                    {
                        FormsAuthentication.SetAuthCookie(user.UserName, true);
                    }
                    //редирект на страницу с формой, где выводим сообщение
                    else
                    {
                        Session["LoginError"] = (jsData.nickname == user.UserName
                                                  ? "Пользователь с таким логином уже зарегистрирован. Пожалуйста, укажите другой логин."
                                                  : "Пользователь с таким Email уже зарегистрирован. Пожалуйста укажите другой Email");
                        Response.Redirect(CMSPage.Get("main").FullUrl);
                    }
                }

            }
            catch (Exception ex)
            {
                Session["LoginError"] = ex.Message;

            }
            Response.Redirect(CMSPage.Get(from).FullUrl);

            return PartialView();

        }

    }
}
