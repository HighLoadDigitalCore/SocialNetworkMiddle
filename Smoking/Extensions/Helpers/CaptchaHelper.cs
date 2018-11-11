using BotDetect;
using BotDetect.Web;
using BotDetect.Web.UI;
using BotDetect.Web.UI.Mvc;

namespace Smoking.Extensions.Helpers
{

    public class CaptchaHelper
    {
        public static MvcCaptcha GetRegistrationCaptcha()
        {
            // create the control instance
            MvcCaptcha registrationCaptcha = new MvcCaptcha("RegistrationCaptcha");
            registrationCaptcha.UserInputClientID = "CaptchaCode";

            // all Captcha settings have to be saved before rendering
            registrationCaptcha.SaveSettings();

            return registrationCaptcha;
        }
    }
}
