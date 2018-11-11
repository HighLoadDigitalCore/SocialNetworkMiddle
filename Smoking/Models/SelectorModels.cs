using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Smoking.Extensions;
using Smoking.Extensions.Helpers;

namespace Smoking.Models
{

    public class CommonPageInfo
    {

        public static CommonPageInfo InitFromQueryParams(string query)
        {
            var parts = query.Split<string>("?").ToList();
            return InitFromQueryParams(parts[0].Split<string>("/").ToList(),
                                       parts.Count == 2
                                           ? parts[1].Split<string>("?", "&")
                                                     .Select(x => x.Split<string>("=").ToArray())
                                                     .Where(x => x.Length == 2)
                                                     .Select(x => new KeyValuePair<string, string>(x[0], x[1])).ToList()
                                           : new List<KeyValuePair<string, string>>());
        }        
        public static CommonPageInfo InitFromQueryParams()
        {
            return InitFromQueryParams(HttpContext.Current.Request.RawUrl.Split<string>("/").ToList());
        }
        public static CommonPageInfo InitFromQueryParams(List<string> slashedParams)
        {
            return InitFromQueryParams(slashedParams,
                                       HttpContext.Current.Request.QueryString.ToString()
                                                  .Split<string>("?", "&")
                                                  .Select(x => x.Split<string>("=").ToArray()).Where(x => x.Length == 2)
                                                  .Select(x => new KeyValuePair<string, string>(x[0], x[1])).ToList());

        }
        public static CommonPageInfo InitFromQueryParams(List<string> allSlashedParams, List<KeyValuePair<string, string>> queryParams)
        {

            var db = new DB();
            var url = "";

            var slashedParams =
                allSlashedParams.Where(x => AccessHelper.CurrentLang.AvailableList.All(z => z.ShortName != x)).ToList();

            //var request = HttpContext.Current.Request;
            url = slashedParams.All(x => x.IsNullOrEmpty())
                      ? db.CMSPages.First(x => x.PageType.TypeName == "MainPage").URL
                      : slashedParams.Last(x => !x.IsNullOrEmpty());



            var routes = new RouteValueDictionary();
            CommonPageInfo info;
            if (slashedParams.Any() && slashedParams[0] == "Master")
            {
                var offset = AccessHelper.CurrentLang.AvailableList.Any(z => slashedParams.Contains(z.ShortName)) ? 0:1;
                routes = ((MvcHandler) HttpContext.Current.Handler).RequestContext.RouteData.Values;
                if (routes.ContainsKey("url"))
                    return InitFromQueryParams(routes["url"].ToString());
                info = new CommonPageInfo()
                    {
                        Controller = slashedParams[2 - offset],
                        Action = slashedParams[3 - offset],
                        CurrentPage = db.CMSPages.First(x => x.PageType.TypeName == "MainPage"),
                        CurrentLang = AccessHelper.CurrentLang,
                        Layout = "_Master",
                        Routes = ((MvcHandler)HttpContext.Current.Handler).RequestContext.RouteData.Values
                    };
                return info;
            }

            var pathPairs =
                slashedParams.Where(x => !x.IsNullOrEmpty()).Select((x, index) => new { Key = "url" + (index + 1), Value = x }).ToList();

            foreach (var pair in pathPairs)
            {
                routes.Add(pair.Key, pair.Value);
            }
            foreach (var pair in queryParams)
            {
                routes.Add(pair.Key, pair.Value);
            }
            var cmsPage = db.CMSPages.FirstOrDefault(x => x.URL.ToLower() == url.ToLower());
            if (cmsPage == null || (slashedParams.Any() && slashedParams[0] == "404"))
            {
                info = new CommonPageInfo
                    {
                        URL = "404",
                        Action = "NotFound",
                        Controller = "TextPage",
                        CurrentPage = db.CMSPages.First(x=> x.PageType.TypeName == "MainPage"),
                        CurrentLang = AccessHelper.CurrentLang,
                        Routes = ((MvcHandler)HttpContext.Current.Handler).RequestContext.RouteData.Values
                    };
                
            }
            else
            {
                cmsPage.LoadLangValues();
                info = new CommonPageInfo()
                {
                    ID = cmsPage.ID,
                    URL = url,
                    CurrentPage = cmsPage,
                    Routes = routes,
                    CurrentLang = AccessHelper.CurrentLang
                };
                info.CurrentPage.Title = cmsPage.Title.IsNullOrEmpty() ? cmsPage.PageName : cmsPage.Title;
            }
           
            info.Layout = "MainPage";
            if (cmsPage != null)
            {
                var currentRole = AccessHelper.CurrentRole;
                var rel = db.CMSPageRoleRels.FirstOrDefault(
                    x =>
                    x.PageID == cmsPage.ID &&
                    (!HttpContext.Current.User.Identity.IsAuthenticated || currentRole.IsNullOrEmpty()
                         ? !x.RoleID.HasValue
                         : x.Role.RoleName == currentRole));

                if (rel == null)
                {
                    info.Controller = "TextPage";
                    info.Action = "AccessDenied";
                    info.Layout = "MainPage";
                }
            }
            return info;
        }

        public int ID { get; set; }
        public string URL { get; set; }
        public string Action { get; set; }
        public string Controller { get; set; }
        public CMSPage CurrentPage { get; set; }
        public RouteValueDictionary Routes { get; set; }
        public Language CurrentLang { get; set; }
        public string Layout { get; set; }

        private string _keywords;
        public string Keywords
        {
            get
            {
                if (_keywords.IsNullOrEmpty())
                {
                    _keywords = CurrentPage == null ? "" : CurrentPage.Keywords;
                }
                return _keywords;
            }
            set { _keywords = value; }
        }

        private string _description;
        public string Description
        {
            get
            {
                if (_description.IsNullOrEmpty())
                {
                    _description = CurrentPage == null ? "" : CurrentPage.Description;
                }
                return _description;
            }
            set { _description = value; }
        }

        private string _title;
        public string Title
        {
            get
            {
                if (_title.IsNullOrEmpty())
                {
                    _title = CurrentPage == null ? "Страница не найдена" : CurrentPage.Title;
                }
                _title = SiteSetting.Get<string>("CommonTitle").Trim() + " "+ _title;
                return _title;
            }
            set { _title = value; }
        }


        public string CurrentPageType
        {
            get
            {
                if (CurrentPage == null)
                    return "TextPage";
                return CurrentPage.PageType.TypeName;
            }

        }

    }

}