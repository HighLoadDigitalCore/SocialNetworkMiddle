using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Smoking.Extensions;
using Smoking.Extensions.Helpers;

namespace Smoking.Models
{

    public partial class Comment
    {
        private string _commentedObjectLink;
        public string CommentedObjectLink
        {
            get
            {
                if (_commentedObjectLink.IsNullOrEmpty())
                {
                    if (LentaComments.Any())
                    {
                        _commentedObjectLink = LentaComments.First().Lenta.Href + "#comments";
                    }
                    if (MapObjectComments.Any())
                    {
                        _commentedObjectLink = MapObjectComments.First().MapObject.CommentPageLink;
                    }
                }
                return _commentedObjectLink;
            }
        }

        private string _commentedObject;
        public string CommentedObject
        {
            get
            {
                if (_commentedObject.IsNullOrEmpty())
                {
                    if (LentaComments.Any())
                    {
                        var head = LentaComments.First().Lenta.HeaderText;
                        if (head.IsNullOrEmpty())
                            head = LentaComments.First().Lenta.Author;

                        _commentedObject = head;

                    }
                    if (MapObjectComments.Any())
                    {
                        _commentedObject = MapObjectComments.First().MapObject.Name + " (" +
                                           MapObjectComments.First().MapObject.Address + ")";
                    }
                }
                return _commentedObject;
            }
        }
    }

    public class LentaViewModel
    {
        public bool IsFullText { get; set; }
        public string SelectedCategory { get; set; }
        public IEnumerable<IGrouping<CMSPageCell, Lenta>> Lenta { get; set; }
        public Lenta SelectedEvent { get; set; }
    }

    public class LastAndPopularViewModel
    {

        private IEnumerable<Lenta> _last;

        public IEnumerable<Lenta> Last
        {
            get
            {
                if (_last == null)
                {
                    var db = new DB();
                    var pageID = AccessHelper.CurrentPageInfo.CurrentPage.ID;
                    _last =
                        db.Lentas.Where(x => x.PageID == pageID && x.Visible)
                          .OrderByDescending(x => x.CreateDate)
                          .Take(9);
                }
                return _last;
            }
        }

        private IEnumerable<Lenta> _favorite;

        public IEnumerable<Lenta> Favorite
        {
            get
            {
                if (_favorite == null)
                {
                    var db = new DB();
                    var pageID = AccessHelper.CurrentPageInfo.CurrentPage.ID;
                    _favorite =
                        db.Lentas.Where(x => x.PageID == pageID && x.Visible)
                          .OrderByDescending(x => x.ViewAmount)
                          .Take(9);
                }
                return _favorite;
            }
        }

        private IEnumerable<Lenta> _commented;

        public IEnumerable<Lenta> Commented
        {
            get
            {
                if (_commented == null)
                {
                    var db = new DB();
                    var pageID = AccessHelper.CurrentPageInfo.CurrentPage.ID;
                    _commented =
                        db.Lentas.Where(x => x.PageID == pageID && x.Visible)
                          .OrderByDescending(x => x.LentaComments.Count)
                          .Take(9);
                }
                return _commented;
            }
        }
        private IEnumerable<Lenta> _pop;

        public IEnumerable<Lenta> Pop
        {
            get
            {
                if (_pop == null)
                {
                    var db = new DB();
                    var pageID = AccessHelper.CurrentPageInfo.CurrentPage.ID;
                    _pop =
                        db.Lentas.Where(x => x.PageID == pageID && x.Visible )
                          .OrderByDescending(x => x.UserFavoriteLentas.Count)
                          .Take(9);
                }
                return _pop;
            }
        }

    }


    public partial class Lenta
    {
        public string Href
        {
            get
            {
                return HttpContext.Current.CreateURL(this.PageID, new object[] { "newsid", ID });
            }
        }
    }


}