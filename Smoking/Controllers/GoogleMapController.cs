using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Smoking.Extensions;
using Smoking.Extensions.Helpers;
using Smoking.Models;

namespace Smoking.Controllers
{
    public class GoogleMapController : Controller
    {
        //
        // GET: /GoogleMap/
        private DB db = new DB();
        [ClientTemplate("Карта на главной")]
        public ActionResult MainPageMap()
        {
            return PartialView();
        }

        [ClientTemplate("Основня карта")]
        public ActionResult Map()
        {
            return PartialView(new MapModel());
        }

        [ClientTemplate("Кнопки редактирования карты")]
        public ActionResult MapEditor()
        {
            return PartialView();
        }

        [ClientTemplate("Фильтр объектов в левой колонке")]
        public ActionResult LeftFilter()
        {
            return PartialView(new MapModel());
        }

        [HttpPost]
        public ActionResult ZoneList(Guid? uid, int? type, int? oid, string Message, FormCollection collection)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                ModelState.AddModelError("", "Для добавления комментария необходимо авторизоваться на сайте");
                return ZoneList(uid, type, oid);
            }
            if (Message.IsNullOrEmpty())
            {
                ModelState.AddModelError("", "Необходимо заполнить текст комментария");
                return ZoneList(uid, type, oid);
            }
            if (!oid.HasValue)
            {
                ModelState.AddModelError("", "Необходимо выбрать объект");
                return ZoneList(uid, type, oid);
            }

            var db = new DB();
            var comment = new Comment()
                {
                    ParentCommentID = null,
                    Date = DateTime.Now,
                    UserID = AccessHelper.CurrentUserKey,
                    CommentText = Message
                };

            var rel = new MapObjectComment() { Comment = comment, ObjectID = oid.Value };
            db.MapObjectComments.InsertOnSubmit(rel);
            db.SubmitChanges();


            MailingList.Get("AddNotification")
           .WithReplacements(
               new[]
                                           {
                                               new MailReplacement("{OBJTYPE}", "комментарий"),
                                               new MailReplacement("{LINK}", AccessHelper.SiteUrl + comment.CommentedObjectLink)
                                           }
                   .
                   ToList()).Send();


