using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Smoking.Extensions;

namespace Smoking.Controllers
{
    public class TemplateEditorController : Controller
    {
        private List<ClientTemplate> _templates = new List<ClientTemplate>()
            {
                new ClientTemplate {ID = 0, Name = "Выберите шаблон", Path = ""},
                new ClientTemplate {ID = 1, Name = "Шаблон главной страницы", Path = "~/Views/Shared/MainPage.cshtml"},
                new ClientTemplate {ID = 2, Name = "Шаблон внутренних страниц", Path = "~/Views/Shared/InnerPage.cshtml"}
            };

        [HttpGet]
        [AuthorizeMaster]
        public ActionResult Template(int? id)
        {
            ViewBag.TemplateList = new SelectList(_templates, "ID", "Name", id ?? 0);
            ClientTemplate template = null;
            if ((id ?? 0) > 0)
            {
                template = _templates.FirstOrDefault(x => x.ID == id);
                var sr = new StreamReader(Server.MapPath(template.Path), Encoding.UTF8);
                template.FileContent = sr.ReadToEnd();
                sr.Close();

            }
            return View(template);
        }

        [HttpPost]
        [AuthorizeMaster]
        [ValidateInput(false)]
        public ActionResult Template(int? id, string FileContent)
        {
            ViewBag.TemplateList = new SelectList(_templates, "ID", "Name", id ?? 0);
            ClientTemplate template = null;
            if ((id ?? 0) > 0)
            {
                template = _templates.FirstOrDefault(x => x.ID == id);
                var sw = new StreamWriter(Server.MapPath(template.Path), false, Encoding.UTF8);
                sw.WriteLine(FileContent);
                sw.Close();
                ModelState.AddModelError("", "Данные успешно сохранены");
            }

            return View(template);

        }


        private static List<ClientTemplate> _blockList;
        public static List<ClientTemplate> BlockList
        {
            get
            {
                if (_blockList == null)
                {
                    _blockList = new List<ClientTemplate>();
                    int counter = 1;
                    var controllers = Assembly.GetCallingAssembly().GetTypes().Where(
                        type => type.IsSubclassOf(typeof (Controller))).ToList();

                    foreach (var controller in controllers)
                    {
                        var methods = controller.GetMethods();
                        ClientTemplateAttribute token = null;
                        foreach (var method in methods)
                        {
                            token = Attribute.GetCustomAttribute(method,
                                typeof(ClientTemplateAttribute), false) as ClientTemplateAttribute;
                            if (token == null)
                                continue;

                            _blockList.Add(new ClientTemplate()
                                {
                                    ID = counter,
                                    IsModul = token.IsModule,
                                    Name = token.Name,
                                    Action = method.Name,
                                    Controller = controller.Name.Replace("Controller", ""),
                                    Path =
                                        string.Format("~/Views/{0}/{1}.cshtml",
                                                      controller.Name.Replace("Controller", ""), method.Name)
                                });
                            counter++;
                        }
                    }

                    _blockList = _blockList.OrderBy(x => x.Name).ToList();
                    counter = 1;
                    foreach (var template in _blockList)
                    {
                        template.ID = counter;
                        counter++;
                    }

                    _blockList.Insert(0, new ClientTemplate() {ID = 0, Name = "Выберите блок", Path = ""});
                }
                return _blockList;
            }
        }

        [HttpGet]
        [AuthorizeMaster]
        public ActionResult Block(int? id)
        {
            ViewBag.TemplateList = new SelectList(BlockList, "ID", "Name", id ?? 0);
            ClientTemplate template = null;
            if ((id ?? 0) > 0)
            {
                template = BlockList.FirstOrDefault(x => x.ID == id);
                var sr = new StreamReader(Server.MapPath(template.Path), Encoding.UTF8);
                template.FileContent = sr.ReadToEnd();
                sr.Close();

            }
            return View(template);
        }


        [HttpPost]
        [AuthorizeMaster]
        [ValidateInput(false)]
        public ActionResult Block(int? id, string FileContent)
        {
            ViewBag.TemplateList = new SelectList(BlockList, "ID", "Name", id ?? 0);
            ClientTemplate template = null;
            if ((id ?? 0) > 0)
            {
                template = BlockList.FirstOrDefault(x => x.ID == id);
                var sw = new StreamWriter(Server.MapPath(template.Path), false, Encoding.UTF8);
                sw.WriteLine(FileContent);
                sw.Close();
                ModelState.AddModelError("", "Данные успешно сохранены");
            }

            return View(template);

        }

    }
}
