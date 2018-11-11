using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Smoking.Extensions;
using Smoking.Extensions.Helpers;
using Smoking.Models;

namespace Smoking.Controllers
{
    public class UniversalEditorController : Controller
    {


        [HttpPost]
        public virtual ActionResult UploadFile(string fileColumn)
        {
            HttpPostedFileBase myFile = Request.Files[fileColumn];
            bool isUploaded = false;
            string message = "Ошибка при загрузке изображения";
            var fileName = "";
            if (myFile != null && myFile.ContentLength != 0)
            {
                string pathForSaving = Server.MapPath("~/content/temp");
                if (this.CreateFolderIfNeeded(pathForSaving))
                {
                    try
                    {
                        fileName = DateTime.Now.ToString("ddMMyyyyHHmmss") + Path.GetExtension(myFile.FileName);
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
            return
                Json(
                    new
                        {
                            isUploaded = isUploaded,
                            message = message,
                            path = "/content/temp/" + fileName,
                            filedName = fileColumn
                        });
        }

        private bool CreateFolderIfNeeded(string path)
        {
            bool result = true;
            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (Exception)
                {
                    /*TODO: Need to process this exception.*/
                    result = false;
                }
            }
            return result;
        }

        [AuthorizeMaster]
        [HttpPost]
        [ValidateInput(false)]
        public PartialViewResult UniversalEdit(UniversalEditorPagedData ds, FormCollection collection)
        {
            ds.AddQueryParamsJoin = collection["AddQueryParamsJoin"];
            var model = GetModel(ds.CallerController, ds.CallerAction, null, ds.AddQueryParams);
            model.EditedRow.LoadPossibleProperties(new[] { "ID" }, collection);

            var imagePaths =
                collection.AllKeys.Where(x => x.EndsWith("_Path"))
                          .Select(x => new { Field = x.Replace("_Path", ""), Data = ReadFile(collection[x]) }).Where(x => x.Data != null && x.Data.Length > 0);


            foreach (var image in imagePaths)
            {
                model.EditedRow.SetPropertyValue(image.Field, new Binary(image.Data));
            }


            var errList = model.ErrorList;
            if (errList.IsFilled())
            {
                var list = errList.Split<string>("<br/>");
                foreach (var err in list)
                {
                    ModelState.AddModelError("", err);
                }
                //ModelState.AddModelError("", errList);
                return PartialView(model);
            }
            bool inserting = (int)model.EditedRow.GetPropertyValue("ID") == 0;
            string msg = "";
            if (model.SaveRow != null)
            {
                msg = model.SaveRow(model.EditedRow);
            }
            else
            {
                msg = model.Settings.UniversalTableSaver(model.EditedRow, model.Settings);

            }
            ModelState.AddModelError("", msg.IsNullOrEmpty() ? "Данные успешно сохранены" : msg);
            if (msg.IsNullOrEmpty())
                if (inserting)
                {
                    var routes = new RouteValueDictionary
                        {
                            {"Type", "List"},
                            {"Page", Request.QueryString["Page"].ToInt()}
                        };
                    var filterRoutes = model.FilterParams;
                    foreach (var route in filterRoutes.Where(route => Request.QueryString[route].IsFilled()))
                    {
                        routes.Add(route, Request.QueryString[route]);
                    }
                    model.RedirectURL = Url.Action(model.CallerAction, model.CallerController, routes);
                }
            return PartialView(model);
        }

        private byte[] ReadFile(string path)
        {
            ClearOldImages();

            if (string.IsNullOrEmpty(path))
                return null;

            try
            {
                using (var fs = new FileStream(Server.MapPath(path), FileMode.Open, FileAccess.Read))
                {
                    var buffer = new byte[fs.Length];
                    //TODO Check for large files
                    fs.Read(buffer, 0, (int)fs.Length);
                    fs.Close();
                    return buffer;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private void ClearOldImages()
        {
            int timeout = 30;
            var di = new DirectoryInfo(Server.MapPath("/content/temp"));
            var files = di.GetFiles();
            foreach (var file in files)
            {
                var name = Path.GetFileNameWithoutExtension(file.Name);
                DateTime nameDate;
                if (DateTime.TryParseExact(name, "ddMMyyyyHHmmss", CultureInfo.CurrentCulture, DateTimeStyles.None,
                                           out nameDate))
                {
                    if (nameDate.AddMinutes(timeout) < DateTime.Now)
                        try
                        {
                            file.Delete();
                        }
                        catch
                        {

                        }
                }
            }
        }

        private UniversalEditorPagedData GetModel(string callerController, string callerAction, object[] pa = null, string[] aa = null, string query = "")
        {
            NameValueCollection collection = new NameValueCollection();
            collection.Add(Request.QueryString);
            if (query.IsFilled())
            {
                var items =
                    query.Split<string>("&").Select(x => x.Split<string>("=").ToArray()).ToList();
                foreach (var item in items)
                {
                    if (collection[item.ElementAt(0)] == null)
                        collection.Add(item.ElementAt(0), item.ElementAt(1));
                }
            }
            var type = Type.GetType("Smoking.Controllers." + callerController + "Controller");
            var obj = Activator.CreateInstance(type);
            MethodInfo methodInfo = type.GetMethod(callerAction);
            object result = null;

            if (methodInfo != null)
            {
                var parametersArray = (pa ?? new object[]
                    {
                        collection["Type"], collection["Page"].ToInt(),
                        collection["UID"].ToInt()
                    }).ToList();

                if (aa != null)
                    parametersArray.AddRange(aa.Select(key => collection[key].ToSuitableType()));
                result = methodInfo.Invoke(obj, parametersArray.ToArray());
            }

            var model = (UniversalEditorPagedData)((ViewResult)result).ViewData.Model;
            return model;
        }

        [HttpPost]
        [AuthorizeMaster]
        public PartialViewResult UniversalDelete(UniversalEditorPagedData ds, FormCollection collection)
        {
            ds.AddQueryParamsJoin = collection["AddQueryParamsJoin"];
            var model = GetModel(ds.CallerController, ds.CallerAction, null, ds.AddQueryParams);
            var db = new DB();
            var table = db.GetTableByName(model.Settings.TableName);
            object target = null;
            var uid = (int)model.EditedRow.GetPropertyValue(model.Settings.UIDColumnName);
            foreach (var item in table)
            {
                if ((int)item.GetPropertyValue(model.Settings.UIDColumnName) == uid)
                    target = item;
            }
            if (target == null)
            {
                ModelState.AddModelError("", "Объект не найден.");
                return PartialView(model);
            }

            table.DeleteOnSubmit(target);
            db.SubmitChanges();
            model.RedirectURL = Url.Action(model.CallerAction, model.CallerController,
                                              new { Type = "List", Page = Request.QueryString["Page"].ToInt() });
            return PartialView(model);

        }


        public FileContentResult Image(string tableName, string uidName, string uidValue, string fieldName)
        {
            if (uidValue == "0")
            {
                Response.StatusCode = 404;
            }
            string sql = string.Format("select {0} from {1} where {2} = {3}", fieldName, tableName, uidName, uidValue);
            using (
                var conn =
                    new SqlConnection(ConfigurationManager.ConnectionStrings["MsSqlConnectionString"].ConnectionString))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    conn.Open();
                    var result = cmd.ExecuteScalar();
                    if (result == null || result is DBNull)
                    {
                        Response.StatusCode = 404;
                    }
                    Response.Cache.SetCacheability(HttpCacheability.ServerAndPrivate);
                    return new FileContentResult(result is DBNull || result == null ? new byte[0] : (byte[])result,
                                                 MIMETypeWrapper.GetMIMEForData(
                                                     new Binary(result is DBNull ? new byte[0] : (byte[])result)));


                }
            }

        }

        public ContentResult ClearImage(string tableName, string uidName, string uidValue, string fieldName)
        {

            string sql = string.Format("update {0} set {1} = null where {2} = {3}", tableName, fieldName, uidName,
                                       uidValue);
            using (
                var conn =
                    new SqlConnection(ConfigurationManager.ConnectionStrings["MsSqlConnectionString"].ConnectionString))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    conn.Open();
                    var result = cmd.ExecuteScalar();
                    if (result == null)
                    {
                        //Response.StatusCode = 404;
                    }
                    return new ContentResult() { Content = "1" };

                }
            }
        }


        [HttpPost]
        [AuthorizeMaster]
        public PartialViewResult changeOrder(int id, int? value, int page, string tablename, string uidname, string ordername, string cc, string ca, string addqs, string query)
        {
            if (!value.HasValue)
                return PartialView();
            var db = new DB();
            var table = db.GetTableByName(tablename);
            object target = null;
            var all = new List<KeyValuePair<object, int>>();

            foreach (var item in table)
            {
                if ((int)item.GetPropertyValue(uidname) == id)
                    target = item;
                else all.Add(new KeyValuePair<object, int>(item, (int)item.GetPropertyValue(ordername)));
            }
            if (target == null)
                return PartialView();

            if (all.Any())
            {
                all = all.OrderBy(x => x.Value).ToList();
                int pos = value.Value - 1;
                if (pos <= 0) pos = 0;
                int max = all.Max(x => x.Value);
                if (pos >= max)
                    all.Add(new KeyValuePair<object, int>(target, max + 1));
                else
                    all.Insert(pos, new KeyValuePair<object, int>(target, pos));
                int counter = 1;
                foreach (var rec in all)
                {
                    rec.Key.SetPropertyValue(ordername, counter);
                    counter++;
                }
            }
            else
            {
                target.SetPropertyValue(ordername, 1);
            }
            db.Refresh(RefreshMode.KeepChanges);
            db.SubmitChanges();


            var qp = (addqs ?? "").Split<string>("&").ToList();


            var model = GetModel(cc, ca, new object[] { "List", page, id }, qp.Any() ? qp.ToArray() : null, query);
            return PartialView("UniversalList", model);
        }

    }
}
