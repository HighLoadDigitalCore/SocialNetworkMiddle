using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Smoking.Models
{

    public class RefillModel
    {
        [RegularExpression("[,\\d\\.]+", ErrorMessage = "Пожалуйста укажите число")]
        public decimal Sum { get; set; }
    }


    [Serializable]
    public class UserDataFromNetwork
    {
        public string network { get; set; }
        public string identity { get; set; }
        public string uid { get; set; }
        public string email { get; set; }
        public string nickname { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string bdate { get; set; }
        public string photo { get; set; }
        public string photo_big { get; set; }
        public string city { get; set; }
        public string profile { get; set; }
        public int verified_email { get; set; }
        public int sex { get; set; }

    }


    public class RegisterModel
    {
        /*
                [Display(Name = "Подтвердите пароль"), DataType(DataType.Password), System.Web.Mvc.Compare("Password", ErrorMessage = "Пароли не совпадают.")]
                public string ConfirmPassword { get; set; }
        */

        [DataType(DataType.EmailAddress), Required(AllowEmptyStrings = false), Display(Name = "Email"), RegularExpression(@"^([\w\!\#$\%\&\'\*\+\-\/\=\?\^\`{\|\}\~]+\.)*[\w\!\#$\%\&\'\*\+\-\/\=\?\^\`{\|\}\~]+@((((([a-zA-Z0-9]{1}[a-zA-Z0-9\-]{0,62}[a-zA-Z0-9]{1})|[a-zA-Z])\.)+[a-zA-Z]{2,6})|(\d{1,3}\.){3}\d{1,3}(\:\d{1,5})?)$", ErrorMessage = "Необходимо указать Email")]
        public string Email { get; set; }

        [Required(AllowEmptyStrings = false), Display(Name = "Пароль"), DataType(DataType.Password), StringLength(100, ErrorMessage = "{0} должен содержать минимум {2} символов.", MinimumLength = 6)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Display(Name = "Имя для отображения")]
        public string Nick { get; set; }
        [Display(Name = "Фамилия")]
        public string Surname { get; set; }
        [Display(Name = "Имя")]
        public string Name { get; set; }
        [Display(Name = "Отчество")]
        public string Patrinomic { get; set; }
        [Display(Name = "Телефон")]
        public string MobilePhone { get; set; }

        public string RedirectURL { get; set; }

        public bool Agreed { get; set; }
    }
    public class LogOnModel
    {
        [Display(Name = "Введите символы, указанные на картинке"), Required(ErrorMessage = "*")]
        public string CaptchaCode { get; set; }

        [Required(ErrorMessage = "Необходимо заполнить поле \"{0}\""), DataType(DataType.Password), Display(Name = "Пароль")]
        public string Password { get; set; }

        [Display(Name = "Запомнить меня?")]
        public bool RememberMe { get; set; }

        [Required(ErrorMessage = "Необходимо заполнить поле \"{0}\""), Display(Name = "Логин")]
        public string UserName { get; set; }
    }

    public class ChangePasswordModel
    {
        [Display(Name = "Подтвердите пароль"), System.Web.Mvc.Compare("NewPassword", ErrorMessage = "Пароли не совпадают."), DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Новый пароль"), DataType(DataType.Password), StringLength(100, ErrorMessage = "{0} должен содержать минимум {2} символов.", MinimumLength = 6), Required]
        public string NewPassword { get; set; }

        [DataType(DataType.Password), Required, Display(Name = "Текущий пароль")]
        public string OldPassword { get; set; }
    }

    public class AuthModel : CommonFormModel
    {
        public string PageURL { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool Remember { get; set; }
        public string RedirectURL { get; set; }
    }

    public class RestoreModel:CommonFormModel
    {
        public string Email { get; set; }
    }

    public class CommonFormModel
    {
        public string ErrorText { get; set; }
        public bool NeedRedirect { get; set; }
    }
}