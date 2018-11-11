using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace Smoking
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{*botdetect}", new { botdetect = @"(.*)BotDetectCaptcha\.ashx" });
            routes.IgnoreRoute("{*ckfinder}", new { ckfinder = @"(.*)ckfinder(.*)" });



            routes.MapRoute(
                name: "Master",
                url: "Master/{lang}/{controller}/{action}/{id}",
                defaults: new { controller = "MasterHome", action = "Index", lang = "ru", id = UrlParameter.Optional }
                );

/*
            routes.MapRoute(
                name: "UploadAvatar",
                url: "Master/Service/UploadAvatar",
                defaults: new { controller = "CommonBlocks", action = "AvatarUpload"}
                );
*/


            routes.MapRoute(
                name: "MasterListPaged",
                url: "Master/{lang}/{controller}/{action}/Page/{page}",
                defaults: new { controller = "MasterHome", action = "Index", lang = "ru", page = UrlParameter.Optional });


            routes.MapRoute(
                name: "Default",
                url: "{url1}/{url2}/{url3}/{url4}/{url5}/{url6}/{url7}/{url8}/{url9}",
                defaults: new
                    {
                        controller = "Selector",
                        action = "Index",
                        url1 = UrlParameter.Optional,
                        url2 = UrlParameter.Optional,
                        url3 = UrlParameter.Optional,
                        url4 = UrlParameter.Optional,
                        url5 = UrlParameter.Optional,
                        url6 = UrlParameter.Optional,
                        url7 = UrlParameter.Optional,
                        url8 = UrlParameter.Optional,
                        url9 = UrlParameter.Optional
                    }
                );

        }
    }
}