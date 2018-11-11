using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using Smoking.Extensions.Helpers;
using Smoking.Models;

namespace Smoking.Extensions
{


    public class PagedData<T> : List<T>
    {
        public PagedData(IQueryable<T> source, int pageIndex, int pageSize)
            : this(source, pageIndex, pageSize, "MasterListPaged", new RouteValueDictionary())
        {
        }
        public PagedData(IQueryable<T> source, int pageIndex, int pageSize, RouteValueDictionary dictionary, int totalCount)
            : this(source, pageIndex, pageSize, "MasterListPaged", dictionary, totalCount)
        {
        }
        public PagedData(IQueryable<T> source, int pageIndex, int pageSize, RouteValueDictionary dictionary)
            : this(source, pageIndex, pageSize, "MasterListPaged", dictionary)
        {
        }
        public PagedData(IQueryable<T> source, int pageIndex, int pageSize, string mapRuleName)
            : this(source, pageIndex, pageSize, mapRuleName, new RouteValueDictionary())
        {
        }
        public PagedData(IQueryable<T> source, int pageIndex, int pageSize, string mapRuleName, IEnumerable<FilterConfiguration> filters)
            
        {
            MapRuleName = mapRuleName;
            PageIndex = pageIndex;
            PageSize = pageSize;

            var dictionary = new RouteValueDictionary();
            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    var val = filter.ValueFromQuery == null ? "" : filter.ValueFromQuery.ToString();
                    if (val.IsFilled())
                        dictionary.Add(filter.QueryKey, filter.ValueFromQuery);
                }
            }
            DefaultRoutes = dictionary;
            Filters = filters;
            var filtered = AddRangeFiltered(source);
            TotalCount = filtered.Count();
            TotalPages = (int)Math.Ceiling((double)(((double)TotalCount) / ((double)PageSize)));

        }

        public PagedData(IQueryable<T> source, int pageIndex, int pageSize, string mapRuleName, RouteValueDictionary dictionary, int totalCount)
        {
            DefaultRoutes = dictionary;
            MapRuleName = mapRuleName;
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalCount = totalCount;
            TotalPages = (int)Math.Ceiling((double)(((double)TotalCount) / ((double)PageSize)));
            AddRangeFiltered(source);

        }

        private IQueryable<T> AddRangeFiltered(IQueryable<T> source)
        {
            var target = source;
            if (Filters != null && Filters.Any())
            {
                target = Filters.Where(x=> !x.SkipInQuery).Aggregate(target, (current, filter) => filter.ApplyToQuery(current));
            }
            AddRange(target.Skip((PageIndex * PageSize)).Take(PageSize).ToList());
            return target;
        }



        public PagedData(IQueryable<T> source, int pageIndex, int pageSize, string mapRuleName, RouteValueDictionary dictionary)
        {
            DefaultRoutes = dictionary;
            MapRuleName = mapRuleName;
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalCount = source.Count();
            TotalPages = (int)Math.Ceiling((double)(((double)TotalCount) / ((double)PageSize)));
            AddRangeFiltered(source);
        }

        public PagedData(IQueryable<T> source, int pageIndex, int pageSize, string mapRuleName, HttpRequestBase request, IEnumerable<string> queryFilter)
            : this(source, pageIndex, pageSize, mapRuleName)
        {
            var dictionary = new RouteValueDictionary();
            NameValueCollection query = request.QueryString;
            foreach (string key in query.Keys.Cast<string>().Where(queryFilter.Contains))
            {
                dictionary.Add(key, query[key]);
            }

            DefaultRoutes = dictionary;
        }

        public MvcHtmlString PagerMenu(HtmlHelper hhelper)
        {
            var content = "";

            content = string.Concat(new object[] { content, "<input type=\"hidden\" value=\"", PageIndex, "\" id=\"page\"/>" });
            if (TotalPages > 1)
            {

                content = content + "<div class=\"pager\">Страницы:&nbsp;";
                if (!DefaultRoutes.ContainsKey("page"))
                    DefaultRoutes.Add("page", 0);
                if (HasPreviousPage)
                {
                    DefaultRoutes["page"] = PageIndex - 1;
                    content = content + (hhelper.RouteLink("<<", MapRuleName, DefaultRoutes,
                                                           (AjaxPager
                                                                ? ((object)
                                                                   new
                                                                       {
                                                                           onclick =
                                                                       "return {0}({1})".FormatWith(PageJSFunction,
                                                                                                    (PageIndex - 1).
                                                                                                        ToString())
                                                                       })
                                                                : ((object)new { })).ToDictionary())) + "&nbsp;";
                }
                string links = "";
                string fmtInactive = "<span>{0}</span>";
                List<int> allowed = new List<int>();
                if (TotalPages < 15)
                    for (int i = 0; i < 15; i++)
                        allowed.Add(i);
                else
                {
                    int start = PageIndex - 8;
                    for (int i = start; i < PageIndex + 8; i++)
                    {
                        allowed.Add(i);
                    }
                    allowed = allowed.Where(x => x >= 0).ToList();
                    int additional = 15 - allowed.Count;
                    if (additional > 0)
                    {
                        int alm = allowed.Max();
                        int alma = allowed.Max() + additional;
                        for (int i = alm; i < alma; i++)
                        {
                            allowed.Add(i);
                        }
                    }
                    allowed = allowed.Where(x => x <= TotalPages).ToList();
                    if (!allowed.Any())
                        return new MvcHtmlString("");
                    additional = 15 - allowed.Count;
                    if (additional > 0)
                    {
                        int alm = allowed.Min();
                        int alma = allowed.Min() - additional;
                        for (int i = alm; i < alma; i--)
                        {
                            allowed.Add(i);
                        }
                    }

                }


                for (int i = 0; i < TotalPages; i++)
                {
                    if (!allowed.Contains(i)) continue;
                    if ((links.Length > 0) && (i < TotalPages))
                    {
                        links = links + "&nbsp;&nbsp;&nbsp;";
                    }
                    if (i.Equals(PageIndex))
                    {
                        links = links + string.Format(fmtInactive, i + 1);
                    }
                    else
                    {
                        DefaultRoutes["page"] = i;
                        int currentPage = i + 1;

                        if (!AccessHelper.IsMasterPage)
                        {
                            var info = AccessHelper.CurrentPageInfo;
                            if (info != null)
                            {
                                string paramList = string.Join("&",
                                                               DefaultRoutes.Where(
                                                                   x => !x.Key.StartsWith("url")).
                                                                   Select(x => string.Format("{0}={1}", x.Key, x.Value))
                                                                   .ToList());
                                links += string.Format("<a href=\"/{0}?{1}\">{2}</a>",
                                                       string.Join("/",
                                                                   info.Routes.Where(x => x.Key.Contains("url")).Select(
                                                                       x => x.Value).ToArray()), paramList, currentPage);
                            }
                        }
                        else
                        {
                            links = links + hhelper.RouteLink(currentPage.ToString(),
                                                              MapRuleName,
                                                              DefaultRoutes,
                                                              (AjaxPager
                                                                   ? ((object)
                                                                      new
                                                                          {
                                                                              onclick =
                                                                          "return {0}({1})".FormatWith(new string[]
                                                                                                       {
                                                                                                           PageJSFunction
                                                                                                           , i.ToString()
                                                                                                       })
                                                                          })
                                                                   : ((object)new { })).ToDictionary());

                        }

                    }
                }
                content = content + links;
                if (HasNextPage)
                {
                    content = content + "&nbsp;";
                    DefaultRoutes["page"] = PageIndex + 1;
                    content = content + hhelper.RouteLink(">>", MapRuleName, DefaultRoutes, (AjaxPager ? ((object)new { onclick = "return {0}({1})".FormatWith(PageJSFunction, (PageIndex + 1).ToString()) }) : ((object)new { })).ToDictionary());
                }
                content = content + "</div>";
            }
            return new MvcHtmlString(content);
        }

        public bool AjaxPager { get; set; }

        public RouteValueDictionary DefaultRoutes { get; set; }

        public bool HasNextPage
        {
            get
            {
                return ((PageIndex + 1) < TotalPages);
            }
        }

        public bool HasPreviousPage
        {
            get
            {
                return (PageIndex > 0);
            }
        }

        public string MapRuleName { get; set; }


        public int PageIndex { get; set; }

        public string PageJSFunction { get; set; }


        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }
        public int TotalPages { get; private set; }
        IEnumerable<FilterConfiguration> Filters { get; set; }
    }
}

