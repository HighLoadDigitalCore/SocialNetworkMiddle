using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Smoking.Extensions;
using Smoking.Extensions.Helpers;

namespace Smoking.Models
{


    public partial class User
    {
        public bool CanDelete
        {
            get { return UserName.ToLower() != "admin"; }
        }

        public UserProfile Profile
        {
            get
            {
                if (UserProfile == null)
                    return new UserProfile() { User = this };
                return UserProfile;
            }

        }
    }


    [MetadataType(typeof(ProfileDataAnnotations))]
    public partial class UserProfile
    {


        public static UserProfile Get(Guid uid)
        {
            return new DB().UserProfiles.FirstOrDefault(x => x.UserID == uid) ?? new UserProfile();
        }

        public string SmokingStatus
        {
            get
            {
                if (!SmokingType.HasValue)
                    return "";
                if (SmokingType.Value == -1)
                    return "Не курит";
                if (SmokingType.Value == 0)
                    return "Бросает";
                if (SmokingType.Value == 1)
                    return "Курит";

                return "";
            }
        }


        private int? _width;
        public int Width
        {
            get
            {
                if (!_width.HasValue)
                {
                    if (Avatar == null) return 0;
                    try
                    {
                        var ms = new MemoryStream(Avatar.ToArray());
                        ms.Seek(0L, SeekOrigin.Begin);
                        var bmp = new Bitmap(ms);
                        _width = bmp.Width;
                        _height = bmp.Height;
                    }
                    catch
                    {
                        return 0;
                    }
                }
                return _width.Value;
            }
        }

        private int? _height;
        public int Height
        {
            get
            {
                if (!_height.HasValue)
                {
                    if (Avatar == null) return 0;
                    try
                    {
                        var ms = new MemoryStream(Avatar.ToArray());
                        ms.Seek(0L, SeekOrigin.Begin);
                        var bmp = new Bitmap(ms);
                        _width = bmp.Width;
                        _height = bmp.Height;
                    }
                    catch
                    {
                        return 0;
                    }

                }
                return _height.Value;
            }
        }

        public int getProperWidth(int width)
        {
            if (Width < width) return Width;
            return width;
        }
        public int getProperHeight(int width)
        {
            if (Width <= width)
                return Height;

            return (int)((((decimal)width / (decimal)Width)) * Height);
        }

        public string GetAvatarLink(int width)
        {
            var rq = HttpContext.Current.Request.RequestContext;
            var helper = new UrlHelper(rq);
            var routeValues = new RouteValueDictionary { { "UID", UserID }, { "Width", width } };
            string url = UrlHelper.GenerateUrl(
                "Master",
                "Avatar",
                "Users",
                routeValues,
                helper.RouteCollection,
                rq,
                true
                );
            return url;
        }

        public string EditProfilePage
        {
            get { return (CMSPage.Get("myobjects") ?? CMSPage.Get("main")).FullUrl + "?uid=" + UserID.ToString(); }
        }

        public string NickOrNameOrMail
        {
            get
            {
                if (Nick.IsFilled())
                    return Nick;
                if (Name.IsFilled())
                    return Name;
                if (MembershipUser != null)
                    return MembershipUser.Email;
                return "";
            }
        }



        public IEnumerable<RoleInfo> RolesList
        {
            get
            {
                var db = new DB();
                var allRoles = db.Roles;
                return
                    allRoles.AsEnumerable().Select(
                        x =>
                        new RoleInfo()
                            {
                                RoleName = x.RoleName,
                                RoleID = x.RoleId,
                                Selected = User.UsersInRoles.Select(z => z.RoleId).Any(c => c == x.RoleId)
                            });
            }
        }

        public string NewPassword { get; set; }


        private string _login;
        public string Login
        {
            get
            {
                if (_login.IsNullOrEmpty())
                {
                    if (MembershipUser != null)
                        _login = MembershipUser.UserName;
                }
                return _login;
            }
            set { _login = value; }
        }

        public string ShortNameOrNick
        {
            get
            {
                string ret = "";
                if (Nick.IsNullOrEmpty())
                {
                    ret += Surname;
                    if (Name.IsFilled())
                        ret += " " + Name.Substring(0, 1).ToUpper() + ".";

                }
                else
                {
                    ret = Nick;
                }
                if (ret.IsNullOrEmpty())
                    ret = "[Anonimous]";
                return ret;
            }
        }

        public string SurnameAndName
        {
            get
            {
                return "{0} {1}".FormatWith(new string[] { Surname, Name });
            }
        }
        public string FullName
        {
            get
            {
                var name = "{0} {1} {2}".FormatWith(new string[] { Name, Patrinomic, Surname });
                if (name.IsNullOrEmpty()) return "[Anonimous]";
                return name;
            }
        }

        private MembershipUser user = null;
        public MembershipUser MembershipUser
        {
            get
            {
                if (user == null)
                {

                    user = Membership.GetUser(UserID);
                }
                return user;
            }
            set { user = value; }
        }
        private string _mail;
        public string Email
        {
            get
            {
                if (!_mail.IsNullOrEmpty())
                {
                    return _mail;
                }
                if (MembershipUser != null)
                {
                    return MembershipUser.Email;
                }
                return "";
            }
            set
            {
                _mail = value;
            }
        }
        public string Password
        {
            get
            {
                if (MembershipUser != null)
                {
                    return MembershipUser.GetPassword();
                }
                return NewPassword;
            }
            set
            {
                NewPassword = value;
            }
        }

        public string UserCommentLink
        {
            get
            {
                return (CMSPage.GetByType("ProfileComments").FirstOrDefault() ?? CMSPage.Get("main")).FullUrl + "?uid=" +
                       (HttpContext.Current.Request["uid"].IsNullOrEmpty()
                            ? AccessHelper.CurrentUserKey.ToString()
                            : HttpContext.Current.Request["uid"]);
            }
        }

        public string UserPointsLink
        {
            get { return CMSPage.Get("myobjects").FullUrl + "?uid=" + UserID + "&type=0"; }
        }
        public string UserZonesLink
        {
            get { return CMSPage.Get("myobjects").FullUrl + "?uid=" + UserID + "&type=1"; }
        }

        public CMSPage EditProfileCMSPage
        {
            get { return CMSPage.Get("userprofile") ?? CMSPage.Get("main"); }
        }

        public class ProfileDataAnnotations
        {

            [Required(AllowEmptyStrings = false, ErrorMessage = "Поле '{0}' обязательно для заполнения")]
            [DisplayName("Пароль *"), StringLength(100, ErrorMessage = "{0} должен содержать минимум {2} символов.", MinimumLength = 6)]
            public string Password { get; set; }


            [DisplayName("Логин *")]
            [Required(AllowEmptyStrings = false, ErrorMessage = "Поле '{0}' обязательно для заполнения")]
            public string Login { get; set; }

            [DisplayName("Email*")]
            [Required(AllowEmptyStrings = false, ErrorMessage = "Поле '{0}' обязательно для заполнения")]
            [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage = "Пожалуйста укажите правильный Email адрес")]
            public string Email { get; set; }

            [Required(AllowEmptyStrings = false, ErrorMessage = "Поле '{0}' обязательно для заполнения")]
            [DisplayName("Имя")]
            public string Name { get; set; }

            [DisplayName("Фамилия")]
            public string Surname { get; set; }

            [DisplayName("Отчество")]
            public string Patrinomic { get; set; }

            [DisplayName("Телефон")]
            public string HomePhone { get; set; }

            [DisplayName("Мобильный телефон")]
            public string MobilePhone { get; set; }



        }

    }
}