using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Smoking.Extensions;
using Smoking.Extensions.Helpers;

namespace Smoking.Models
{
    public class MailReplacement
    {
        public string Key { get; set; }
        public string Text { get; set; }
        public MailReplacement(string key, string text)
        {
            Key = key;
            Text = text;
        }
    }

    [MetadataType(typeof(MailingListDataAnnotations))]
    public partial class MailingList
    {
        public List<string> Attachments { get; set; }
        public List<KeyValuePair<string, MemoryStream>> MemoryAttachments { get; set; }

        private List<MailReplacement> _replacements;

        public class MailingListDataAnnotations
        {

            [Required(AllowEmptyStrings = false, ErrorMessage = "Поле '{0}' обязательно для заполнения")]
            [DisplayName("Тема письма")]
            public string Header { get; set; }

            [DisplayName("Email")]
            public string TargetMail { get; set; }

            [Required(AllowEmptyStrings = false, ErrorMessage = "Поле '{0}' обязательно для заполнения")]
            [DisplayName("Содержимое письма")]
            public string Letter { get; set; }
        }


        public static MailingList Get(string key)
        {
            var db = new DB();
            return db.MailingLists.FirstOrDefault(x => x.LetterKey == key);
        }

        public MailingList WithReplacement(MailReplacement replacement)
        {
            if (_replacements == null)
                _replacements = new List<MailReplacement>();
            _replacements.Add(replacement);
            return this;
        }
        public MailingList WithReplacements(List<MailReplacement> replacements)
        {
            if (_replacements == null)
                _replacements = new List<MailReplacement>();
            _replacements.AddRange(replacements);
            return this;
        }

        public MailingList To(string targetMail)
        {
            TargetMail = targetMail;
            return this;
        }

        public string Send()
        {
            if (!Enabled) return "Рассылка отключена";

            if (TargetMail.IsNullOrEmpty())
                return !IsForAdmin ? "Не указан адрес рассылки." : "";

            _replacements.AddRange(DefaultReplacements);
            var mail = new Email();
            if (Attachments != null)
                mail = mail.WithAttachments(Attachments);
            if (MemoryAttachments != null)
                mail = mail.WithAttachments(MemoryAttachments);
            return
                mail.WithSubject(FillWithReplacements(Header))
                    .WithBody(FillWithReplacements(Letter))
                    .From(SiteSetting.Get<string>("SMTPLogin"))
                    .To(TargetMail)
                    .Send();

        }

        private string FillWithReplacements(string text)
        {
            return _replacements.Aggregate(text, (current, replacement) => current.Replace(replacement.Key, replacement.Text));
        }

        private List<MailReplacement> _defaultReplacements;
        private IEnumerable<MailReplacement> DefaultReplacements
        {
            get
            {
                return _defaultReplacements ?? (_defaultReplacements = new List<MailReplacement>
                    {
                        new MailReplacement("{SITENAME}", AccessHelper.SiteName.ToNiceForm()),
                        new MailReplacement("{SITEURL}", AccessHelper.SiteUrl.ToNiceForm())
                    });
            }
        }
    }


}