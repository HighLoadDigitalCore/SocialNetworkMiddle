using System.Web.Routing;
using Smoking.Extensions.Helpers;
using Smoking.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Security;

namespace Smoking.Extensions
{
    public class AuthorizeClient : AuthorizeAttribute
    {

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {

            return AccessHelper.IsAuthClient;
            //return base.AuthorizeCore(httpContext);
        }



        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var result = new ContentResult();
            result.Content = "<script type=\"text/javascript\">$().ready(function(){document.location.href = '/" +
                             AccessHelper.CurrentLang.ShortName + "'});</script>";

            filterContext.Result = result;
        }

    }

    public class AuthorizeMaster : AuthorizeAttribute
    {

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {

            return AccessHelper.IsMaster;
            //return base.AuthorizeCore(httpContext);
        }



        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            UrlHelper helper = new UrlHelper(filterContext.RequestContext);
            RouteValueDictionary routeValues = new RouteValueDictionary();
            var link = filterContext.HttpContext.Request.Url.PathAndQuery;
            if (
                !link.Split<string>("/")
                              .Any(z => AccessHelper.ShortLangList.Contains(z)))
            {
                link = "/Master/" + AccessHelper.CurrentLang.ShortName +
                       link.Split<string>("/").Where(x => x != "Master").JoinToString("/");
            }
            routeValues.Add("ReturnUrl",  HttpUtility.UrlPathEncode(link));
                                              


            string url = UrlHelper.GenerateUrl(
                "Master",
                "LogOn",
                "Account",
                routeValues,
                helper.RouteCollection,
                filterContext.RequestContext,
                true
                );


            filterContext.Result = new RedirectResult(url);

          
            //base.HandleUnauthorizedRequest(filterContext);
        }

    }
}

