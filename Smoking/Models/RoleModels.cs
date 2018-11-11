using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Smoking.Models;
using Smoking.Extensions;
using Smoking.Extensions.Helpers;
namespace Smoking.Models
{
    public class RoleInfo
    {
        public Guid RoleID { get; set; }
        public bool CanEdit { get; set; }
        public string Description { get; set; }
        public string RoleName { get; set; }
        public bool Selected { get; set; }
    }

    [MetadataType(typeof(RoleMetaData))]
    public partial class Role
    {
        public bool CanDelete
        {
            get
            {
                return (RoleName != "Administrator");
            }
        }

        public bool CanEditUID
        {
            get
            {
                return (RoleName != "Administrator");
            }
        }
        public string FinalName
        {
            get
            {
                if (Description.IsNullOrEmpty())
                {
                    return RoleName;
                }
                return Description;
            }
        }

        public bool IsCreating
        {
            get
            {

                return (RoleId == new Guid());
            }
        }

        public class RoleMetaData
        {
            [Required(AllowEmptyStrings = false, ErrorMessage = "Необходимо указать название"), DisplayName("Краткое описание"), StringLength(0x100, ErrorMessage = "Длина поля '{0}' не должна превышать {1} символов")]
            public string Description { get; set; }

            [DisplayName("Идентификатор"), RegularExpression("[a-zA-Z0-9]+", ErrorMessage = "Идентификатор должен содержать только английские буквы и цифры"), Required(AllowEmptyStrings = false, ErrorMessage = "Необходимо указать идентификатор"), StringLength(0x100, ErrorMessage = "Длина поля '{0}' не должна превышать {1} символов")]
            public string RoleName { get; set; }
        }
    }
}