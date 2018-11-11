using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Smoking.Extensions;
using Smoking.Extensions.Helpers;
using Smoking.Models;

namespace Smoking.Controllers
{
    public class PagesController : Controller
    {
        DB db = new DB();

        [HttpGet]
        [AuthorizeMaster]
        public ActionResult Index(int? Page)
        {
            var pagedList = new PagedData<CMSPage>(db.CMSPages.OrderBy(x => x.OrderNum), Page ?? 0, 20, "MasterListPaged");
            return View(pagedList);
        }

        [HttpGet]
        [AuthorizeMaster]
        public ActionResult Edit(int? ID, int? ParentID)
        {
            CMSPage page = new CMSPage() { ParentID = 0, Visible = true, ViewMenu = true};

            if (!ID.HasValue || ID == 0)
            {
                ViewBag.Header = "Создание нового раздела";

                var parent = CMSPage.FullPageTable.FirstOrDefault(x => x.ID == ParentID);
                if (parent != null)
                {
                    parent.RolesList = null;
                    page.RolesList = parent.RolesList;
                }
            }
            else
            {
                ViewBag.Header = "Редактирование раздела";
                page = db.CMSPages.FirstOrDefault(x => x.ID == ID);
                
                if (page == null)
                {
                    RedirectToAction("Index");
                }
                page.LoadLangValues();
            }
            
            var parents = CMSPage.FullPageTable.Where(x=> x.ID!=ID).ToList();
            parents.Insert(0, new CMSPage() { ID = 0, PageName = "Корневой раздел сайта" });
            ViewBag.Parents = new SelectList(parents, "ID", "PageName", page.ParentID ?? 0);
            ViewBag.Types = new SelectList(db.PageTypes.OrderBy(x=> x.Ordernum).AsEnumerable(), "ID", "Description");
            return View(page);
        }

