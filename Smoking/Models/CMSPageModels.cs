using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Caching;
using Smoking.Extensions;
using Smoking.Extensions.Helpers;

namespace Smoking.Models
{
    [Serializable]
    public class JsTreeModel
    {
        public string data { get; set; }
        public JsTreeAttribute attr { get; set; }
        public List<JsTreeModel> children { get; set; }
    }

    [Serializable]
    public class JsTreeAttribute
    {

        public string id { get; set; }
        public string href { get; set; }
        public int uid { get; set; }
        public int priority { get; set; }

    }

    public partial class CMSPageCellView
    {
        private string _description;
        public string Description
        {
            get
            {
                if (_description.IsNullOrEmpty())
                {
                    _description = CMSPageCell.Description;
                }
                return _description;
            }
            set { _description = value; }
        }
    }



    [MetadataType(typeof(CMSPageDataAnnotations))]
    public partial class CMSPage
    {

        private IEnumerable<RoleInfo> _rolesList;
        public IEnumerable<RoleInfo> RolesList
        {
            get
            {
                if (_rolesList == null)
                {
                    var db = new DB();
                    var allRoles = db.Roles;
                    var list =
                        allRoles.AsEnumerable().Select(
                            x =>
                            new RoleInfo()
                                {
                                    RoleName = x.Description.IsNullOrEmpty() ? x.RoleName : x.Description,
                                    RoleID = x.RoleId,
                                    Selected = db.CMSPageRoleRels.Any(c => c.RoleID == x.RoleId && c.PageID == ID)
                                }).ToList();
                    list.Insert(0,
                                new RoleInfo()
                                    {
                                        RoleName = "Неавторизованные пользователи",
                                        RoleID = new Guid(),
                                        Selected = db.CMSPageRoleRels.Any(c => !c.RoleID.HasValue && c.PageID == ID)
                                    });
                    _rolesList = list;
                }
                return _rolesList;
            }
            set { _rolesList = value; }
        }


        public static List<CMSPage> GetPageTable(int langID)
        {
            List<CMSPage> _fullPageTable = null;
            var cached = HttpRuntime.Cache.Get("FullPageTable_" + langID);
            if (cached is List<CMSPage>)
            {
                _fullPageTable = cached as List<CMSPage>;
            }
            else
            {


                string shortName = new DB().Languages.First(x => x.ID == langID).ShortName;
                _fullPageTable =
                    new DB().getPageList(null, langID).AsEnumerable().Select(
                        x =>
                        new CMSPage()
                        {
                            PageName = x.PageName,
                            FullName = x.FullName,
                            FullUrl = "/" + shortName + x.FullURL,
                            Title = x.Title,
                            Keywords = x.Keywords,
                            Description = x.Description,
                            TreeLevel = x.TreeLevel ?? 0,
                            Type = x.Type ?? 0,
                            ParentID = x.ParentID,
                            ID = x.ID ?? 0,
                            URL = x.URL,
                            BreadCrumbs = x.BreadCrumbs,
                            LinkedBreadCrumbs = x.LinkedBreadCrumbs,
                            Visible = x.Visible ?? false,
                            ViewMenu = x.ViewMenu ?? false,
                            OrderNum = x.OrderNum ?? 0,
                        }).ToList();
                HttpRuntime.Cache.Insert("FullPageTable_" + langID,
                                         _fullPageTable, null,
                                         DateTime.Now.AddDays(1D),
                                         Cache.NoSlidingExpiration);
            }

            return _fullPageTable;
        }

        private static List<CMSPage> _fullPageTable;
        public static List<CMSPage> FullPageTable
        {
            get
            {
                //сначала кеш

                var cached = HttpRuntime.Cache.Get("FullPageTable_" + AccessHelper.CurrentLang.ID);
                if (cached is List<CMSPage>)
                {
                    _fullPageTable = cached as List<CMSPage>;
                }
                else
                {


                    string shortName = "/" + AccessHelper.CurrentLang.ShortName;

                    if (shortName == "/ru")
                        shortName = "";

                    _fullPageTable =
                        new DB().getPageList(null, AccessHelper.CurrentLang.ID).AsEnumerable().Select(
                            x =>
                            new CMSPage()
                                {
                                    PageName = x.PageName,
                                    FullName = x.FullName,
                                    FullUrl = shortName + x.FullURL,
                                    Title = x.Title,
                                    Keywords = x.Keywords,
                                    Description = x.Description,
                                    TreeLevel = x.TreeLevel ?? 0,
                                    Type = x.Type ?? 0,
                                    ParentID = x.ParentID,
                                    ID = x.ID ?? 0,
                                    URL = x.URL,
                                    BreadCrumbs = x.BreadCrumbs,
                                    LinkedBreadCrumbs = x.LinkedBreadCrumbs,
                                    Visible = x.Visible ?? false,
                                    ViewMenu = x.ViewMenu ?? false,
                                    OrderNum = x.OrderNum ?? 0,
                                }).ToList();
                    HttpRuntime.Cache.Insert("FullPageTable_" + AccessHelper.CurrentLang.ID,
                                             _fullPageTable, null,
                                             DateTime.Now.AddDays(1D),
                                             Cache.NoSlidingExpiration);
                }

                return _fullPageTable;
            }
            set
            {
                if (value == null)
                {
                    HttpRuntime.Cache.Remove("FullPageTable_" + AccessHelper.CurrentLang.ID);
                }
            }


        }

