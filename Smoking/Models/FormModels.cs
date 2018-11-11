using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Smoking.Extensions;

namespace Smoking.Models
{

    public class  RestorePassForm:PostForm
    {
        [DisplayName("E-mail:")]
        [Required(ErrorMessage = "Поле '{0}' обязательно для заполнения", AllowEmptyStrings = false)]
        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage = "Пожалуйста укажите правильный Email адрес")]
        public string Email { get; set; }
    }

    public class PopupFeedBackForm
    {
        public BackCallForm BackCall { get; set; }
        public FeedBackForm FeedBack { get; set; }
    }

    public class AuthForm:PostForm
    {
        [DisplayName("E-mail:")]
        [Required(ErrorMessage = "Поле '{0}' обязательно для заполнения", AllowEmptyStrings = false)]
        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage = "Пожалуйста укажите правильный Email адрес")]
        public string Email { get; set; }

        [DisplayName("Пароль:")]
        [Required(ErrorMessage = "Поле '{0}' обязательно для заполнения", AllowEmptyStrings = false)]
        public string Password { get; set; }

        public string RedirectPage { get; set; }

    }

    public class PostForm
    {
        public bool IsSent { get; set; }
        public string ResultMessage { get; set; }
        public string LetterBody
        {
            get
            {
                string msg = "<table style=\"width:100%\">";
                var pl = this.GetType().GetProperties();
                foreach (PropertyInfo info in pl)
                {
                    if (info.Name != "IsSent" && info.Name != "ResultMessage" && info.Name != "LetterBody")
                    {
                        var attrName = info.GetCustomAttributes(true).FirstOrDefault(x => x is DisplayNameAttribute) as DisplayNameAttribute;
                        string name = "[Undefined]";
                        if (attrName != null)
                            name = attrName.DisplayName;
                        msg += "<tr><td><b>{0}&nbsp;&nbsp;&nbsp;</b></td><td>{1}&nbsp;&nbsp;&nbsp;</td></tr>".
                            FormatWith(name, info.GetValue(this, null).ToString());
                    }
                }
                msg += "</table>";
                return msg;
            }
        }
    }

    public class CommentForm:PostForm
    {
        [DisplayName("Ваше имя:")]
        public string Name { get; set; }

        [DisplayName("Email:")]
        public string Mail { get; set; }

        [DisplayName("Ваш отзыв:")]
        [Required(ErrorMessage = "Поле '{0}' обязательно для заполнения", AllowEmptyStrings = false)]
        public string Comment { get; set; }

    }

    public class BackCallForm : PostForm
    {
        [Required(ErrorMessage = "Поле '{0}' обязательно для заполнения", AllowEmptyStrings = false)]
        [DisplayName("Телефон:")]
        public string Phone { get; set; }
        [Required(ErrorMessage = "Поле '{0}' обязательно для заполнения", AllowEmptyStrings = false)]
        [DisplayName("Ваше имя:")]
        public string Name { get; set; }
    }

    public class FeedBackForm : PostForm
    {
        [DisplayName("Телефон:")]
        public string UserPhone { get; set; }
        
        [Required(ErrorMessage = "Поле '{0}' обязательно для заполнения", AllowEmptyStrings = false)]
        [DisplayName("Ваше имя:")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Поле '{0}' обязательно для заполнения", AllowEmptyStrings = false)]
        [DisplayName("E-mail:")]
        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage = "Пожалуйста укажите правильный Email адрес")]
        public string Mail { get; set; }

        [Required(ErrorMessage = "Поле '{0}' обязательно для заполнения", AllowEmptyStrings = false)]
        [DisplayName("Сообщение:")]
        public string Message { get; set; }

    }
}