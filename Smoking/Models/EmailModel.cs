using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Web;
using Smoking.Extensions;
using Smoking.Extensions.Helpers;

namespace Smoking.Models
{

    public class Email
    {
        protected string MailTo { get; set; }
        protected string MailFrom { get; set; }
        protected string MailSubject { get; set; }
        protected string MailBody { get; set; }
        public List<string> Attachments { get; set; }
        public List<KeyValuePair<string, MemoryStream>> MemoryAttachments { get; set; }

        public Email To(string to)
        {
            MailTo = to;
            return this;
        }
        public Email From(string from)
        {
            MailFrom = from;
            return this;
        }
        public Email WithSubject(string subject)
        {
            MailSubject = subject;
            return this;
        }
        public Email WithBody(string body)
        {
            MailBody = body;
            return this;
        }
        /// <summary>
        /// Добавляет вложения
        /// </summary>
        /// <param name="attach">Список вложений, каждое вложение = ПОЛНЫЙ ПУТЬ ФИЗИЧЕСКОМУ ФАЙЛУ НА ДИСКЕ, НЕ URL!</param>
        /// <returns></returns>
        public Email WithAttachments(List<string> attach)
        {
            Attachments = attach;
            return this;
        }        
        /// <summary>
        /// Добавляет вложения
        /// </summary>
        /// <param name="attach">Список пар Название-MemoryStream</param>
        /// <returns></returns>
        public Email WithAttachments(List<KeyValuePair<string, MemoryStream>> attach)
        {
            MemoryAttachments = attach;
            return this;
        }

        public string Send()
        {
            string msg;
            Send(out msg);
            return msg;
        }
        public bool Send(out string result)
        {
            string message;
            var success = Send(MailTo, MailFrom, MailSubject, MailBody, Attachments ?? new List<string>(),
                               MemoryAttachments ?? new List<KeyValuePair<string, MemoryStream>>(), out message);
            result = message;
            return success;
        }

        protected static bool Send(string sendTo, string sendFrom, string sendSubject, string sendMessage, List<string> attachments, List<KeyValuePair<string, MemoryStream>> memAttaches, out string result)
        {
            try
            {
                bool bTest = sendTo.IsMailAdress();
                if (bTest == false)
                {
                    result = "Неправильно указан адрес: " + sendTo;
                    return false;
                }

                var message = new MailMessage(
                   sendFrom,
                   sendTo,
                   sendSubject,
                   sendMessage);
                message.IsBodyHtml = true;
                message.BodyEncoding = Encoding.UTF8;
                foreach (string attach in attachments)
                {
                    var attached = new Attachment(attach, MediaTypeNames.Application.Octet);
                    //attached.NameEncoding = Encoding.UTF8;
                    message.Attachments.Add(attached);
                }

                foreach (var pair in memAttaches)

                {
                    pair.Value.Position = 0;
                    var attached = new Attachment(pair.Value, pair.Key, MIMETypeWrapper.GetMIME(Path.GetExtension(pair.Key).Substring(1)));
                    //attached.NameEncoding = Encoding.UTF8;
                    message.Attachments.Add(attached);
                }


                // create smtp client at mail server location
                var client = new SmtpClient(SiteSetting.Get<string>("SMTP"));
                if (SiteSetting.Get<Int32>("SMTPPort") > 0)
                    client.Port = SiteSetting.Get<Int32>("SMTPPort");

                if (SiteSetting.Get<string>("SMTPLogin").IsNullOrEmpty() || SiteSetting.Get<string>("SMTPPass").IsNullOrEmpty())
                {
                    client.UseDefaultCredentials = true;
                }
                else
                {
                    client.Credentials = new NetworkCredential(SiteSetting.Get<string>("SMTPLogin"), SiteSetting.Get<string>("SMTPPass"));
                }

                client.Send(message);
                result = "";
                return true;

            }

            catch (Exception ex)
            {
                result = ex.Message;
                return false;

            }
        }
    }

}