        public string PageName { get; set; }
        public string FullName { get; set; }
        public string Title { get; set; }
        public string Keywords { get; set; }
        public string Description { get; set; }

        public CMSPage LoadLangValues()
        {
            var lang = CMSPageLangs.FirstOrDefault(x => x.LanguageID == AccessHelper.CurrentLang.ID);
            if (lang != null)
            {
                this.LoadPossibleProperties(lang, new[] { "ID" });
            }
            return this;
        }

        public void SaveLangValues()
        {
            var db = new DB();
            var lang =
                db.CMSPageLangs.FirstOrDefault(x => x.CMSPageID == ID && x.LanguageID == AccessHelper.CurrentLang.ID);
            if (lang == null)
            {
                lang = new CMSPageLang() { CMSPageID = ID, LanguageID = AccessHelper.CurrentLang.ID };
                db.CMSPageLangs.InsertOnSubmit(lang);
            }
            lang.LoadPossibleProperties(this, new[] { "ID" });
            db.SubmitChanges();
        }

        public class CMSPageDataAnnotations
        {
            [Required(AllowEmptyStrings = false, ErrorMessage = "Поле '{0}' обязательно для заполнения"), DisplayName("Название раздела"), /*StringLength(100, ErrorMessage = "{0} должен содержать минимум {2} символов.", MinimumLength = 6)*/]
            public string PageName { get; set; }

            [Required(AllowEmptyStrings = false, ErrorMessage = "Поле '{0}' обязательно для заполнения"), DisplayName("URL раздела"), /*StringLength(100, ErrorMessage = "{0} должен содержать минимум {2} символов.", MinimumLength = 6)*/]
            [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)", ErrorMessage = "Поле '{0}' должно содержать только буквы английского алфавита и цифры")]
            public string URL { get; set; }

            [DisplayName("Заголовок страницы")]
            [Required(AllowEmptyStrings = false, ErrorMessage = "Поле '{0}' обязательно для заполнения")]
            public string FullName { get; set; }

            [DisplayName("Тайтл")]
            public string Title { get; set; }

            [DisplayName("Ключевые слова")]
            public string Keywords { get; set; }

            [DisplayName("Описание")]
            public string Description { get; set; }

            [DisplayName("Родительский раздел"), Required]
            public string ParentID { get; set; }

            [DisplayName("Отображать на сайте"), DefaultValue(true)]
            public string Visible { get; set; }

            [DisplayName("Отображать в меню"), DefaultValue(true)]
            public string ViewMenu { get; set; }

            [DisplayName("Тип раздела"), Required]
            public string Type { get; set; }
        }




        private List<string> _urlPath;
        public List<string> UrlPath
        {
            get
            {
                return _urlPath ?? (_urlPath = FullPageTable.First(x => x.URL == URL).FullUrl.Split<string>("/").ToList());
            }
            set { _urlPath = value; }

        }

        public IEnumerable<CMSPage> GetHalfOfChildren(bool left, int limit = 100)
        {
            var all = FullPageTable.Where(x => x.ParentID == ID).Take(limit).ToList();
            int leftCount = 0;
            int rem = 0;
            leftCount = Math.DivRem(all.Count(), 2, out rem);
            if (rem > 0)
                leftCount++;
            if (left)
                return all.Take(leftCount);
            return all.Skip(leftCount);

        }

        private int? _treeLevel;

        public int TreeLevel
        {
            get
            {
                if (!_treeLevel.HasValue)
                {
                    var item = FullPageTable.FirstOrDefault(x => x.ID == ID);
                    if (item == null)
                        return 0;
                    _treeLevel = item.TreeLevel;
                }
                return _treeLevel.Value;
            }
            set { _treeLevel = value; }
        }


        private string _fullUrl;
        public string FullUrl
        {
            get
            {
                if (_fullUrl.IsNullOrEmpty())
                    _fullUrl = string.Format("/{0}", string.Join("/", UrlPath.ToArray()));
                return _fullUrl;
            }
            set { _fullUrl = value; }
        }

