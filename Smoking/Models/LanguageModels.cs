using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Smoking.Extensions;
using Smoking.Extensions.Helpers;

namespace Smoking.Models
{
    [MetadataType(typeof(LanguageDataAnnotations))]
    public partial class Language
    {
        private SelectList _languageList;
        public SelectList LanguageList
        {
            get
            {
                //      if (_languageList == null)
                {
                    var db = new DB();
                    var selected = db.Languages.First(x => x.ByDef).ShortName;
                    var cook = HttpContext.Current.Request.Cookies["MasterLang"];
                    if (cook != null)
                        selected = cook.Value;
                    _languageList = new SelectList(db.Languages.OrderBy(x => x.Ordernum), "ShortName", "Name", selected);
                }
                return _languageList;
            }
        }
        public SelectList LanguageListWithUrl
        {
            get
            {
                var db = new DB();
                var languageListWithUrl = new SelectList(db.Languages.Where(x=> x.Enabled).OrderBy(x => x.Ordernum).ToList(),
                                                          "CurrentPageURL", "Name",
                                                          AccessHelper.CurrentLang.CurrentPageURL);
                return languageListWithUrl;
            }
        }

        private int? _width;
        public int Width
        {
            get
            {
                if (!_width.HasValue)
                {
                    if (Icon == null) return 0;
                    try
                    {
                        var ms = new MemoryStream(Icon.ToArray());
                        ms.Seek(0L, SeekOrigin.Begin);
                        var bmp = new Bitmap(ms);
                        _width = bmp.Width;
                        _height = bmp.Height;
                    }
                    catch
                    {
                        return 0;
                    }
                }
                return _width.Value;
            }
        }

        private int? _height;
        public int Height
        {
            get
            {
                if (!_height.HasValue)
                {
                    if (Icon == null) return 0;
                    try
                    {
                        var ms = new MemoryStream(Icon.ToArray());
                        ms.Seek(0L, SeekOrigin.Begin);
                        var bmp = new Bitmap(ms);
                        _width = bmp.Width;
                        _height = bmp.Height;
                    }
                    catch
                    {
                        return 0;
                    }

                }
                return _height.Value;
            }
        }

        public int getProperWidth(int width)
        {
            if (Width < width) return Width;
            return width;
        }
        public int getProperHeight(int width)
        {
            if (Width <= width)
                return Height;

            return (int)((((decimal)width / (decimal)Width)) * Height);
        }

        public string Image
        {
            get
            {
                var rq = HttpContext.Current.Request.RequestContext;
                var helper = new UrlHelper(rq);
                var routeValues = new RouteValueDictionary { { "ID", ID } };
                string url = UrlHelper.GenerateUrl(
                    "Master",
                    "Flag",
                    "Languages",
                    routeValues,
                    helper.RouteCollection,
                    rq,
                    true
                    );
                return url;
            }
        }

        private IEnumerable<Language> _availableList;
        public IEnumerable<Language> AvailableList
        {
            get
            {

                return _availableList ??
                       (_availableList = new DB().Languages.Where(x => x.Enabled).OrderBy(x => x.Ordernum).ToList());
            }
        }

        private string _currentPageURL;
        public string CurrentPageURL
        {
            get
            {
                //if (_currentPageURL.IsNullOrEmpty())
                {
                    _currentPageURL = HttpContext.Current.Request.RawUrl;
                    foreach (var lang in AvailableList)
                    {
                        _currentPageURL = _currentPageURL.Replace("/" + lang.ShortName + "/", "");
                    }
                    _currentPageURL = "/" + ShortName + "/" + _currentPageURL;
                    _currentPageURL = _currentPageURL.Replace("//", "/");
                }
                return _currentPageURL;
            }
        }

        public string GetPreview(int width)
        {
            return string.Format("{0}?width={1}", Image, width);
        }
    }

    public class LanguageDataAnnotations
    {

        [DisplayName("Название языка")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Поле '{0}' обязательно для заполнения")]
        public string Name { get; set; }

        [DisplayName("Псевдоним")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Поле '{0}' обязательно для заполнения")]
        [RegularExpression(@"[a-z]{2}", ErrorMessage = "Поле должно содержать 2-х буквенный код латинскими буквами в нижнем регистре")]
        public string ShortName { get; set; }

        [DisplayName("Язык используется на сайте")]
        public bool Enabled { get; set; }

        [DisplayName("Язык по умолчанию")]
        public bool ByDef { get; set; }

        [DisplayName("Флаг")]
        public byte[] Icon { get; set; }

    }
}