            return ZoneList(uid, type, oid);
        }

        [ClientTemplate("Список точек и объектов пользователя")]
        public ActionResult ZoneList(Guid? uid, int? type, int? oid)
        {
            if (!uid.HasValue)
                uid = AccessHelper.CurrentUserKey;

            var db = new DB();
            ViewBag.IsObjectView = false;
            if (oid.HasValue)
            {
                ViewBag.IsObjectView = true;
                return PartialView(db.MapObjects.Where(x => x.ID == oid).Take(1));
            }

            var objects = db.MapObjects.AsQueryable();
            if (!(AccessHelper.IsMaster && AccessHelper.CurrentUserKey == uid ))
                objects = objects.Where(x => x.CreatorID == uid);

            if (type.HasValue)
            {
                if (type == 0)
                    objects = objects.Where(x => x.MapCoords.All(z => z.IsMarker));
                else if (type == 1)
                    objects = objects.Where(x => x.MapCoords.Any(z => !z.IsMarker));
            }

            return PartialView(objects.Take(1000));
        }

        [HttpPost]
        [Authorize]
        public virtual ContentResult AddObject(string qs)
        {
            var objData = new PointData().FromJson<PointData>(qs);
            string msg = "1";
            if (!AccessHelper.IsAuthClient)
            {
                msg = "Для добавления точки необходимо авторизоваться на сайте";
            }
            else
            {
                if (objData.UserData.ID == 0)
                {
                    try
                    {
                        var data = objData.ToDBModel();
                        db.MapObjects.InsertOnSubmit(data);
                        db.SubmitChanges();

                        MailingList.Get("AddNotification")
                                   .WithReplacements(
                                       new[]
                                           {
                                               new MailReplacement("{OBJTYPE}", objData.HasPolygon ? "зона" : "объект"),
                                               new MailReplacement("{LINK}",  AccessHelper.SiteUrl + data.CommentPageLink)
                                           }
                                           .
                                           ToList()).Send();


                        msg = "1";
                    }
                    catch (Exception e)
                    {
                        msg = e.Message;
                    }
                }
                else
                {



                    try
                    {

                        var dbmodel = db.MapObjects.First(x => x.ID == objData.UserData.ID);


                        if (dbmodel.CreatorID != AccessHelper.CurrentUserKey && !AccessHelper.IsMaster)
                        {
                            msg = "У вас нет прав на редактирование этой точки";
                        }
                        else
                        {
                            dbmodel.LoadPossibleProperties(objData.ToDBModel(), new string[] {"ID"});

                            if (objData.UserData.ZonePhoto.IsFilled())
                            {
                                db.MapObjectPhotos.DeleteAllOnSubmit(
                                    db.MapObjectPhotos.Where(x => x.ObjectID == objData.UserData.ID));
                            }

                            db.MapCoords.DeleteAllOnSubmit(db.MapCoords.Where(x => x.ObjectID == objData.UserData.ID));

                            db.SubmitChanges();

                            if (objData.HasPolygon)
                            {
                                var coords =
                                    objData.Polygon.Select(
                                        (x, index) =>
                                        new MapCoord()
                                            {
                                                IsMarker = false,
                                                XPos = x.Lat,
                                                YPos = x.Lng,
                                                OrderNum = index,
                                                MapObject = dbmodel
                                            }).ToList();
                            }

                            var mainCoord = new MapCoord()
                                {
                                    IsMarker = true,
                                    XPos = objData.Marker.Lat,
                                    YPos = objData.Marker.Lng,
                                    OrderNum = 0,
                                    MapObject = dbmodel
                                };

                            if (objData.UserData.ZonePhoto.IsFilled())
                            {
                                var fi = new FileInfo(HttpContext.Server.MapPath(objData.UserData.ZonePhoto));
                                var photo = new MapObjectPhoto() {MapObject = dbmodel, RawData = fi.ToBinary()};
                            }

                            db.SubmitChanges();
                        }

                    }
                    catch (Exception e)
                    {
                        msg = e.Message;
                    }

                }

            }
            return new ContentResult()
                {
                    Content = msg,
                    ContentEncoding = Encoding.UTF8
                };

        }


        [HttpPost]
        [Authorize]
        public virtual JsonResult GetObject(int id)
        {
            var obj = db.MapObjects.FirstOrDefault(x => x.ID == id);
            return new JsonResult() { Data = (obj ?? new MapObject()).ToUploadData() };
        }

        [HttpPost]
        public virtual ActionResult GetPoints(string qs)
        {
            var filter = new MapFilterData(Request.Params).FromJson<MapFilterData>(qs);

            return new JsonResult()
                {
                    Data = filter.GetArrayForUpload()
                };
        }
        [HttpPost]
        public virtual ActionResult UploadFile(string fileColumn)
        {
            HttpPostedFileBase myFile = Request.Files.Count > 0 ? Request.Files[0] : null;
            bool isUploaded = false;
            string message = "Ошибка при загрузке изображения";
            var fileName = "";
            if (AccessHelper.IsAuthClient)
            {

                var usr = Membership.GetUser();

                if (myFile != null && myFile.ContentLength != 0 && usr != null)
                {
                    string pathForSaving = Server.MapPath("~/content/temp/userimg");
                    if (pathForSaving.CreateFolderIfNeeded())
                    {
                        try
                        {
                            fileName = usr.ProviderUserKey + "_" + DateTime.Now.ToString("ddMMyyyyHHmmss") +
                                       Path.GetExtension(myFile.FileName);
                            myFile.SaveAs(Path.Combine(pathForSaving, fileName));
                            isUploaded = true;
                            message = "Изображение успешно загружено";
                        }
                        catch (Exception ex)
                        {
                            message = string.Format("Ошибка при загрузке изображения: {0}", ex.Message);
                        }
                    }
                }
            }
            return
                    Json(
                        new
                            {
                                isUploaded = isUploaded,
                                message = message,
                                path = "/content/temp/userimg/" + fileName,
                                filedName = fileColumn
                            });


        }


        public ActionResult Delete(int oid, Guid uid)
        {
            if (uid != AccessHelper.CurrentUserKey && !AccessHelper.IsMaster)
            {
                return Redirect(Request.RawUrl);
            }

            var db = new DB();
            var obj = db.MapObjects.FirstOrDefault(x => (x.CreatorID == uid || AccessHelper.IsMaster) && x.ID == oid);
            if (obj != null)
            {
                db.MapObjects.DeleteOnSubmit(obj);
                db.SubmitChanges();
            }

            return Redirect(CMSPage.GetByType("ProfileZones").First().FullUrl + "?uid=" + uid);
        }
    }
}
