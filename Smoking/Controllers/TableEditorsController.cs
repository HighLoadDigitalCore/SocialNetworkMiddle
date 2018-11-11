using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Smoking.Extensions;
using Smoking.Extensions.Helpers;
using Smoking.Models;


namespace Smoking.Controllers
{
    public class TableEditorsController : Controller
    {
        private DB db = new DB();

        #region Редактор комментариев


        [AuthorizeMaster]
        public ActionResult CommentEditor(string Type, int? Page, int? UID)
        {

            var editedRow = db.Comments.FirstOrDefault(x => x.ID == UID) ??
                            new Comment()
                                {
                                    UserID = AccessHelper.CurrentUserKey,
                                    ID = 0,
                                };


            var settings = new UniversalEditorSettings()
                {
                    TableName = "Comments",
                    HasDeleteColumn = true,
                    CanAddNew = false,
                    UIDColumnName = "ID",
                    ShowedFieldsInList =
                        new List<UniversalListField>()
                            {
                                new UniversalListField()
                                    {
                                        FieldName = "Date",
                                        IsLinkToEdit = true,
                                        HeaderText = "Дата добавления",
                                        Template = "<span style='white-space: nowrap;'>{{0}}</span>"
                                    },
                                new UniversalListField()
                                    {
                                        FieldName = "CommentText",
                                        IsLinkToEdit = false,
                                        HeaderText = "Комментарий",
                                        Width = 300
                                    },
                                new UniversalListField()
                                    {
                                        FieldName = "UserID",
                                        HeaderText = "Комментарий добавлен",
                                        IsLinkToEdit = false,
                                        TextFunction = x => UserProfile.Get((Guid) x).FullName,
                                    },
                                new UniversalListField()
                                    {
                                        FieldName = "ID",
                                        HeaderText = "Ссылка на страницу",
                                        IsLinkToEdit = false,
                                        TextFunction = x =>
                                            {
                                                var row = db.Comments.First(z => z.ID == (int) x);
                                                return string.Format("<a href='{0}'>{1}</a>",
                                                                     row.CommentedObjectLink,
                                                                     row.CommentedObject);
                                            }
                                    }
                            },

                    EditedFieldsList =
                        new List<UniversalEditorField>()
                            {
                                new UniversalEditorField()
                                    {
                                        FieldName = "Date",
                                        FieldType = UniversalEditorFieldType.Calendar,
                                        HeaderText = "Дата",
                                        DataType = typeof (DateTime),
                                        Modificators =
                                            new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                    },

                                new UniversalEditorField()
                                    {
                                        FieldName = "CommentText",
                                        FieldType = UniversalEditorFieldType.TextArea,
                                        HeaderText = "Комментарий",
                                        DataType = typeof (string),
                                        Modificators =
                                            new List<IUniversalFieldModificator>() {new RequiredModificator()}

                                    },



                            },
                    AutoFilter = false,

                };


            var type = (CurrentEditorType)Enum.Parse(typeof(CurrentEditorType), Type ?? "List", true);
            var data = new UniversalEditorPagedData()
            {
                PagedData =
                    type == CurrentEditorType.List
                        ? new PagedData<Comment>(db.Comments.OrderByDescending(x => x.Date), Page ?? 0, 50, "Master")
                        : null,
                Settings = settings,
                CurrentType = type,
                EditedRow =
                    type == CurrentEditorType.List
                        ? null
                        : editedRow
            };
            data.IsAddingNew = data.EditedRow != null && ((Comment)data.EditedRow).ID == 0;
            data.CallerController = "TableEditors";
            data.CallerAction = "CommentEditor";
            data.EditorName = "Редактор комментариев";
            return View("TableEditor", data);
        }

        #endregion


        #region Редактор Объектов


        [AuthorizeMaster]
        public ActionResult ObjectEditor(string Type, int? Page, int? UID)
        {

            var editedRow = db.MapObjects.FirstOrDefault(x => x.ID == UID) ??
                            new MapObject()
                                {
                                    ID = 0,
                                };


            var settings = new UniversalEditorSettings()
                {
                    TableName = "MapObjects",
                    HasDeleteColumn = true,
                    CanAddNew = false,
                    UIDColumnName = "ID",
                    ShowedFieldsInList =
                        new List<UniversalListField>()
                            {
                                new UniversalListField()
                                    {
                                        FieldName = "Name",
                                        IsLinkToEdit = true,
                                        HeaderText = "Название",
                                        Width = 150
                                    },
                                new UniversalListField()
                                    {
                                        FieldName = "CreateDate",
                                        HeaderText = "Дата добавления"
                                    },
                                new UniversalListField()
                                    {
                                        FieldName = "CreatorID",
                                        HeaderText = "Автор",
                                        IsLinkToEdit = false,
                                        TextFunction = x => UserProfile.Get((Guid) x).FullName,
                                    },
                                new UniversalListField()
                                    {
                                        FieldName = "ID",
                                        HeaderText = "Тип объекта",
                                        TextFunction = x =>
                                            {
                                                var row = db.MapObjects.First(z => z.ID == (int) x);
                                                return row.MapObjectType.TypeName;
                                            }
                                    }
                            },

                    EditedFieldsList =
                        new List<UniversalEditorField>()
                            {
                                new UniversalEditorField()
                                    {
                                        FieldName = "Name",
                                        FieldType = UniversalEditorFieldType.TextBox,
                                        HeaderText = "Название",
                                        DataType = typeof (string),
                                        Modificators =
                                            new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                    },     new UniversalEditorField()
                                    {
                                        FieldName = "CreateDate",
                                        FieldType = UniversalEditorFieldType.Calendar,
                                        HeaderText = "Дата добавления",
                                        DataType = typeof (DateTime),
                                        Modificators =
                                            new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                    },

                                new UniversalEditorField()
                                    {
                                        FieldName = "Address",
                                        FieldType = UniversalEditorFieldType.TextArea,
                                        HeaderText = "Адрес",
                                        DataType = typeof (string),
                                        Modificators =
                                            new List<IUniversalFieldModificator>() {new RequiredModificator()}

                                    },

                                new UniversalEditorField()
                                    {
                                        FieldName = "Description",
                                        FieldType = UniversalEditorFieldType.TextArea,
                                        HeaderText = "Описание",
                                        DataType = typeof (string),
                                        Modificators =
                                            new List<IUniversalFieldModificator>() {new RequiredModificator()}

                                    },



                            },
                    AutoFilter = false,

                };


            var type = (CurrentEditorType)Enum.Parse(typeof(CurrentEditorType), Type ?? "List", true);
            var data = new UniversalEditorPagedData()
            {
                PagedData =
                    type == CurrentEditorType.List
                        ? new PagedData<MapObject>(db.MapObjects.OrderByDescending(x => x.CreateDate), Page ?? 0, 50, "Master")
                        : null,
                Settings = settings,
                CurrentType = type,
                EditedRow =
                    type == CurrentEditorType.List
                        ? null
                        : editedRow
            };
            data.IsAddingNew = data.EditedRow != null && ((MapObject)data.EditedRow).ID == 0;
            data.CallerController = "TableEditors";
            data.CallerAction = "ObjectEditor";
            data.EditorName = "Редактор объектов";
            return View("TableEditor", data);
        }

        #endregion

        #region Редактор типов объектов на карте


        [AuthorizeMaster]
        public ActionResult MapObjectTypesEditor(string Type, int? Page, int? UID)
        {

            var editedRow = db.MapObjectTypes.FirstOrDefault(x => x.ID == UID) ??
                            new MapObjectType()
                                {
                                    Icon = "",
                                    TypeName = "",
                                    OrderNum = db.MapObjectTypes.Count() + 1
                                };

            var settings = new UniversalEditorSettings()
                {
                    TableName = "MapObjectTypes",
                    HasDeleteColumn = true,
                    CanAddNew = true,
                    UIDColumnName = "ID",
                    ShowedFieldsInList =
                        new List<UniversalListField>()
                            {
                                new UniversalListField()
                                    {
                                        FieldName = "TypeName",
                                        IsLinkToEdit = true,
                                        HeaderText = "Тип объекта"
                                    },
                                new UniversalListField()
                                    {
                                        FieldName = "Icon",
                                        IsLinkToEdit = false,
                                        HeaderText = "CSS класс",
                                    },
                                new UniversalListField()
                                    {
                                        FieldName = "OrderNum",
                                        HeaderText = "Порядковый номер",
                                        IsOrderColumn = true
                                    }
                            },

                    EditedFieldsList =
                        new List<UniversalEditorField>()
                            {
                                new UniversalEditorField()
                                    {
                                        FieldName = "TypeName",
                                        FieldType = UniversalEditorFieldType.TextBox,
                                        HeaderText = "Тип объекта",
                                        DataType = typeof (string),
                                        Modificators =
                                            new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                    },

                                new UniversalEditorField()
                                    {
                                        FieldName = "Icon",
                                        FieldType = UniversalEditorFieldType.TextBox,
                                        HeaderText = "CSS класс",
                                        DataType = typeof (string),
                                        Modificators =
                                            new List<IUniversalFieldModificator>() {new RequiredModificator()}

                                    },



                            },
                    AutoFilter = false,

                };


            var type = (CurrentEditorType)Enum.Parse(typeof(CurrentEditorType), Type ?? "List", true);
            var data = new UniversalEditorPagedData()
            {
                PagedData =
                    type == CurrentEditorType.List
                        ? new PagedData<MapObjectType>(db.MapObjectTypes.OrderBy(x => x.OrderNum), Page ?? 0, 10, "Master")
                        : null,
                Settings = settings,
                CurrentType = type,
                EditedRow =
                    type == CurrentEditorType.List
                        ? null
                        : editedRow
            };
            data.IsAddingNew = data.EditedRow != null && ((MapObjectType)data.EditedRow).ID == 0;
            data.CallerController = "TableEditors";
            data.CallerAction = "MapObjectTypesEditor";
            data.EditorName = "Редактор типов объектов на карте";
            return View("TableEditor", data);
        }

        #endregion

        #region Редактор городов