        [HttpPost]
        [AuthorizeMaster]
        public ActionResult Edit(int? ID, FormCollection collection)
        {
            var page = new CMSPage();
            bool exist = false;
            if (ID.HasValue && ID > 0)
            {
                page = db.CMSPages.FirstOrDefault(x => x.ID == ID);
                exist = true;
                if (page == null)
                    return RedirectToAction("Index");
            }
            else
            {
                page.OrderNum = (db.CMSPages.Count() + 1) * 10;
                db.CMSPages.InsertOnSubmit(page);
            }
            UpdateModel(page);
            if (page.ParentID == 0) page.ParentID = null;

            var parents = CMSPage.FullPageTable.Where(x => x.ID != ID).ToList();
            parents.Insert(0, new CMSPage() { ID = 0, PageName = "Корневой раздел сайта" });
            ViewBag.Parents = new SelectList(parents, "ID", "PageName", page.ParentID ?? 0);
            ViewBag.Types = new SelectList(db.PageTypes.OrderBy(x=> x.Ordernum).AsEnumerable(), "ID", "Description");

            try
            {
                db.SubmitChanges();
                page.SaveLangValues();
                ModelState.AddModelError("", "Данные успешно сохранены");

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            var roles =
                collection.AllKeys.Where(x => x.StartsWith("ID_"))
                          .Select(
                              x =>
                              new
                                  {
                                      Value = (bool)collection.GetValue(x).ConvertTo(typeof(bool)),
                                      ID = new Guid(x.Split<string>("_").ToArray()[1])
                                  });

            foreach (var role in roles)
            {
                var rel =
                    db.CMSPageRoleRels.FirstOrDefault(
                        x => (role.ID == new Guid() ? !x.RoleID.HasValue : x.RoleID == role.ID) && x.PageID == page.ID);
                if (role.Value && rel == null)
                {
                    db.CMSPageRoleRels.InsertOnSubmit(new CMSPageRoleRel()
                        {
                            RoleID = role.ID == new Guid() ? (Guid?) null : role.ID,
                            PageID = page.ID
                        });
                }
                if (!role.Value && rel != null)
                {
                    db.CMSPageRoleRels.DeleteOnSubmit(rel);
                }
            }
            db.SubmitChanges();

            CMSPage.FullPageTable = null;
            if (exist)
                return View(page);
            else return RedirectToAction("Index");

//            return RedirectToAction("Index");

        }

        [HttpGet]
        [AuthorizeMaster]
        public JsonResult getTreeData()
        {
            var result = new JsonResult();
            JsTreeModel root = new JsTreeModel() { data = AccessHelper.SiteName.ToNiceForm(), attr = new JsTreeAttribute() { id = "x0", href = "#", uid = 0 }, children = new List<JsTreeModel>() };
            fillModel(ref root, null);
            result.Data = root;
            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            result.ContentType = "application/json";
            return result;

        }


        private void fillModel(ref JsTreeModel model, int? id)
        {
            var pages = CMSPage.FullPageTable.Where(x => id == null ? !x.ParentID.HasValue : x.ParentID == id).OrderBy(
                x => x.OrderNum);

            //var pages = db.CMSPages.Where(x => id == null ? !x.ParentID.HasValue : x.ParentID == id).OrderBy(x => x.OrderNum);
            foreach (var cmsPage in pages)
            {
                var child = new JsTreeModel()
                                {
                                    data = cmsPage.PageName,
                                    attr =
                                        new JsTreeAttribute()
                                            {
                                                id = "x" + cmsPage.ID,
                                                href = Url.Action("Edit", "Pages", new { ID = cmsPage.ID }),
                                                uid = cmsPage.ID
                                            },
                                };
                if (model.children == null)
                    model.children = new List<JsTreeModel>();
                model.children.Add(child);
                fillModel(ref child, cmsPage.ID);
            }
        }

        [HttpGet]
        [AuthorizeMaster]
        public ActionResult Delete(int? ID)
        {
            var page = db.CMSPages.FirstOrDefault(x => x.ID == ID);
            if (page == null)
                return RedirectToAction("Index");
            return View(page);
        }

        [HttpPost]
        [AuthorizeMaster]
        public ActionResult Delete(int? ID, FormCollection collection)
        {
            var page = db.CMSPages.FirstOrDefault(x => x.ID == ID);
            if (page == null)
                return RedirectToAction("Index");

            deleteRecursive(page);
            db.SubmitChanges();
            CMSPage.ClearAllCache();
            return RedirectToAction("Index");
        }

        private void deleteRecursive(CMSPage page)
        {
            if (page.Children.Any())
            {
                foreach (var child in page.Children)
                {
                    deleteRecursive(child);
                }
            }
            db.CMSPages.DeleteOnSubmit(page);
        }

        [HttpPost]
        [AuthorizeMaster]
        public ContentResult saveNode(int nodeID, int targetID, string type)
        {
            var currentNode = db.CMSPages.FirstOrDefault(x => x.ID == nodeID);
            var targetNode = db.CMSPages.FirstOrDefault(x => x.ID == targetID);
            if (currentNode == null || (targetNode == null && targetID != 0)) return new ContentResult();
            var targetParent = targetNode == null ? null : (int?)targetNode.ID;
            switch (type)
            {
                //родитель меняется
                case "first":
                    currentNode.ParentID = targetParent;
                    var inLevelNodes = db.CMSPages.Where(x => targetParent == null ? !x.ParentID.HasValue : x.ParentID == targetParent);
                    if (inLevelNodes.Any())
                        currentNode.OrderNum = inLevelNodes.Min(x => x.OrderNum) - 20;
                    break;
                case "last":
                    currentNode.ParentID = targetParent;
                    var inLevelNodesA = db.CMSPages.Where(x => targetParent == null ? !x.ParentID.HasValue : x.ParentID == targetParent);
                    if (inLevelNodesA.Any())
                        currentNode.OrderNum = inLevelNodesA.Max(x => x.OrderNum) + 20;
                    break;
                //родитель не меняется ??
                case "before":
                    targetParent = targetNode == null ? null : (int?)targetNode.ParentID;
                    var prevInOrder =
                        db.CMSPages.Where(x => (targetParent == null ? !x.ParentID.HasValue : x.ParentID == targetParent) && x.OrderNum < targetNode.OrderNum);
                    foreach (var page in prevInOrder)
                    {
                        page.OrderNum -= 40;
                    }
                    currentNode.OrderNum = targetNode.OrderNum - 20;
                    currentNode.ParentID = targetParent;
                    break;
                case "after":
                    targetParent = targetNode == null ? null : (int?)targetNode.ParentID;
                    var nextInOrder =
                        db.CMSPages.Where(x => (targetParent == null ? !x.ParentID.HasValue : x.ParentID == targetParent) && x.OrderNum > targetNode.OrderNum);
                    foreach (var page in nextInOrder)
                    {
                        page.OrderNum += 40;
                    }
                    currentNode.OrderNum = targetNode.OrderNum + 20;
                    currentNode.ParentID = targetParent;
                    break;
            }

            db.SubmitChanges();
            //Обнуляем кеш
            CMSPage.FullPageTable = null;
            return new ContentResult() { Content = "1" };
        }

       
    }
}
