using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Smoking.Models;
using Smoking.Extensions;
using Smoking.Extensions.Helpers;
namespace Smoking.Controllers
{
    public class RolesController : Controller
    {
        private DB db = new DB();

        [HttpGet]
        [AuthorizeMaster]
        public ActionResult AccessEditor(Guid? RoleID)
        {
            var roleID = (RoleID == new Guid() ? null : RoleID);
            ViewBag.PageListPlain =
                db.CMSPageRoleRels.Where(x => (roleID == null ? !x.RoleID.HasValue : x.RoleID == roleID))
                  .Select(x => x.PageID)
                  .AsEnumerable()
                  .Select(x => (object) x)
                  .JoinToString(";");
            var roles =
                db.Roles.AsEnumerable().OrderBy(x => x.CanDelete)
                  .ThenBy(x => x.RoleName)
                  .Select(
                      x => new {UID = x.RoleId, Name = x.Description.IsNullOrEmpty() ? x.RoleName : x.Description})
                  .ToList();
            roles.Insert(0, new {UID = new Guid(), Name = "Неавторизованные пользователи"});
            ViewBag.RoleSelectList = new SelectList(roles, "UID", "Name", RoleID);
            return View();
        }

        [HttpPost]
        [AuthorizeMaster]
        public ActionResult AccessEditor(Guid? RoleID, FormCollection collection)
        {
            var pages =
                collection.GetValue("PageListPlain")
                          .AttemptedValue.Split<int>(";")
                          .Where(x => x > 0)
                          .Select(x => CMSPage.FullPageTable.FirstOrDefault(z => z.ID == x))
                          .Where(x => x != null).ToList();


            var pagesIdsForAdd = pages.SelectMany(x => x.FullChildrenList).Union(pages.Select(x=> x.ID)).ToList();
            var pagesIdsForDel = CMSPage.FullPageTable.Select(x => x.ID).Except(pagesIdsForAdd);

            var roleID = (RoleID == new Guid() ? null : RoleID);

            var relsForDel =
                db.CMSPageRoleRels.Where(
                    x => pagesIdsForDel.Contains(x.PageID) && (roleID == null ? !x.RoleID.HasValue : x.RoleID == roleID));
            db.CMSPageRoleRels.DeleteAllOnSubmit(relsForDel);
            db.SubmitChanges();

            foreach (var id in pagesIdsForAdd)
            {
                var rel =
                    db.CMSPageRoleRels.FirstOrDefault(
                        x => x.PageID == id && (roleID == null ? !x.RoleID.HasValue : x.RoleID == roleID));
                if (rel == null)
                {
                    rel = new CMSPageRoleRel() {PageID = id, RoleID = roleID};
                    db.CMSPageRoleRels.InsertOnSubmit(rel);
                }
            }
            db.SubmitChanges();
            return RedirectToAction("AccessEditor", new {RoleID = roleID});
        }


        [HttpGet]
        [AuthorizeMaster]
        public ActionResult Delete(Guid? RoleID)
        {
            if (RoleID.IsEmpty())
            {
                return RedirectToAction("Index");
            }
            var role = from x in db.Roles
                       where x.RoleId == RoleID
                       select x;
            if (!role.Any())
            {
                return RedirectToAction("Index");
            }
            return View(role.First());
        }

        [AuthorizeMaster, HttpPost]
        public ActionResult Delete(Guid? RoleID, bool includeUsers)
        {
            if (!RoleID.IsEmpty())
            {
                var role = from x in db.Roles
                           where x.RoleId == RoleID
                           select x;
                if (!role.Any())
                {
                    return RedirectToAction("Index");
                }
                if (includeUsers)
                {
                    string[] users = Roles.GetUsersInRole(role.First().RoleName);
                    foreach (string user in users)
                    {
                        Membership.DeleteUser(user);
                    }
                }
                Roles.DeleteRole(role.First().RoleName);
            }
            return RedirectToAction("Index");
        }

        [HttpGet, AuthorizeMaster]
        public ActionResult Edit(Guid? RoleID)
        {
            if (RoleID.HasValue)
            {
                var role = from x in db.Roles
                           where x.RoleId == RoleID
                           select x;
                if (role.Any())
                {
                    return View(role.First());
                }
                RedirectToAction("Index");
            }
            return View(new Role());
        }

        [HttpPost, AuthorizeMaster]
        public ActionResult Edit(Guid? RoleID, Role model)
        {
            if (!model.IsCreating)
            {
                var role = from x in db.Roles
                           where x.RoleId == RoleID
                           select x;
                if (role.Any())
                {
                    if (role.First().CanEditUID)
                    {
                        UpdateModel(role.First());
                        role.First().LoweredRoleName = model.RoleName.ToLower();
                    }
                    else
                    {
                        role.First().Description = model.Description;
                    }
                    db.SubmitChanges();
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError("", "Тип исполнителя не найден");
            }
            else if (Roles.RoleExists(model.RoleName))
            {
                ModelState.AddModelError("", "Такой тип исполнителя уже существует");
            }
            else
            {
                Roles.CreateRole(model.RoleName);
                Role role = (from x in db.Roles
                             where x.RoleName == model.RoleName
                             select x).First();
                UpdateModel(role);
                role.LoweredRoleName = model.RoleName.ToLower();
                db.SubmitChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [AuthorizeMaster, HttpGet]
        public ActionResult Index()
        {
            return View(from x in
                            (from x in db.Roles
                             where x.RoleName != "GrandAdmin"
                             select x).AsEnumerable()
                        orderby x.CanDelete, x.RoleName
                        select x);
        }
    }
}