        [AuthorizeMaster]
        public ActionResult MapSelect(string Type, int? Page, int? UID)
        {

            var editedRow = db.MapSelects.FirstOrDefault(x => x.ID == UID) ??
                            new MapSelect()
                                {
                                    Visible = true,
                                    OrderNum = db.MapSelects.Count() + 1
                                };

            var settings = new UniversalEditorSettings()
                {
                    TableName = "MapSelects",
                    HasDeleteColumn = true,
                    CanAddNew = true,
                    UIDColumnName = "ID",
                    ShowedFieldsInList =
                        new List<UniversalListField>()
                            {
                                new UniversalListField()
                                    {
                                        FieldName = "Name",
                                        IsLinkToEdit = true,
                                        HeaderText = "Название города"
                                    },
                                new UniversalListField()
                                    {
                                        FieldName = "Visible",
                                        IsLinkToEdit = false,
                                        HeaderText = "Отображать на сайте",
                                    },
                                new UniversalListField()
                                    {
                                        FieldName = "OrderNum",
                                        HeaderText = "Порядковый номер",
                                        IsOrderColumn = true
                                    }
                            },

                    EditedFieldsList =
                        new List<UniversalEditorField>()
                            {
                                new UniversalEditorField()
                                    {
                                        FieldName = "Name",
                                        FieldType = UniversalEditorFieldType.TextBox,
                                        HeaderText = "Название города",
                                        DataType = typeof (string),
                                        Modificators =
                                            new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                    },
                                new UniversalEditorField()
                                    {
                                        FieldName = "Lat",
                                        FieldType = UniversalEditorFieldType.TextBox,
                                        HeaderText = "Широта",
                                        DataType = typeof (decimal),
                                        Modificators =
                                            new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                    },
                                new UniversalEditorField()
                                    {
                                        FieldName = "Lng",
                                        FieldType = UniversalEditorFieldType.TextBox,
                                        HeaderText = "Долгота",
                                        DataType = typeof (decimal),
                                        Modificators =
                                            new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                    },

                                new UniversalEditorField()
                                    {
                                        FieldName = "Zoom",
                                        FieldType = UniversalEditorFieldType.TextBox,
                                        HeaderText = "Приближение",
                                        DataType = typeof (int),
                                        Modificators =
                                            new List<IUniversalFieldModificator>() {new RequiredModificator()}

                                    },
                                new UniversalEditorField()
                                    {
                                        FieldName = "Visible",
                                        FieldType = UniversalEditorFieldType.CheckBox,
                                        HeaderText = "Отображать на сайте",
                                        DataType = typeof (bool)
                                    },



                            },
                    AutoFilter = false,

                };


            var type = (CurrentEditorType)Enum.Parse(typeof(CurrentEditorType), Type ?? "List", true);
            var data = new UniversalEditorPagedData()
            {
                PagedData =
                    type == CurrentEditorType.List
                        ? new PagedData<MapSelect>(db.MapSelects.OrderBy(x => x.OrderNum), Page ?? 0, 10, "Master")
                        : null,
                Settings = settings,
                CurrentType = type,
                EditedRow =
                    type == CurrentEditorType.List
                        ? null
                        : editedRow
            };
            data.IsAddingNew = data.EditedRow != null && ((MapSelect)data.EditedRow).ID == 0;
            data.CallerController = "TableEditors";
            data.CallerAction = "MapSelect";
            data.EditorName = "Редактор списка городов";
            return View("TableEditor", data);
        }

        #endregion

        #region Текстовый редактор
        [AuthorizeMaster]
        public ActionResult TextEditor(string Type, int? Page, int? UID, int? LangID, int? CMSPageID, int? ViewID)
        {
            if (!LangID.HasValue)
            {
                LangID = (db.Languages.FirstOrDefault(x => x.ShortName == "ru") ?? db.Languages.First()).ID;
            }
            if (!CMSPageID.HasValue)
            {
                CMSPageID =
                    (db.CMSPages.FirstOrDefault(
                        x =>
                        x.PageType.CMSPageCells.Any(
                            c => c.CMSPageCellViews.Any(v => v.Action == "Index" && v.Controller == "TextPage"))) ??
                     new CMSPage() { ID = 0 }).ID;

            }

            var views =
                db.CMSPageCellViews.Where(x => x.Action == "Index" && x.Controller == "TextPage")
                  .Select(x => x.CMSPageCell)
                  .Intersect(db.CMSPages.Where(x => x.ID == CMSPageID).SelectMany(x => x.PageType.CMSPageCells))
                  .ToList()
                  .SelectMany(x => x.CMSPageCellViews.Where(c => c.Action == "Index" && c.Controller == "TextPage"))
                  .GroupBy(x => x.CMSPageCell);


            foreach (var view in views)
            {
                if (view.Count() > 1)
                {
                    for (int i = 1; i <= view.Count(); i++)
                    {
                        view.ElementAt(i - 1).Description += " - блок №" + i;
                    }
                }

            }

            var blocks = views.SelectMany(x => x).ToList();

            if (!ViewID.HasValue)
            {
                ViewID = (blocks.FirstOrDefault() ?? new CMSPageCellView()).ID;/*
                    (db.CMSPageCellViews.Where(x => x.Action == "Index" && x.Controller == "TextPage")
                       .Select(x => x.CMSPageCell)
                       .Intersect(db.CMSPages.Where(x => x.ID == CMSPageID).SelectMany(x => x.PageType.CMSPageCells))
                       .SelectMany(x => x.CMSPageCellViews.Where(x=> x.))
                       .FirstOrDefault() ?? new CMSPageCellView()).ID;*/
            }


            var editedRow = (db.CMSPageTextDatas.FirstOrDefault(x => x.ID == UID) ??
                             new CMSPageTextData()
                             {
                                 LangID = LangID.Value,
                                 CMSPageID = CMSPageID.Value,
                                 ViewID = ViewID.Value,
                                 Text = "",
                                 Visible = true
                             });





            var settings = new UniversalEditorSettings()
            {
                TableName = "CMSPageTextDatas",
                HasDeleteColumn = true,
                CanAddNew = true,
                UIDColumnName = "ID",
                ShowedFieldsInList =
                    new List<UniversalListField>()
                            {
                                new UniversalListField()
                                    {
                                        FieldName = "Text",
                                        IsLinkToEdit = true,
                                        HeaderText = "Текст",
                                        TextFunction = x =>
                                            {
                                                var cleared = x.ToString().ClearHTML();
                                                if (cleared.Length > 100)
                                                    cleared = cleared.Substring(0, 100) + "...";
                                                return cleared;
                                            }
                                    },
                                new UniversalListField()
                                    {
                                        FieldName = "Visible",
                                        HeaderText = "Статус",
                                        TextFunction = x => (bool) x ? "Отображается" : "Неактивен"
                                    },
                                new UniversalListField()
                                    {
                                        FieldName = "OrderNum",
                                        IsLinkToEdit = false,
                                        IsOrderColumn = true,
                                        HeaderText = "Порядок"
                                    }
                            },

                EditedFieldsList =
                    new List<UniversalEditorField>()
                            {
                                new UniversalEditorField()
                                    {
                                        FieldName = "LangID",
                                        Hidden = true,
                                        FieldType = UniversalEditorFieldType.DropDown,
                                        InnerListDataSource = new UniversalDataSource()
                                            {
                                                DefValue = AccessHelper.CurrentLang.ID,
                                                HasEmptyDef = false,
                                                KeyField = "ID",
                                                ValueField = "Name",
                                                Source = db.Languages.Where(x => x.Enabled),

                                            },
                                        HeaderText = "Язык",
                                        Modificators =
                                            new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                    },
                                new UniversalEditorField()
                                    {
                                        FieldName = "CMSPageID",
                                        FieldType = UniversalEditorFieldType.DropDown,
                                        InnerListDataSource = new UniversalDataSource()
                                            {
                                                DefValue = CMSPageID,
                                                HasEmptyDef = false,
                                                KeyField = "ID",
                                                ValueField = "PageName",
                                                Source =
                                                    db.CMSPages.Where(
                                                        x =>
                                                        x.PageType.CMSPageCells.Any(
                                                            c =>
                                                            c.CMSPageCellViews.Any(
                                                                v => v.Action == "Index" && v.Controller == "TextPage")))
                                                      .ToList()
                                                      .Select(x => x.LoadLangValues()),

                                            },
                                        HeaderText = "Страница",
                                        Modificators =
                                            new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                    },
                                new UniversalEditorField()
                                    {
                                        FieldName = "ViewID",
                                        FieldType = UniversalEditorFieldType.DropDown,
                                        InnerListDataSource = new UniversalDataSource()
                                            {
                                                DefValue = ViewID,
                                                HasEmptyDef = false,
                                                KeyField = "ID",
                                                ValueField = "Description",
                                                Source = blocks,

                                            },
                                        HeaderText = "Контейнер",
                                        Modificators =
                                            new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                    },

                                new UniversalEditorField()
                                    {
                                        FieldName = "Visible",
                                        FieldType = UniversalEditorFieldType.CheckBox,
                                        HeaderText = "Отображать на сайте",
                                        DataType = typeof (bool),
                                        Modificators =
                                            new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                    },
                                new UniversalEditorField()
                                    {
                                        FieldName = "Text",
                                        FieldType = UniversalEditorFieldType.TextEditor,
                                        HeaderText = "Текст",
                                        DataType = typeof (string),
                                        Modificators =
                                            new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                    }
                            }

            };




            settings.AutoFilter = true;
            settings.Filters = new List<FilterConfiguration>()
                {
                    new FilterConfiguration()
                        {
                            FilterSource = new UniversalDataSource()
                                {
                                    DefValue = CMSPageID,
                                    HasEmptyDef = false,
                                    KeyField = "ID",
                                    ValueField = "PageName",
                                    Source =


                                        db.CMSPages.Where(
                                            x => 
                                            x.PageType.CMSPageCells.Any(
                                                c =>
                                                c.CMSPageCellViews.Any(
                                                    v => v.Action == "Index" && v.Controller == "TextPage")))
                                          .ToList()
                                          .Select(x => x.LoadLangValues()),



                                },
                            HeaderText = "Страница",
                            IsDropDown = true,
                            QueryKey = "CMSPageID",
                            Type = FilterType.Integer,
                            MainFilter = true
                        },
                    new FilterConfiguration()
                        {
                            FilterSource = new UniversalDataSource()
                                {
                                    DefValue = ViewID,
                                    HasEmptyDef = false,
                                    KeyField = "ID",
                                    ValueField = "Description",
                                    Source = blocks
                                },

                            HeaderText = "Выберите контейнер",
                            IsDropDown = true,
                            QueryKey = "ViewID",
                            Type = FilterType.Integer,

                        }
                };


            var type = (CurrentEditorType)Enum.Parse(typeof(CurrentEditorType), Type ?? "List", true);
            var data = new UniversalEditorPagedData()
            {
                PagedData =
                    type == CurrentEditorType.List
                        ? new PagedData<CMSPageTextData>(
                              db.CMSPageTextDatas.Where(
                                  x => x.ViewID == ViewID && x.LangID == LangID && x.CMSPageID == CMSPageID)
                                .OrderBy(x => x.OrderNum), Page ?? 0, 10, "Master", settings.Filters)
                        : null,
                Settings = settings,
                CurrentType = type,
                EditedRow =
                    type == CurrentEditorType.List
                        ? null
                        : editedRow
            };
            data.AddQueryParams = new[] { "CMSPageID", "LangID", "ViewID" };
            data.IsAddingNew = data.EditedRow != null && ((CMSPageTextData)data.EditedRow).ID == 0;
            data.CallerController = "TableEditors";
            data.CallerAction = "TextEditor";
            data.SaveRow = SaveTextData;
            data.EditorName = "Редактирование текстовых блоков";
            return View("TableEditor", data);

        }