        private string _breadCrumbs;
        public string BreadCrumbs
        {
            get
            {
                if (_breadCrumbs == null)
                {
                    var first = FullPageTable.First(x => x.ID == ID);
                    if (first == null)
                        _breadCrumbs = "";
                    else
                        _breadCrumbs = first.BreadCrumbs;
                }

                return _breadCrumbs;
            }
            set { _breadCrumbs = value; }
        }


        public bool IsCurrent
        {
            get { return AccessHelper.CurrentPageInfo.CurrentPage.ID == ID; }
        }

        private string _linkedBreadCrumbs;
        public string LinkedBreadCrumbs
        {
            get
            {
                if (_linkedBreadCrumbs == null)
                {
                    var first = FullPageTable.First(x => x.ID == ID);
                    if (first == null)
                        _linkedBreadCrumbs = "";
                    else
                    {
                        _linkedBreadCrumbs = first.LinkedBreadCrumbs;
                        _linkedBreadCrumbs =
                            string.Join("&mdash;",
                                        _linkedBreadCrumbs.Split(new[] { "&mdash;" },
                                                                 StringSplitOptions.RemoveEmptyEntries).Select(
                                                                     x =>
                                                                     x.Split(new[] { ";" },
                                                                             StringSplitOptions.RemoveEmptyEntries)).
                                            Select(
                                                x =>
                                                string.Format(x[1],
                                                              "/" +
                                                              FullPageTable.First(z => z.ID == int.Parse(x[0])).FullUrl)));
                    }
                }

                return _linkedBreadCrumbs;
            }
            set { _linkedBreadCrumbs = value; }
        }


        private void FillChildren(CMSPage cmsPage)
        {
            var childs = FullPageTable.Where(x => x.ParentID == cmsPage.ID).ToList();
            if (childs.Any())
                _fullChildrenList.AddRange(childs.Select(x => x.ID));
            foreach (var child in childs)
            {
                FillChildren(child);
            }
        }


        public string FullPath
        {
            get
            {
                if (URL == "catalog") return "Каталог";
                return string.Format("{0} &mdash; {1}", FullPageTable.First(x => x.URL == "catalog").PageName, BreadCrumbs);
            }
        }
        private List<int> _fullChildrenList;
        public List<int> FullChildrenList
        {
            get
            {
                if (_fullChildrenList == null)
                {
                    _fullChildrenList = new List<int>();
                    FillChildren(this);

                }
                return _fullChildrenList;
            }
            set { _fullChildrenList = value; }
        }

        public CMSPageTextData TextData
        {
            get
            {
                return CMSPageTextDatas.FirstOrDefault(x => x.LangID == AccessHelper.CurrentLang.ID) ??
                       new CMSPageTextData() { LangID = AccessHelper.CurrentLang.ID, Text = "", CMSPageID = ID };
            }
        }

        public string PageNameTranslated
        {
            get
            {
                if (PageName.IsNullOrEmpty() || PageName == "<noname>")
                {
                    var langID = new DB().Languages.First(x => x.ShortName == "ru").ID;
                    return
                        GetPageTable(langID)
                            .First(x => x.ID == ID)
                            .PageName;
                }
                return PageName;
            }
        }

        public IEnumerable<int> ShortChildrenList(int count)
        {
            var list = new List<int>();
            list.Add(ID);
            FillChildrenList(this, ref list, count + 1);
            return list.Take(count + 1);
        }

        private void FillChildrenList(CMSPage cmsPage, ref List<int> list, int count)
        {
            var childs = FullPageTable.Where(x => x.ParentID == cmsPage.ID).ToList();
            if (childs.Any())
                list.AddRange(childs.Select(x => x.ID));
            if (list.Count >= count)
                return;
            foreach (var child in childs)
            {
                FillChildrenList(child, ref list, count);
            }

        }


        public static void ClearAllCache()
        {
            var db = new DB();
            var langs = db.Languages;
            foreach (var lang in langs)
            {
                HttpRuntime.Cache.Remove("FullPageTable_" + lang.ID);
            }

        }

        public static CMSPage Get(string url)
        {
            return FullPageTable.FirstOrDefault(x => x.URL == url) ??
                   (GetByType("MainPage").FirstOrDefault() ?? new CMSPage() {URL = "main"});
        }
        public static CMSPage Get(int ID)
        {
            return FullPageTable.FirstOrDefault(x => x.ID == ID) ?? new CMSPage();
        }
        public static IEnumerable<CMSPage> GetByType(string typeName)
        {
            var type = new DB().PageTypes.FirstOrDefault(x => x.TypeName == typeName);
            return type == null ? new List<CMSPage>() : FullPageTable.Where(x => x.Type == type.ID).ToList();
        }

        public bool HasView(string viewName)
        {
            var db = new DB();
            return
                db.CMSPageCellViews.Any(
                    z => z.Action == viewName && z.CMSPageCell.PageType.CMSPages.Any(x => x.ID == ID));
        }
    }
}