        private string SaveTextData(object row)
        {
            var r = (CMSPageTextData)row;
            try
            {
                if (r == null) return "Объект не найден";
                if (r.ID == 0)
                {
                    r.OrderNum = db.CMSPageTextDatas.Count() + 1;
                    db.CMSPageTextDatas.InsertOnSubmit(r);
                }
                else
                {
                    db.Refresh(RefreshMode.KeepChanges, row);
                }
                db.SubmitChanges();
                return "";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        #endregion

        #region Слайдер
        [AuthorizeMaster]
        public ActionResult Slider(string Type, int? Page, int? UID, int? LangID, int? CMSPageID, int? ViewID)
        {
            if (!LangID.HasValue)
            {
                LangID = (db.Languages.FirstOrDefault(x => x.ShortName == "ru") ?? db.Languages.First()).ID;
            }
            if (!CMSPageID.HasValue)
            {

                CMSPageID = (db.CMSPages.FirstOrDefault(
                    x =>
                    x.PageType.CMSPageCells.Any(
                        c =>
                        c.CMSPageCellViews.Any(
                            v => v.Action == "Slider" && v.Controller == "CommonBlocks"))) ??
                     new CMSPage() { ID = 0 }).ID;


            }

            var views =
                db.CMSPageCellViews.Where(x => x.Action == "Slider" && x.Controller == "CommonBlocks")
                  .Select(x => x.CMSPageCell)
                  .Intersect(db.CMSPages.Where(x => x.ID == CMSPageID).SelectMany(x => x.PageType.CMSPageCells))
                  .ToList()
                  .SelectMany(x => x.CMSPageCellViews.Where(c => c.Action == "Slider" && c.Controller == "CommonBlocks")).Distinct()
                  .GroupBy(x => x.CMSPageCell);


            foreach (var view in views)
            {
                if (view.Count() > 1)
                {
                    for (int i = 1; i <= view.Count(); i++)
                    {
                        view.ElementAt(i - 1).Description += " - блок №" + i;
                    }
                }
            }

            var blocks = views.SelectMany(x => x).ToList();

            if (!ViewID.HasValue)
            {
                ViewID = (blocks.FirstOrDefault() ?? new CMSPageCellView()).ID;
            }


            var editedRow = (db.CMSPageSliders.FirstOrDefault(x => x.ID == UID) ??
                             new CMSPageSlider()
                             {
                                 LangID = LangID.Value,
                                 CMSPageID = CMSPageID.Value,
                                 ViewID = ViewID.Value,
                                 Text = "",
                                 Name = "",
                                 Visible = true,
                                 OrderNum = db.CMSPageSliders.Count() + 1
                             });





            var settings = new UniversalEditorSettings()
                {
                    TableName = "CMSPageSliders",
                    HasDeleteColumn = true,
                    CanAddNew = true,
                    UIDColumnName = "ID",
                    ShowedFieldsInList =
                        new List<UniversalListField>()
                            {
                                new UniversalListField()
                                    {
                                        FieldName = "Name",
                                        IsLinkToEdit = true,
                                        HeaderText = "Название"

                                    },
                                new UniversalListField()
                                    {
                                        FieldName = "Visible",
                                        HeaderText = "Статус",
                                        TextFunction = x => (bool) x ? "Отображается" : "Неактивен"
                                    },
                                new UniversalListField()
                                    {
                                        FieldName = "OrderNum",
                                        IsLinkToEdit = false,
                                        IsOrderColumn = true,
                                        HeaderText = "Порядок"
                                    }
                            },

                    EditedFieldsList =
                        new List<UniversalEditorField>()
                            {
                                new UniversalEditorField()
                                    {
                                        FieldName = "LangID",
                                        Hidden = true,
                                        FieldType = UniversalEditorFieldType.DropDown,
                                        InnerListDataSource = new UniversalDataSource()
                                            {
                                                DefValue = AccessHelper.CurrentLang.ID,
                                                HasEmptyDef = false,
                                                KeyField = "ID",
                                                ValueField = "Name",
                                                Source = db.Languages.Where(x => x.Enabled),

                                            },
                                        HeaderText = "Язык",
                                        Modificators =
                                            new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                    },
                                new UniversalEditorField()
                                    {
                                        FieldName = "CMSPageID",
                                        FieldType = UniversalEditorFieldType.DropDown,
                                        InnerListDataSource = new UniversalDataSource()
                                            {
                                                DefValue = CMSPageID,
                                                HasEmptyDef = false,
                                                KeyField = "ID",
                                                ValueField = "PageName",
                                                Source =
                                                    db.CMSPages.Where(
                                                        x =>
                                                        x.PageType.CMSPageCells.Any(
                                                            c =>
                                                            c.CMSPageCellViews.Any(
                                                                v => v.Action == "Slider" && v.Controller == "CommonBlocks")))
                                                      .ToList()
                                                      .Select(x => x.LoadLangValues()),

                                            },
                                        HeaderText = "Страница",
                                        Modificators =
                                            new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                    },
                                new UniversalEditorField()
                                    {
                                        FieldName = "ViewID",
                                        FieldType = UniversalEditorFieldType.DropDown,
                                        InnerListDataSource = new UniversalDataSource()
                                            {
                                                DefValue = ViewID,
                                                HasEmptyDef = false,
                                                KeyField = "ID",
                                                ValueField = "Description",
                                                Source = blocks,

                                            },
                                        HeaderText = "Контейнер",
                                        Modificators =
                                            new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                    },

                                new UniversalEditorField()
                                    {
                                        FieldName = "Visible",
                                        FieldType = UniversalEditorFieldType.CheckBox,
                                        HeaderText = "Отображать на сайте",
                                        DataType = typeof (bool),
                                        Modificators =
                                            new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                    },

                                new UniversalEditorField()
                                    {
                                        FieldName = "Name",
                                        FieldType = UniversalEditorFieldType.TextBox,
                                        HeaderText = "Название",
                                        DataType = typeof (string),
                                        Modificators =
                                            new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                    },
                                new UniversalEditorField()
                                    {
                                        FieldName = "Text",
                                        FieldType = UniversalEditorFieldType.TextEditor,
                                        HeaderText = "Текст",
                                        DataType = typeof (string),
                                        Modificators =
                                            new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                    },
                                new UniversalEditorField()
                                    {
                                        FieldName = "Img",
                                        FieldType = UniversalEditorFieldType.DBImageUpload,
                                        HeaderText = "Изображение",
                                        Modificators =
                                            new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                    }
                            }

                };




            settings.AutoFilter = true;
            settings.Filters = new List<FilterConfiguration>()
                {
                    new FilterConfiguration()
                        {
                            FilterSource = new UniversalDataSource()
                                {
                                    DefValue = CMSPageID,
                                    HasEmptyDef = false,
                                    KeyField = "ID",
                                    ValueField = "PageName",
                                    Source =


                                        db.CMSPages.Where(
                                            x => 
                                            x.PageType.CMSPageCells.Any(
                                                c =>
                                                c.CMSPageCellViews.Any(
                                                    v => v.Action == "Slider" && v.Controller == "CommonBlocks")))
                                          .ToList()
                                          .Select(x => x.LoadLangValues()),



                                },
                            HeaderText = "Страница",
                            IsDropDown = true,
                            QueryKey = "CMSPageID",
                            Type = FilterType.Integer,
                            MainFilter = true
                        },
                    new FilterConfiguration()
                        {
                            FilterSource = new UniversalDataSource()
                                {
                                    DefValue = ViewID,
                                    HasEmptyDef = false,
                                    KeyField = "ID",
                                    ValueField = "Description",
                                    Source = blocks
                                },

                            HeaderText = "Выберите контейнер",
                            IsDropDown = true,
                            QueryKey = "ViewID",
                            Type = FilterType.Integer,

                        }
                };


            var type = (CurrentEditorType)Enum.Parse(typeof(CurrentEditorType), Type ?? "List", true);
            var data = new UniversalEditorPagedData()
            {
                PagedData =
                    type == CurrentEditorType.List
                        ? new PagedData<CMSPageSlider>(
                              db.CMSPageSliders.Where(
                                  x => x.ViewID == ViewID && x.LangID == LangID && x.CMSPageID == CMSPageID)
                                .OrderBy(x => x.OrderNum), Page ?? 0, 10, "Master", settings.Filters)
                        : null,
                Settings = settings,
                CurrentType = type,
                EditedRow =
                    type == CurrentEditorType.List
                        ? null
                        : editedRow
            };
            data.AddQueryParams = new[] { "CMSPageID", "LangID", "ViewID" };
            data.IsAddingNew = data.EditedRow != null && ((CMSPageSlider)data.EditedRow).ID == 0;
            data.CallerController = "TableEditors";
            data.CallerAction = "Slider";
            data.EditorName = "Редактирование слайдеров";
            return View("TableEditor", data);
        }

        #endregion

        #region Редактирование содержимого разделов
        [AuthorizeMaster]
        public ActionResult ViewManager(string Type, int? Page, int? UID, int? CellID, int? TypeID)
        {
            if (!TypeID.HasValue)
            {
                TypeID = db.PageTypes.First(x => x.TypeName == "MainPage").ID;
            }
            if (!CellID.HasValue)
            {
                CellID = db.CMSPageCells.First(x => x.TypeID == TypeID).ID;
            }

            var editedRow = (db.CMSPageCellViews.FirstOrDefault(x => x.ID == UID) ??
                             new CMSPageCellView()
                             {
                                 Controller = "TextPage",
                                 Action = "Index",
                                 CellID = (int)CellID,
                                 OrderNum = db.CMSPageCellViews.Count() + 1
                             });

            var settings = new UniversalEditorSettings()
            {
                TableName = "CMSPageCellViews",
                HasDeleteColumn = true,
                CanAddNew = true,
                UIDColumnName = "ID",
                ShowedFieldsInList =
                    new List<UniversalListField>()
                            {
                                new UniversalListField()
                                    {
                                        FieldName = "Path",
                                        IsLinkToEdit = true,
                                        HeaderText = "Модуль",
                                        TextFunction =
                                            x =>
                                            (TemplateEditorController.BlockList.FirstOrDefault(
                                                z => z.Path == x.ToString()) ??
                                             new ClientTemplate() {Name = "--Undef--"}).Name
                                    },
                                new UniversalListField()
                                    {
                                        FieldName = "OrderNum",
                                        IsLinkToEdit = false,
                                        IsOrderColumn = true,
                                        HeaderText = "Порядок"
                                    }
                            },

                EditedFieldsList =
                    new List<UniversalEditorField>()
                            {
                                new UniversalEditorField()
                                    {
                                        FieldName = "CellID",
                                        FieldType = UniversalEditorFieldType.DropDown,
                                        InnerListDataSource = new UniversalDataSource()
                                            {
                                                DefValue = CellID,
                                                HasEmptyDef = false,
                                                KeyField = "ID",
                                                ValueField = "Description",
                                                Source = db.CMSPageCells.Where(x => x.TypeID == TypeID && !x.Hidden)
                                            },
                                        HeaderText = "Контейнер",
                                        Modificators =
                                            new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                    },
                                new UniversalEditorField()
                                    {
                                        FieldName = "Path",
                                        FieldType = UniversalEditorFieldType.DropDown,
                                        HeaderText = "Модуль",
                                        InnerListDataSource = new UniversalDataSource()
                                            {
                                                DefValue = editedRow.ID == 0 ? TemplateEditorController.BlockList.First(x=> x.Controller=="TextPage").Path:editedRow.Path,
                                                HasEmptyDef = false,
                                                KeyField = "Path",
                                                ValueField = "Name",
                                                Source = TemplateEditorController.BlockList.Where(x=> x.IsModul)/*.GroupBy(x=> x.Controller).Select(x=> x.First()).Where(x=> x.Name!="Выберите блок")*/
                                            },
                                        DataType = typeof (string),
                                        Modificators =
                                            new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                    },
                               /* new UniversalEditorField()
                                    {
                                        FieldName = "OrderNum",
                                        DataType = typeof (int),
                                        FieldType = UniversalEditorFieldType.TextBox,
                                        HeaderText = "Порядковый номер",
                                    }*/
                            }

            };



            settings.AutoFilter = true;
            settings.Filters = new List<FilterConfiguration>()
                {
                    new FilterConfiguration()
                        {
                            FilterSource = new UniversalDataSource()
                                {
                                    DefValue = TypeID,
                                    HasEmptyDef = false,
                                    KeyField = "ID",
                                    ValueField = "Description",
                                    Source = db.PageTypes
                                },
                            HeaderText = "Выберите тип страницы",
                            IsDropDown = true,
                            QueryKey = "TypeID",
                            Type = FilterType.Integer,
                            SkipInQuery = true,
                            MainFilter = true
                        },
                    new FilterConfiguration()
                        {
                            FilterSource = new UniversalDataSource()
                                {
                                    DefValue = CellID,
                                    HasEmptyDef = false,
                                    KeyField = "ID",
                                    ValueField = "Description",
                                    Source = db.CMSPageCells.Where(x => x.TypeID == TypeID && !x.Hidden)
                                },
                            HeaderText = "Выберите контейнер",
                            IsDropDown = true,
                            QueryKey = "CellID",
                            Type = FilterType.Integer,
                            
                        }
                };



            var type = (CurrentEditorType)Enum.Parse(typeof(CurrentEditorType), Type ?? "List", true);
            var data = new UniversalEditorPagedData()
            {
                PagedData =
                    type == CurrentEditorType.List
                        ? new PagedData<CMSPageCellView>(db.CMSPageCellViews.OrderBy(x => x.OrderNum), Page ?? 0, 10, "Master", settings.Filters)
                        : null,
                Settings = settings,
                CurrentType = type,
                EditedRow =
                    type == CurrentEditorType.List
                        ? null
                        : editedRow
            };
            data.IsAddingNew = data.EditedRow != null && ((CMSPageCellView)data.EditedRow).ID == 0;
            data.CallerController = "TableEditors";
            data.CallerAction = "ViewManager";
            data.SaveRow = SaveCellView;
            data.AddQueryParams = new[] { "CellID", "TypeID" };
            data.EditorName = "Редактирование содержимого разделов";
            return View("TableEditor", data);
        }

        private string SaveCellView(object row)
        {
            var r = (CMSPageCellView)row;
            try
            {
                if (r == null) return "Объект не найден";

                var module = TemplateEditorController.BlockList.First(x => x.Path == r.Path);
                r.Controller = module.Controller;
                r.Action = module.Action;
                r.Path = module.Path;
                if (r.ID == 0)
                {
                    db.CMSPageCellViews.InsertOnSubmit(r);
                }
                else
                {
                    db.Refresh(RefreshMode.KeepChanges, row);
                }
                db.SubmitChanges();
                return "";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
        #endregion

        #region Редактирование ленты событий и новостей

        [AuthorizeMaster]
        public ActionResult LentaEditor(string Type, int? Page, int? UID, int? PageID)
        {
            if (!PageID.HasValue)
            {
                var lentas = CMSPage.GetByType("Lenta").ToList();
                if (lentas.Any())
                    PageID = lentas.First().ID;
            }
            var editedRow = db.Lentas.FirstOrDefault(x => x.ID == UID) ??
                            new Lenta()
                                {
                                    CreateDate = DateTime.Now,
                                    Visible = true,
                                    ShowInfo = false,
                                    ShowTime = false,
                                    ViewAmount = 0
                                };


            var cssList = new[]
                {
                    new {CSS = "stream-block", Name = "--Не выбрано--"},
                    new {CSS = "stream-block_2x", Name = "Двойная ширина"},
                    new {CSS = "stream-block1", Name = "Блок с фоновой картинкой (надпись по центру)"},
                    new {CSS = "stream-block1 title-l", Name = "Блок с фоновой картинкой (надпись слева по центру)"},
                    new {CSS = "stream-block1 title-lb", Name = "Блок с фоновой картинкой (надпись слева-снизу)"},
                    new {CSS = "cite", Name = "Блок в виде цитаты"},
                    new {CSS = "cite1", Name = "Блок в виде цитаты с фото"},
                    new {CSS = "cite cite-big", Name = "Блок в виде цитаты (крупные буквы)"},
                    new {CSS = "-lgreen", Name = "Зеленый фон"},
                    new {CSS = "-pink", Name = "Красный фон"},
                    new {CSS = "-peach", Name = "Персиковый фон"},
                    new {CSS = "title-blue", Name = "Голубой заголовок"},
                };

            var settings = new UniversalEditorSettings()
                {
                    TableName = "Lenta",
                    HasDeleteColumn = true,
                    CanAddNew = true,
                    UIDColumnName = "ID",
                    ShowedFieldsInList =
                        new List<UniversalListField>()
                            {
                                new UniversalListField()
                                    {
                                        FieldName = "PageID",
                                        IsLinkToEdit = true,
                                        HeaderText = "Страница для поста",
                                        TextFunction = x => CMSPage.Get((int) x).PageName
                                    },
                                new UniversalListField()
                                    {
                                        FieldName = "HeaderText",
                                        IsLinkToEdit = true,
                                        HeaderText = "Заголовок",
                                        TextFunction = x=> x.ToString().ClearHTML()
                                    },
                                new UniversalListField() {FieldName = "CreateDate", HeaderText = "Дата создания"},
                                new UniversalListField()
                                    {
                                        FieldName = "Visible",
                                        HeaderText = "Статус",
                                        TextFunction = x => (bool) x ? "Отображается" : "Скрыто"
                                    }
                            },

                    EditedFieldsList =
                        new List<UniversalEditorField>()
                            {
                                new UniversalEditorField()
                                    {
                                        FieldName = "PageID",
                                        FieldType = UniversalEditorFieldType.DropDown,
                                        HeaderText = "Страница",
                                        DataType = typeof (int),
                                        InnerListDataSource = new UniversalDataSource()
                                            {
                                                DefValue = editedRow.PageID,
                                                HasEmptyDef = false,
                                                KeyField = "ID",
                                                ValueField = "PageName",
                                                Source = CMSPage.GetByType("Lenta")
                                            },
                                        Modificators =
                                            new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                    },

                                new UniversalEditorField()
                                    {
                                        FieldName = "CellID",
                                        FieldType = UniversalEditorFieldType.DropDown,
                                        HeaderText = "Контейнер",
                                        DataType = typeof (int),
                                        InnerListDataSource = new UniversalDataSource()
                                            {
                                                DefValue = editedRow.CellID,
                                                HasEmptyDef = false,
                                                KeyField = "ID",
                                                ValueField = "Description",
                                                Source =
                                                    db.CMSPageCells.Where(
                                                        x =>
                                                        x.PageType.TypeName == "Lenta" && x.Hidden).ToList()
                                            }
                                    },

                                new UniversalEditorField()
                                    {
                                        FieldName = "CategoryName",
                                        FieldType = UniversalEditorFieldType.TextBox,
                                        HeaderText = "Категория",
                                        DataType = typeof (string)
                                    },
                                new UniversalEditorField()
                                    {
                                        FieldName = "HeaderText",
                                        FieldType = UniversalEditorFieldType.TextArea,
                                        HeaderText = "Текст в заголовке",
                                        DataType = typeof (string)
                                    },
                                new UniversalEditorField()
                                    {
                                        FieldName = "Image",
                                        FieldType = UniversalEditorFieldType.DBImageUpload,
                                        HeaderText = "Фоновое изображение"
                                    },
                                new UniversalEditorField()
                                    {
                                        FieldName = "Photo",
                                        FieldType = UniversalEditorFieldType.DBImageUpload,
                                        HeaderText = "Фото в рамке"
                                    },
                                new UniversalEditorField()
                                    {
                                        FieldName = "Author",
                                        FieldType = UniversalEditorFieldType.TextBox,
                                        HeaderText = "Автор"
                                    },
                                new UniversalEditorField()
                                    {
                                        FieldName = "Text",
                                        FieldType = UniversalEditorFieldType.TextArea,
                                        HeaderText = "Текст в ленте"
                                    },
                                new UniversalEditorField()
                                    {
                                        FieldName = "ShowInfo",
                                        FieldType = UniversalEditorFieldType.CheckBox,
                                        HeaderText = "Отображать инфо-блок",
                                        DataType = typeof (bool)
                                    },
                                new UniversalEditorField()
                                    {
                                        FieldName = "ShowTime",
                                        FieldType = UniversalEditorFieldType.CheckBox,
                                        HeaderText = "Отображать дату",
                                        DataType = typeof (bool)
                                    },
                                new UniversalEditorField()
                                    {
                                        FieldName = "Visible",
                                        FieldType = UniversalEditorFieldType.CheckBox,
                                        HeaderText = "Отображать в ленте",
                                        DataType = typeof (bool)
                                    },
                                new UniversalEditorField()
                                    {
                                        FieldName = "FullText",
                                        FieldType = UniversalEditorFieldType.TextEditor,
                                        HeaderText = "Полный текст"
                                    },
                                new UniversalEditorField()
                                    {
                                        FieldName = "ViewAmount",
                                        FieldType = UniversalEditorFieldType.Label,
                                        HeaderText = "Количество просмотров"
                                    },
                                new UniversalEditorField()
                                    {
                                        FieldName = "CreateDate",
                                        FieldType = UniversalEditorFieldType.Calendar,
                                        HeaderText = "Дата создания"
                                    },

                                new UniversalEditorField()
                                    {
                                        FieldName = "CSS1",
                                        FieldType = UniversalEditorFieldType.DropDown,
                                        HeaderText = "Стиль CSS 1",
                                        InnerListDataSource = new UniversalDataSource()
                                            {
                                                KeyField = "CSS",
                                                ValueField = "Name",
                                                Source = cssList,
                                                DefValue = editedRow.CSS1.IsNullOrEmpty() ? "stream-block": editedRow.CSS1,
                                                HasEmptyDef = false
                                            }
                                    },
                                new UniversalEditorField()
                                    {
                                        FieldName = "CSS2",
                                        FieldType = UniversalEditorFieldType.DropDown,
                                        HeaderText = "Стиль CSS 2",
                                        InnerListDataSource = new UniversalDataSource()
                                            {
                                                KeyField = "CSS",
                                                ValueField = "Name",
                                                Source = cssList,
                                                DefValue = editedRow.CSS2.IsNullOrEmpty() ? "stream-block": editedRow.CSS2
                                            }
                                    },
                                new UniversalEditorField()
                                    {
                                        FieldName = "CSS3",
                                        FieldType = UniversalEditorFieldType.DropDown,
                                        HeaderText = "Стиль CSS 3",
                                        InnerListDataSource = new UniversalDataSource()
                                            {
                                                KeyField = "CSS",
                                                ValueField = "Name",
                                                Source = cssList,
                                                DefValue = editedRow.CSS3.IsNullOrEmpty() ? "stream-block": editedRow.CSS3
                                            }
                                    },
                                new UniversalEditorField()
                                    {
                                        FieldName = "CSS4",
                                        FieldType = UniversalEditorFieldType.DropDown,
                                        HeaderText = "Стиль CSS 4",
                                        InnerListDataSource = new UniversalDataSource()
                                            {
                                                KeyField = "CSS",
                                                ValueField = "Name",
                                                Source = cssList,
                                                DefValue = editedRow.CSS4.IsNullOrEmpty() ? "stream-block": editedRow.CSS4
                                            }
                                    },

                                new UniversalEditorField()
                                    {
                                        FieldName = "CSS5",
                                        FieldType = UniversalEditorFieldType.DropDown,
                                        HeaderText = "Стиль CSS 5",
                                        InnerListDataSource = new UniversalDataSource()
                                            {
                                                KeyField = "CSS",
                                                ValueField = "Name",
                                                Source = cssList,
                                                DefValue = editedRow.CSS5.IsNullOrEmpty() ? "stream-block": editedRow.CSS5
                                            }
                                    },


                            },
                    AutoFilter = true,
                    Filters = new List<FilterConfiguration>()
                        {
                            new FilterConfiguration()
                                {
                                    FilterSource = new UniversalDataSource()
                                        {
                                            DefValue = editedRow.PageID == 0 ? PageID : editedRow.PageID,
                                            HasEmptyDef = false,
                                            KeyField = "ID",
                                            ValueField = "PageName",
                                            Source = CMSPage.GetByType("Lenta")
                                        },
                                    HeaderText = "Страница",
                                    QueryKey = "PageID",
                                    IsDropDown = true,
                                    Type = FilterType.Integer,


                                }
                        }

                };


            var type = (CurrentEditorType)Enum.Parse(typeof(CurrentEditorType), Type ?? "List", true);
            var data = new UniversalEditorPagedData()
                {
                    PagedData =
                        type == CurrentEditorType.List
                            ? new PagedData<Lenta>(db.Lentas.OrderByDescending(x => x.CreateDate), Page ?? 0, 10, "Master", settings.Filters)
                            : null,
                    Settings = settings,
                    CurrentType = type,
                    EditedRow =
                        type == CurrentEditorType.List
                            ? null
                            : editedRow
                };
            data.AddQueryParams = new[] { "PageID" };
            data.IsAddingNew = data.EditedRow != null && ((Lenta)data.EditedRow).ID == 0;
            data.CallerController = "TableEditors";
            data.CallerAction = "LentaEditor";
            data.SaveRow = SaveLenta;

            data.EditorName = "Редактирование ленты событий и новостей";
            return View("TableEditor", data);
        }

        private string SaveLenta(object game)
        {
            try
            {
                if (game == null) return "Объект не найден";
                var tGame = (Lenta)game;
                tGame.TypeClass =
                    new[] { tGame.CSS1, tGame.CSS2, tGame.CSS3, tGame.CSS4, tGame.CSS5, "stream-block" }.SelectMany(
                        x => x.Split<string>(" ")).Distinct().OrderBy(x => x).JoinToString(" ");

                if (tGame.ID == 0)
                {
                    db.Lentas.InsertOnSubmit(tGame);
                }
                else
                {
                    db.Refresh(RefreshMode.KeepChanges, game);
                }
                db.SubmitChanges();
                return "";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
        #endregion

        /* Старый код из проекта (оставлен, чтобы не забыть как пользоваться классом)
         * [AuthorizeMaster]
           public ActionResult GameEditor(string Type, int? Page, int? UID)
           {
               var settings = new UniversalEditorSettings()
                   {
                       TableName = "Games",
                       HasDeleteColumn = true,
                       CanAddNew = true,
                       UIDColumnName = "ID",
                       ShowedFieldsInList =
                           new List<UniversalListField>()
                               {
                                   new UniversalListField()
                                       {
                                           FieldName = "Name",
                                           IsLinkToEdit = true,
                                           HeaderText = "Название"
                                       },
                                   new UniversalListField() {FieldName = "SuperPrize", HeaderText = "Суперприз"},
                                   new UniversalListField()
                                       {
                                           FieldName = "OrderNum",
                                           IsOrderColumn = true,
                                           HeaderText = "Порядок"
                                       }
                               },

                       EditedFieldsList =
                           new List<UniversalEditorField>()
                               {
                                   new UniversalEditorField()
                                       {
                                           FieldName = "Name",
                                           FieldType = UniversalEditorFieldType.TextBox,
                                           HeaderText = "Название",
                                           Modificators =
                                               new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                       },
                                   new UniversalEditorField()
                                       {
                                           FieldName = "SuperPrize",
                                           FieldType = UniversalEditorFieldType.TextBox,
                                           HeaderText = "Суперприз",
                                           DataType = typeof (int),
                                           Modificators =
                                               new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                       }
                               }

                   };
               var type = (CurrentEditorType)Enum.Parse(typeof(CurrentEditorType), Type ?? "List", true);
               var data = new UniversalEditorPagedData()
                   {
                       PagedData =
                           type == CurrentEditorType.List
                               ? new PagedData<object>(db.Games.OrderBy(x => x.OrderNum), Page ?? 0, 10, "Master")
                               : null,
                       Settings = settings,
                       CurrentType = type,
                       EditedRow =
                           type == CurrentEditorType.List
                               ? null
                               : (db.Games.FirstOrDefault(x => x.ID == UID) ?? new Game())
                   };
               data.IsAddingNew = data.EditedRow != null && ((Game)data.EditedRow).ID == 0;
               data.CallerController = "TableEditors";
               data.CallerAction = "GameEditor";
               data.SaveRow = SaveGame;

               data.EditorName = "Редактирование типов игр";
               return View("TableEditor", data);
           }

           private string SaveGame(object game)
           {
               try
               {
                   if (game == null) return "Объект не найден";
                   var tGame = (Game)game;
                   if (tGame.ID == 0)
                   {
                       db.Games.InsertOnSubmit(tGame);
                       tGame.OrderNum = db.Games.Count() + 1;
                   }
                   else
                   {
                       db.Refresh(RefreshMode.KeepCurrentValues, game);
                   }
                   db.SubmitChanges();
                   return "";
               }
               catch (Exception e)
               {
                   return e.Message;
               }
           }

           [AuthorizeMaster]
           public ActionResult LuckyEditor(string Type, int? Page, int? UID)
           {
               var settings = new UniversalEditorSettings()
                   {
                       TableName = "LuckyPeople",
                       HasDeleteColumn = false,
                       CanAddNew = true,
                       UIDColumnName = "ID",
                       ShowedFieldsInList =
                           new List<UniversalListField>()
                             {
                                 new UniversalListField()
                                     {
                                         FieldName = "ActionDate",
                                         IsLinkToEdit = true,
                                         HeaderText = "Дата розыгрыша"
                                     },
                                 new UniversalListField()
                                     {
                                         FieldName = "IsCompleted",
                                         IsLinkToEdit = true,
                                         HeaderText = "Статус",
                                         TextFunction = StatusFunc
                                     }
                             },

                       EditedFieldsList =
                           new List<UniversalEditorField>()
                             {
                                 new UniversalEditorField()
                                     {
                                         FieldName = "ActionDate",
                                         FieldType = UniversalEditorFieldType.Calendar,
                                         DataType = typeof (DateTime),
                                         HeaderText = "Дата розыгрыша",
                                         Modificators =
                                             new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                     },
                                 new UniversalEditorField()
                                     {
                                         FieldName = "MoneyPrize1",
                                         FieldType = UniversalEditorFieldType.TextBox,
                                         HeaderText = "Приз за первое место в реальном режиме",
                                         DataType = typeof (decimal),
                                         Modificators =
                                             new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                     },
                                 new UniversalEditorField()
                                     {
                                         FieldName = "MoneyPrize2",
                                         FieldType = UniversalEditorFieldType.TextBox,
                                         HeaderText = "Приз за второе место в реальном режиме",
                                         DataType = typeof (decimal),
                                         Modificators =
                                             new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                     },
                                 new UniversalEditorField()
                                     {
                                         FieldName = "MoneyPrize3",
                                         FieldType = UniversalEditorFieldType.TextBox,
                                         HeaderText = "Приз за третье место в реальном режиме",
                                         DataType = typeof (decimal),
                                         Modificators =
                                             new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                     },
                                 new UniversalEditorField()
                                     {
                                         FieldName = "ChipPrize1",
                                         FieldType = UniversalEditorFieldType.TextBox,
                                         HeaderText = "Приз за первое место в тактическом режиме",
                                         DataType = typeof (decimal),
                                         Modificators =
                                             new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                     },
                                 new UniversalEditorField()
                                     {
                                         FieldName = "ChipPrize2",
                                         FieldType = UniversalEditorFieldType.TextBox,
                                         HeaderText = "Приз за второе место в тактическом режиме",
                                         DataType = typeof (decimal),
                                         Modificators =
                                             new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                     },
                                 new UniversalEditorField()
                                     {
                                         FieldName = "ChipPrize3",
                                         FieldType = UniversalEditorFieldType.TextBox,
                                         HeaderText = "Приз за третье место в тактическом режиме",
                                         DataType = typeof (decimal),
                                         Modificators =
                                             new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                     },
                                 new UniversalEditorField()
                                     {
                                         FieldName = "IsCompleted",
                                         FieldType = UniversalEditorFieldType.CheckBox,
                                         HeaderText = "Игра завершена?",
                                         DataType = typeof (bool),
                                     }
                             }

                   };
               var type = (CurrentEditorType)Enum.Parse(typeof(CurrentEditorType), Type ?? "List", true);
               var data = new UniversalEditorPagedData()
                   {
                       PagedData =
                           type == CurrentEditorType.List
                               ? new PagedData<object>(db.LuckyPeoples.OrderByDescending(x => x.ActionDate), Page ?? 0, 10, "Master")
                               : null,
                       Settings = settings,
                       CurrentType = type,
                       EditedRow =
                           type == CurrentEditorType.List
                               ? null
                               : (db.LuckyPeoples.FirstOrDefault(x => x.ID == UID) ?? new LuckyPeople())
                   };
               data.IsAddingNew = data.EditedRow != null && ((LuckyPeople)data.EditedRow).ID == 0;
               data.CallerController = "TableEditors";
               data.CallerAction = "LuckyEditor";
               data.SaveRow = SaveLucky;

               data.AddView = new UniversalEditorAddViewInfo()
               {
                   InEditor = true,
                   Action = "LuckWinnersAddView",
                   Controller = "MainPage",
                   Routes = HttpContext == null || HttpContext.Request == null ||
                       HttpContext.Request.QueryString["UID"] == null
                           ? new RouteValueDictionary()
                           : new RouteValueDictionary() { { "ID", HttpContext.Request.QueryString["UID"] } }
               };

               data.EditorName = "Редактирование типов игр";
               return View("TableEditor", data);
           }

           private string StatusFunc(object input)
           {
               return input.ToTypedValue<bool>() ? "Завершено" : "Ожидание даты розыгрыша";
           }

           private string SaveLucky(object lucky)
           {
               try
               {
                   if (lucky == null) return "Объект не найден";
                   var tGame = (LuckyPeople)lucky;
                   if (tGame.ID == 0)
                   {
                       db.LuckyPeoples.InsertOnSubmit(tGame);
                   }
                   else
                   {
                       db.Refresh(RefreshMode.KeepCurrentValues, lucky);
                   }
                   db.SubmitChanges();
                   return "";
               }
               catch (Exception e)
               {
                   return e.Message;
               }
           }

           [AuthorizeMaster]
           public ActionResult BetEditor(string Type, int? Page, int? UID)
           {
               var type = (CurrentEditorType)Enum.Parse(typeof(CurrentEditorType), Type ?? "List", true);
               var er = type == CurrentEditorType.List
                            ? null
                            : (db.GameBets.FirstOrDefault(x => x.ID == UID) ?? new GameBet());
               var defGame = db.Games.OrderBy(x => x.OrderNum).FirstOrDefault();
               var gameDataSource = new UniversalDataSource()
                   {
                       DefValue = er == null ? (defGame == null ? null : (object)defGame.ID) : er.GameID,
                       HasEmptyDef = false,
                       KeyField = "ID",
                       ValueField = "Name",
                       Source = db.Games.AsEnumerable()
                   };
               var settings = new UniversalEditorSettings()
                   {
                       AutoFilter = true,
                       TableName = "GameBets",
                       HasDeleteColumn = true,
                       CanAddNew = true,
                       UIDColumnName = "ID",
                       ShowedFieldsInList =
                           new List<UniversalListField>()
                               {
                                   new UniversalListField()
                                       {
                                           FieldName = "DigitAmount",
                                           IsLinkToEdit = true,
                                           HeaderText = "Количество цифр"
                                       },
                                   new UniversalListField()
                                       {
                                           FieldName = "Bet",
                                           HeaderText = "Цена ставки"
                                       },
                                   new UniversalListField() {FieldName = "Chance", HeaderText = "Вероятность выигрыша"},
                                   new UniversalListField() {FieldName = "CombiAmount", HeaderText = "Кол-во комбинаций"},
                                   new UniversalListField() {FieldName = "PlayerAmount", HeaderText = "Количество игроков"},
                                   new UniversalListField()
                                       {
                                           FieldName = "OrderNum",
                                           IsOrderColumn = true,
                                           HeaderText = "Порядок"
                                       }
                               },


                       EditedFieldsList =
                           new List<UniversalEditorField>()
                               {
                                   new UniversalEditorField()
                                       {
                                           FieldName = "GameID",
                                           FieldType = UniversalEditorFieldType.DropDown,
                                           DataType = typeof (int),
                                           HeaderText = "Тип игры",
                                           Modificators =
                                               new List<IUniversalFieldModificator>() {new RequiredModificator()},
                                           InnerListDataSource = gameDataSource
                                       },
                                   new UniversalEditorField()
                                       {
                                           FieldName = "DigitAmount",
                                           FieldType = UniversalEditorFieldType.TextBox,
                                           DataType = typeof (int),
                                           HeaderText = "Количество цифр",
                                           Modificators =
                                               new List<IUniversalFieldModificator>()
                                                   {
                                                       new RequiredModificator(),
                                                       new RangeModificator(5, decimal.MaxValue)
                                                   }
                                       },
                                   new UniversalEditorField()
                                       {
                                           FieldName = "Bet",
                                           FieldType = UniversalEditorFieldType.TextBox,
                                           HeaderText = "Цена ставки",
                                           DataType = typeof (decimal),
                                           Modificators =
                                               new List<IUniversalFieldModificator>()
                                                   {
                                                       new RequiredModificator(),
                                                       new RangeModificator(0, decimal.MaxValue)
                                                   }
                                       },
                                   new UniversalEditorField()
                                       {
                                           FieldName = "Chance",
                                           FieldType = UniversalEditorFieldType.TextBox,
                                           HeaderText = "Вероятность выигрыша",
                                           DataType = typeof (int),
                                           Modificators =
                                               new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                       },
                                   new UniversalEditorField()
                                       {
                                           FieldName = "CombiAmount",
                                           FieldType = UniversalEditorFieldType.TextBox,
                                           HeaderText = "Кол-во комбинаций",
                                           DataType = typeof (int),
                                           Modificators =
                                               new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                       },
                                   new UniversalEditorField()
                                       {
                                           FieldName = "PlayerAmount",
                                           FieldType = UniversalEditorFieldType.TextBox,
                                           HeaderText = "Количество игроков в синдикате",
                                           DataType = typeof (int),
                                           Modificators =
                                               new List<IUniversalFieldModificator>()
                                                   {
                                                       new RequiredModificator(),
                                                       new RangeModificator(0, decimal.MaxValue)
                                                   }
                                       },
                                   new UniversalEditorField()
                                       {
                                           FieldName = "IsSuper",
                                           FieldType = UniversalEditorFieldType.CheckBox,
                                           HeaderText = "Суперсиндикат",
                                           DataType = typeof (bool),
                                           Modificators =
                                               new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                       }

                               }

                   };


               var filters = new List<FilterConfiguration>()
                   {
                       new FilterConfiguration()
                           {
                               FilterSource = gameDataSource,
                               IsDropDown = true,
                               QueryKey = "GameID",
                               HeaderText = "Выберите игру",
                               Type = FilterType.Integer
                           }
                   };
               var data = new UniversalEditorPagedData()
                   {
                       PagedData =
                           type == CurrentEditorType.List
                               ? new PagedData<GameBet>(db.GameBets.OrderBy(x => x.OrderNum), Page ?? 0, 15, "Master",
                                                        filters)
                               : null,
                       Settings = settings,
                       CurrentType = type,
                       EditedRow = er

                   };
               data.Settings.Filters = filters;

               data.IsAddingNew = data.EditedRow != null && ((GameBet)data.EditedRow).ID == 0;
               data.CallerController = "TableEditors";
               data.CallerAction = "BetEditor";
               data.SaveRow = SaveGameBet;
               data.EditorName = "Управление ставками";
               return View("TableEditor", data);
           }

           private string SaveGameBet(object bet)
           {
               try
               {
                   if (bet == null) return "Объект не найден";
                   var tBet = (GameBet)bet;

                   if (tBet.ID == 0)
                   {
                       db.GameBets.InsertOnSubmit(tBet);
                       tBet.OrderNum = db.GameBets.Count() + 1;
                   }
                   else
                   {
                       //db.GameBets.Attach(tBet);
                       db.Refresh(RefreshMode.KeepCurrentValues, tBet);
                   }
                   db.SubmitChanges();
                   return "";
               }
               catch (Exception e)
               {
                   return e.Message;
               }
           }

           public string EditMoneyWins(object input)
           {
               return "<a href='{0}'>редактировать выигрыши</a>".FormatWith(Url.Action("WinsEditor", "MainPage", new { ID = input }));
           }

           [AuthorizeMaster]
           public ActionResult PlanEditor(string Type, int? Page, int? UID)
           {
               var type = (CurrentEditorType)Enum.Parse(typeof(CurrentEditorType), Type ?? "List", true);
               var er = type == CurrentEditorType.List
                            ? null
                            : (db.GamePlans.FirstOrDefault(x => x.ID == UID) ?? new GamePlan());
               var defGame = db.Games.OrderBy(x => x.OrderNum).FirstOrDefault();
               var gameDataSource = new UniversalDataSource()
                   {
                       DefValue = er == null ? (defGame == null ? null : (object)defGame.ID) : er.GameID,
                       HasEmptyDef = false,
                       KeyField = "ID",
                       ValueField = "Name",
                       Source = db.Games.AsEnumerable()
                   };
               var settings = new UniversalEditorSettings()
                   {
                       AutoFilter = true,
                       TableName = "GamePlans",
                       HasDeleteColumn = true,
                       CanAddNew = true,
                       UIDColumnName = "ID",
                       ShowedFieldsInList =
                           new List<UniversalListField>()
                               {
                                   new UniversalListField()
                                       {
                                           FieldName = "Date",
                                           IsLinkToEdit = true,
                                           HeaderText = "Дата тиража",
                                       },
                                   new UniversalListField()
                                       {
                                           FieldName = "Number",
                                           IsLinkToEdit = false,
                                           HeaderText = "Номер тиража"
                                       },
                                   new UniversalListField()
                                       {
                                           FieldName = "WinnerCombination",
                                           IsLinkToEdit = false,
                                           HeaderText = "Выигрышная комбинация", 
                                           TextFunction = UniversalEditorSettings.DefaultTextCheckerNoReplace
                                       },

                                   new UniversalListField()
                                       {
                                           FieldName = "ID",
                                           HeaderText = "",
                                           TextFunction = EditMoneyWins
                                       }
                               },


                       EditedFieldsList =
                           new List<UniversalEditorField>()
                               {
                                   new UniversalEditorField()
                                       {
                                           FieldName = "GameID",
                                           FieldType = UniversalEditorFieldType.DropDown,
                                           DataType = typeof (int),
                                           HeaderText = "Тип игры",
                                           Modificators =
                                               new List<IUniversalFieldModificator>() {new RequiredModificator()},
                                           InnerListDataSource = gameDataSource
                                       },
                                   new UniversalEditorField()
                                       {
                                           FieldName = "Date",
                                           FieldType = UniversalEditorFieldType.Calendar,
                                           DataType = typeof (DateTime),
                                           HeaderText = "Дата тиража",
                                           Modificators =
                                               new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                       },
                                   new UniversalEditorField()
                                       {
                                           FieldName = "Number",
                                           FieldType = UniversalEditorFieldType.TextBox,
                                           DataType = typeof (string),
                                           HeaderText = "Номер тиража",
                                           Modificators =
                                               new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                       },
                                   new UniversalEditorField()
                                       {
                                           FieldName = "SuperPrize",
                                           FieldType = UniversalEditorFieldType.TextBox,
                                           DataType = typeof (int),
                                           HeaderText = "Суперприз"
                                       },
                                   new UniversalEditorField()
                                       {
                                           FieldName = "WinnerCombination",
                                           FieldType = UniversalEditorFieldType.TextBox,
                                           DataType = typeof (string),
                                           HeaderText = "Выигрышная комбинация (через ;)"
                                       }
                               }
                   };


               var filters = new List<FilterConfiguration>()
                   {
                       new FilterConfiguration()
                           {
                               FilterSource = gameDataSource,
                               IsDropDown = true,
                               QueryKey = "GameID",
                               HeaderText = "Выберите игру",
                               Type = FilterType.Integer
                           }
                   };
               var data = new UniversalEditorPagedData()
                   {
                       PagedData =
                           type == CurrentEditorType.List
                               ? new PagedData<GamePlan>(db.GamePlans.OrderByDescending(x => x.Date), Page ?? 0, 15,
                                                         "Master", filters)
                               : null,
                       Settings = settings,
                       CurrentType = type,
                       EditedRow = er

                   };
               data.Settings.Filters = filters;

               data.IsAddingNew = data.EditedRow != null && ((GamePlan)data.EditedRow).ID == 0;
               data.CallerController = "TableEditors";
               data.CallerAction = "PlanEditor";
               data.SaveRow = SaveGamePlan;
               data.EditorName = "Управление расписанием";
               return View("TableEditor", data);
           }

           private string SaveGamePlan(object bet)
           {
               try
               {
                   if (bet == null) return "Объект не найден";
                   var tBet = (GamePlan)bet;

                   if (tBet.ID == 0)
                   {
                       db.GamePlans.InsertOnSubmit(tBet);
                   }
                   else
                   {
                       db.Refresh(RefreshMode.KeepCurrentValues, tBet);
                   }
                   db.SubmitChanges();
                   return "";
               }
               catch (Exception e)
               {
                   return e.Message;
               }
           }


           [AuthorizeMaster]
           public ActionResult FAQEditor(string Type, int? Page, int? UID)
           {
               var settings = new UniversalEditorSettings()
                   {
                       TableName = "FAQs",
                       HasDeleteColumn = true,
                       CanAddNew = true,
                       UIDColumnName = "ID",
                       ShowedFieldsInList =
                           new List<UniversalListField>()
                               {
                                   new UniversalListField()
                                       {
                                           FieldName = "Qst",
                                           IsLinkToEdit = true,
                                           HeaderText = "Вопрос"
                                       },
                                   new UniversalListField() {FieldName = "Author", HeaderText = "Автор"},
                                   new UniversalListField()
                                       {
                                           FieldName = "OrderNum",
                                           IsOrderColumn = true,
                                           HeaderText = "Порядок"
                                       }
                               },

                       EditedFieldsList =
                           new List<UniversalEditorField>()
                               {
                                   new UniversalEditorField()
                                       {
                                           FieldName = "LangID",
                                           FieldType = UniversalEditorFieldType.DropDown,
                                           HeaderText = "Язык",
                                           DataType = typeof(int),
                                           ReadOnly = true,
                                           InnerListDataSource = new UniversalDataSource()
                                               {
                                                   DefValue = AccessHelper.CurrentLang.ID,
                                                   HasEmptyDef = false,
                                                
                                                   KeyField = "ID",
                                                   ValueField = "Name",
                                                   Source = db.Languages.ToList(),
                                                
                                               },
                                           Modificators =
                                               new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                       },    
                                    
                                       new UniversalEditorField()
                                       {
                                           FieldName = "Qst",
                                           FieldType = UniversalEditorFieldType.TextArea,
                                           HeaderText = "Вопрос",
                                           Modificators =
                                               new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                       },
                                   new UniversalEditorField()
                                       {
                                           FieldName = "Author",
                                           FieldType = UniversalEditorFieldType.TextBox,
                                           HeaderText = "Автор",

                                       },
                                   new UniversalEditorField()
                                       {
                                           FieldName = "Theme",
                                           FieldType = UniversalEditorFieldType.TextBox,
                                           HeaderText = "Тема",

                                       },
                                   new UniversalEditorField()
                                       {
                                           FieldName = "Section",
                                           FieldType = UniversalEditorFieldType.TextBox,
                                           HeaderText = "Раздел",

                                       },
                                   new UniversalEditorField()
                                       {
                                           FieldName = "Ans",
                                           FieldType = UniversalEditorFieldType.TextArea,
                                           HeaderText = "Ответ",
                                       },
                                   new UniversalEditorField()
                                       {
                                           FieldName = "CreateDate",
                                           FieldType = UniversalEditorFieldType.Calendar,
                                           HeaderText = "Дата",
                                           DataType = typeof (DateTime),
                                           Modificators =
                                               new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                       }
                               }

                   };
               var type = (CurrentEditorType)Enum.Parse(typeof(CurrentEditorType), Type ?? "List", true);
               var data = new UniversalEditorPagedData()
               {
                   PagedData =
                       type == CurrentEditorType.List
                           ? new PagedData<object>(db.FAQs.Where(x => x.LangID == AccessHelper.CurrentLang.ID).OrderBy(x => x.OrderNum), Page ?? 0, 10, "Master")
                           : null,
                   Settings = settings,
                   CurrentType = type,
                   EditedRow =
                       type == CurrentEditorType.List
                           ? null
                           : (db.FAQs.FirstOrDefault(x => x.ID == UID) ?? new FAQ())
               };
               data.IsAddingNew = data.EditedRow != null && ((FAQ)data.EditedRow).ID == 0;
               data.CallerController = "TableEditors";
               data.CallerAction = "FAQEditor";
               data.SaveRow = SaveFAQ;

               data.EditorName = "Редактирование вопросов-ответов";
               return View("TableEditor", data);
           }


           private string SaveFAQ(object bet)
           {
               try
               {
                   if (bet == null) return "Объект не найден";
                   var tBet = (FAQ)bet;

                   if (tBet.ID == 0)
                   {
                       tBet.LangID = AccessHelper.CurrentLang.ID;
                       db.FAQs.InsertOnSubmit(tBet);
                   }
                   else
                   {
                       db.Refresh(RefreshMode.KeepCurrentValues, tBet);
                   }
                   db.SubmitChanges();
                   return "";
               }
               catch (Exception e)
               {
                   return e.Message;
               }
           }

           [AuthorizeMaster]
           public ActionResult Syndicates(string Type, int? Page, int? UID)
           {

               int syndType = Session == null ? 1 : (int)(Session["SyndicateType"] ?? 1);

               var type = (CurrentEditorType)Enum.Parse(typeof(CurrentEditorType), Type ?? "List", true);
               var source = db.Syndicates/*.Where(
                   x =>
                   syndType == -1 || (syndType == 0
                                          ? x.GameType == (int) SyndicateType.Tactic
                                          : (x.GameType == (int) SyndicateType.Real || x.GameType == (int) SyndicateType.Vip)))#1#
                              .OrderBy(x => x.PlayDate.HasValue)
                              .ThenByDescending(x => x.PlayDate)
                              .ToList()
                              .Select(x => x.PlayDate)
                              .Distinct()
                              .Select(
                                  x =>
                                  new KeyValuePair<string, string>(
                                      x.HasValue ? x.Value.ToString("yyyy.MM.ddTHH:mm:ss.fff") : "",
                                      x.HasValue ? x.Value.ToString("d MMMMM yyyy, HH:mm") : "Тираж не определен"));

               var er = type == CurrentEditorType.List
                          ? null
                          : (db.Syndicates.FirstOrDefault(x => x.ID == UID) ?? new Models.Syndicate());

               var defGame = (object)null;
               var gameDataSource = new UniversalDataSource()
                   {
                       DefValue = er == null || !er.PlayDate.HasValue ? (defGame == null ? null : (object)defGame) : er.PlayDate.Value.ToString("yyyy.MM.ddTHH:mm:ss.fff"),
                       HasEmptyDef = false,
                       KeyField = "Key",
                       ValueField = "Value",
                       Source = source
                   };


               var settings = new UniversalEditorSettings()
                   {
                       AutoFilter = true,
                       TableName = "Syndicate",
                       HasDeleteColumn = false,
                       CanAddNew = false,
                       UIDColumnName = "ID",
                       ShowedFieldsInList =
                           new List<UniversalListField>()
                               {
                                   new UniversalListField()
                                       {
                                           FieldName = "ID",
                                           IsLinkToEdit = true,
                                           HeaderText = "№ синдиката",
                                           TextFunction = CreateNum
                                       },
                                   new UniversalListField()
                                       {
                                           FieldName = "Status",
                                           HeaderText = "Текущий статус"
                                       },
                                   new UniversalListField()
                                       {
                                           FieldName = "GameName",
                                           HeaderText = "Тип игры"
                                       },
                                   new UniversalListField()
                                       {
                                           FieldName = "Combination",
                                           HeaderText = "Комбинация синдиката",
                                           TextFunction = UniversalEditorSettings.DefaultTextCheckerNoReplace
                                       },

                                   new UniversalListField()
                                       {
                                           FieldName = "TicketNumber",
                                           HeaderText = "Номер билета",
                                           TextFunction = UniversalEditorSettings.DefaultTextCheckerNoReplace
                                       },
                                   new UniversalListField()
                                       {
                                           FieldName = "ID",
                                           HeaderText = "",
                                           TextFunction = CancelTextFunc
                                       },
                                   new UniversalListField()
                                       {
                                           FieldName = "GameType",
                                           HeaderText = "",
                                           TextFunction = GameTypeTextFunc
                                       }
                               },


                       EditedFieldsList =
                           new List<UniversalEditorField>()
                               {
                                   new UniversalEditorField()
                                       {
                                           FieldName = "ID",
                                           FieldType = UniversalEditorFieldType.Label,
                                           DataType = typeof (int),
                                           HeaderText = "Номер синдиката",
                                           TextFunction = CreateNum
                                       },
                                   new UniversalEditorField()
                                       {
                                           FieldName = "Bet",
                                           FieldType = UniversalEditorFieldType.Label,
                                           DataType = typeof (decimal),
                                           HeaderText = "Ставка",
                                       },

                                   new UniversalEditorField()
                                       {
                                           FieldName = "TicketNumber",
                                           FieldType = UniversalEditorFieldType.TextBox,
                                           DataType = typeof (string),
                                           HeaderText = "Номер билета",
                                       },
                                   new UniversalEditorField()
                                       {
                                           FieldName = "IsPlayed",
                                           FieldType = UniversalEditorFieldType.CheckBox,
                                           HeaderText = "Тираж состоялся",
                                           DataType = typeof (bool),
                                       },                               
                                
                                   new UniversalEditorField()
                                       {
                                           FieldName = "CloseDate",
                                           FieldType = UniversalEditorFieldType.Label,
                                           HeaderText = "Дата формирования синдиката",
                                           DataType = typeof (DateTime),
                                       },
                                   new UniversalEditorField()
                                       {
                                           FieldName = "Combination",
                                           FieldType = UniversalEditorFieldType.TextBox,
                                           ReadOnly = true,
                                           HeaderText = "Комбинация синдиката"
                                       },
                                   new UniversalEditorField()
                                       {
                                           FieldName = "PlayDate",
                                           FieldType = UniversalEditorFieldType.Calendar,
                                           HeaderText = "Дата тиража",
                                           DataType = typeof (DateTime),
                                       },
                                   new UniversalEditorField()
                                       {
                                           FieldName = "WinnerCombination",
                                           FieldType = UniversalEditorFieldType.TextBox,
                                           DataType = typeof (DateTime),
                                           HeaderText = "Выигрышная комбинация"
                                       },
                                   new UniversalEditorField()
                                       {
                                           FieldName = "OverageWin",
                                           FieldType = UniversalEditorFieldType.TextBox,
                                           DataType = typeof (decimal),
                                           HeaderText = "Выигрыш на человека"
                                       },

                               }

                   };


               var filters = new List<FilterConfiguration>()
                   {
                       new FilterConfiguration()
                           {
                               FilterSource = gameDataSource,
                               IsDropDown = true,
                               QueryKey = "PlayDate",
                               HeaderText = "Выберите дату тиража",
                               Type = FilterType.Date
                           }
                   };
               var data = new UniversalEditorPagedData()
                   {
                       PagedData =
                           type == CurrentEditorType.List
                               ? new PagedData<Models.Syndicate>(
                                     db.Syndicates.Where(x => x.CloseDate.HasValue && (syndType == -1 || (syndType == 0
                                                                                                              ? x.GameType ==
                                                                                                                (int)
                                                                                                                SyndicateType
                                                                                                                    .Tactic
                                                                                                              : (x.GameType ==
                                                                                                                 (int)
                                                                                                                 SyndicateType
                                                                                                                     .Real ||
                                                                                                                 x.GameType ==
                                                                                                                 (int)
                                                                                                                 SyndicateType
                                                                                                                     .Vip))))
                                       .OrderBy(x => x.PlayDate), Page ?? 0, 15, "Master",
                                     filters)
                               : null,
                       Settings = settings,
                       CurrentType = type,
                       EditedRow = er

                   };
               data.Settings.Filters = filters;

               data.IsAddingNew = false;
               data.CallerController = "TableEditors";
               data.CallerAction = "Syndicates";
               data.SaveRow = SaveSyndicate;
               data.EditorName = "Список синдикатов";

               if (type == CurrentEditorType.List)
               {
                   data.AddView = new UniversalEditorAddViewInfo()
                       {
                           InEditor = false,
                           Action = "SyndicateGameAddView",
                           Controller = "MainPage",
                           Routes = HttpContext == null || HttpContext.Request == null ||
                                    HttpContext.Request.QueryString["PlayDate"] == null
                                        ? new RouteValueDictionary()
                                        : new RouteValueDictionary()
                                            {
                                                {"PlayDate", HttpContext.Request.QueryString["PlayDate"]}
                                            }
                       };
               }
               else if (type == CurrentEditorType.Edit && UID > 0)
               {
                   data.AddView = new UniversalEditorAddViewInfo()
                       {
                           InEditor = true,
                           Action = "SyndicateTicket",
                           Controller = "MainPage",
                           Routes =
                               new RouteValueDictionary()
                                   {
                                       {"SyndicateID", UID}
                                   }
                       };

               }


               return View("TableEditor", data);
           }

           private string GameTypeTextFunc(object input)
           {
               return string.Format("<input type='hidden' class='game-type' value='{0}'/>", input);
           }

           private string CancelTextFunc(object input)
           {
               var s = db.Syndicates.FirstOrDefault(x => x.ID == (int)input);
               if (s == null || s.IsPrizeDistributed) return "";
               if (s.Canceled == 1)
                   return "<b class='canceled'>Отменен</b>";
               if (s.Canceled == 2)
                   return "<b class='canceled'>Не состоялся</b>";
               return string.Format("<a href='#' arg='{0}' class='synd-cancel'>Отменить</a>", input);
           }

           private string CreateNum(object input)
           {
               return (input.ToTypedValue<int>()).ToString("d8");
           }

           private string SaveSyndicate(object syndicate)
           {
               try
               {
                   if (syndicate == null) return "Объект не найден";
                   var tBet = (Models.Syndicate)syndicate;

                   var exist = db.Syndicates.FirstOrDefault(x => x.ID == tBet.ID);

                   if (tBet.ID == 0)
                   {
                       db.Syndicates.InsertOnSubmit(tBet);
                   }
                   else
                   {
                       db.Refresh(RefreshMode.KeepCurrentValues, tBet);
                   }
                   if (!exist.IsPrizeDistributed && tBet.WinnerCombination.IsFilled() && tBet.OverageWin.HasValue)
                   {
                       exist.DistributePrize(tBet.OverageWin.Value, tBet.Combination);
                       tBet.IsPrizeDistributed = true;
                   }
                   db.SubmitChanges();
                   return "";
               }
               catch (Exception e)
               {
                   return e.Message;
               }
           }



           /////////////////////////////
           ///  Х  Х   У  У   Й  /Й  ///
           ///   Х      У    Й / Й   ///
           /// Х  Х    У    Й/  Й    ///
           /// /////////////////////////
           [AuthorizeMaster]
           public ActionResult WithdrawalEditor(string Type, int? Page, int? UID)
           {
               var type = (CurrentEditorType)Enum.Parse(typeof(CurrentEditorType), Type ?? "List", true);
               var er = type == CurrentEditorType.List
                            ? null
                            : (db.MoneyWithdrawals.FirstOrDefault(x => x.ID == UID) ?? new MoneyWithdrawal());

               var typeDataSource = new UniversalDataSource()
               {
                   DefValue = 0,
                   HasEmptyDef = false,

                   KeyField = "Key",
                   ValueField = "Value",
                   Source = new List<KeyValuePair<int, string>>()
                       {
                           {new KeyValuePair<int, string>(0, "В ожидании")},
                           {new KeyValuePair<int, string>(-1, "Отмененные")},
                           {new KeyValuePair<int, string>(1, "Завершенные")}
                       }
               };
               var settings = new UniversalEditorSettings()
                   {
                       AutoFilter = true,
                       TableName = "MoneyWithdrawals",
                       HasDeleteColumn = false,
                       CanAddNew = false,
                       UIDColumnName = "ID",
                       ShowedFieldsInList =
                           new List<UniversalListField>()
                               {
                                   new UniversalListField()
                                       {
                                           FieldName = "ID",
                                           IsLinkToEdit = true,
                                           HeaderText = "Номер заявки",
                                           TextFunction = x => ((int) x).ToString("d10")
                                       },
                                   new UniversalListField()
                                       {
                                           FieldName = "UserID",
                                           HeaderText = "Пользователь",
                                           TextFunction = val => string.Format("<a href='{0}'>{1}</a>",
                                                                               Url.Action("Edit", "Users",
                                                                                          new {user = val}),
                                                                               Membership.GetUser((Guid) val)
                                                                                         .GetProfile()
                                                                                         .FullName)
                                       },
                                   new UniversalListField()
                                       {
                                           FieldName = "Sum",
                                           HeaderText = "Сумма"
                                       },
                                   new UniversalListField()
                                       {
                                           FieldName = "RequestDate",
                                           HeaderText = "Дата заявки"
                                       },
                                   new UniversalListField()
                                       {
                                           FieldName = "TransactionID",
                                           HeaderText = "Номер транзакции"
                                       }
                               },


                       EditedFieldsList =
                           new List<UniversalEditorField>()
                               {
                                   new UniversalEditorField()
                                       {
                                           FieldName = "Status",
                                           FieldType = UniversalEditorFieldType.DropDown,
                                           DataType = typeof (int),
                                        
                                           HeaderText = "Статус",
                                           Modificators =
                                               new List<IUniversalFieldModificator>() {new RequiredModificator()},
                                           InnerListDataSource = typeDataSource
                                       },
                                   new UniversalEditorField()
                                       {
                                           FieldName = "Sum",
                                           FieldType = UniversalEditorFieldType.TextBox,
                                           DataType = typeof (decimal),
                                           HeaderText = "Сумма для снятия",
                                           Modificators =
                                               new List<IUniversalFieldModificator>()
                                                   {
                                                       new RequiredModificator(),
                                                       new RangeModificator(0, SiteSetting.Get<int>("MaxWithdrawal")+1)
                                                   }
                                       },
                                   new UniversalEditorField()
                                       {
                                           FieldName = "BankName",
                                           FieldType = UniversalEditorFieldType.TextBox,
                                           HeaderText = "Наименование и номер отделения банка",
                                           Modificators =
                                               new List<IUniversalFieldModificator>() {new RequiredModificator()}

                                       },
                                   new UniversalEditorField()
                                       {
                                           FieldName = "BankKorr",
                                           FieldType = UniversalEditorFieldType.TextBox,
                                           HeaderText = "Корреспондентский счет отделения банка",
                                           Modificators =
                                               new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                       },
                                   new UniversalEditorField()
                                       {
                                           FieldName = "BankBik",
                                           FieldType = UniversalEditorFieldType.TextBox,
                                           HeaderText = "БИК банка",
                                           Modificators =
                                               new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                       },
                                   new UniversalEditorField()
                                       {
                                           FieldName = "BankINN",
                                           FieldType = UniversalEditorFieldType.TextBox,
                                           HeaderText = "ИНН банка",
                                           Modificators =
                                               new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                       },
                                   new UniversalEditorField()
                                       {
                                           FieldName = "BankKPP",
                                           FieldType = UniversalEditorFieldType.TextBox,
                                           HeaderText = "КПП банка",
                                           Modificators =
                                               new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                       },
                                   new UniversalEditorField()
                                       {
                                           FieldName = "BankAccount",
                                           FieldType = UniversalEditorFieldType.TextBox,
                                           HeaderText = "Расчетный счет отделения банка для перечисления выигрыша"
                                       },
                                   new UniversalEditorField()
                                       {
                                           FieldName = "Comment",
                                           FieldType = UniversalEditorFieldType.TextBox,
                                           HeaderText =
                                               "Правильная формулировка о назначении платежа для отделения банка получателя",
                                           Modificators =
                                               new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                       },
                                   new UniversalEditorField()
                                       {
                                           FieldName = "UserAccount",
                                           FieldType = UniversalEditorFieldType.TextBox,
                                           HeaderText =
                                               "Номер лицевого счета, либо номер счета пластиковой карты для зачисления выигрыша",
                                           Modificators =
                                               new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                       },
                                   new UniversalEditorField()
                                       {
                                           FieldName = "RequestDate",
                                           FieldType = UniversalEditorFieldType.Calendar,
                                           HeaderText = "Дата подачи заявки",
                                           Modificators =
                                               new List<IUniversalFieldModificator>() {new RequiredModificator()}
                                       },
                                   new UniversalEditorField()
                                       {
                                           FieldName = "CloseDate",
                                           FieldType = UniversalEditorFieldType.Calendar,
                                           HeaderText = "Дата закрытия заявки"
                                       },
                                   new UniversalEditorField()
                                       {
                                           FieldName = "TransactionID",
                                           FieldType = UniversalEditorFieldType.Label,
                                           HeaderText = "Номер транзакции списания денег в системе",
                                           DataType = typeof (int?),
                                           TextFunction = x => ((int?) x).HasValue ? x.ToString() : "&lt;NOT ASSIGNED&gt;"
                                       }

                               }

                   };


               var filters = new List<FilterConfiguration>()
                   {
                       new FilterConfiguration()
                           {
                               FilterSource = typeDataSource,
                               IsDropDown = true,
                               QueryKey = "Status",
                               HeaderText = "Статус заявки",
                               Type = FilterType.Integer
                           }
                   };
               var data = new UniversalEditorPagedData()
               {
                   PagedData =
                       type == CurrentEditorType.List
                           ? new PagedData<MoneyWithdrawal>(db.MoneyWithdrawals.Where(x => x.Status == Request["Status"].ToInt()).OrderByDescending(x => x.RequestDate), Page ?? 0, 15, "Master",
                                                    filters)
                           : null,
                   Settings = settings,
                   CurrentType = type,
                   EditedRow = er

               };
               data.Settings.Filters = filters;
               data.IsAddingNew = data.EditedRow != null && ((MoneyWithdrawal)data.EditedRow).ID == 0;
               data.CallerController = "TableEditors";
               data.CallerAction = "WithdrawalEditor";
               data.SaveRow = SaveWithdrawal;
               data.EditorName = "Управление заявками на вывод средств";


               data.AddView = new UniversalEditorAddViewInfo()
                   {
                       Action = "CancelWithdrawal",
                       Controller = "Cabinet",
                       InEditor = true,
                       Routes = new RouteValueDictionary(
                           new
                               {
                                   RedirectURL = "/Master/ru/TableEditors/WithdrawalEditor",
                                   ID = ((MoneyWithdrawal)data.EditedRow ?? new MoneyWithdrawal()).ID
                               })

                   };
               return View("TableEditor", data);
           }

           private string SaveWithdrawal(object bet)
           {
               try
               {
                   if (bet == null) return "Объект не найден";
                   var tBet = (MoneyWithdrawal)bet;

                   if (tBet.ID == 0)
                   {
                       db.MoneyWithdrawals.InsertOnSubmit((MoneyWithdrawal)bet);
                   }
                   else
                   {
                       //db.GameBets.Attach(tBet);
                       db.Refresh(RefreshMode.KeepCurrentValues, tBet);
                   }
                   db.SubmitChanges();
                   return "";
               }
               catch (Exception e)
               {
                   return e.Message;
               }
           }
    */

    }
}