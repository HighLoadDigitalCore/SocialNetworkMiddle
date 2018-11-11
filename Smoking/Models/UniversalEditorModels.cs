using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Linq;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using System.Web.Mvc.Ajax;
using System.Web.Routing;
using Smoking.Extensions;

namespace Smoking.Models
{
    public delegate string TextFunctionDelegate(object input);
    public delegate string SaveFunctionDelegate(object row);
    public delegate object BeforeSaveFunctionDelegate(object row);

    public class UniversalDataSource
    {
        public object Source { get; set; }
        public string KeyField { get; set; }
        public string ValueField { get; set; }
        public object DefValue { get; set; }
        public bool HasEmptyDef { get; set; }

        public TextFunctionDelegate TextFunction { get; set; }

        public object CalculatedDef(string fieldName, UniversalEditorPagedData model)
        {
            if (model.EditedRow != null && model.Settings.Filters != null && (int)model.EditedRow.GetPropertyValue(model.Settings.UIDColumnName) == 0)
            {
                var filter = model.Settings.Filters.FirstOrDefault(x => x.QueryKey == fieldName);
                if (filter != null && filter.ValueFromQuery.ToString().IsFilled())
                {
                    return filter.ValueFromQuery;
                }
            }
            return HasEmptyDef ? null : DefValue;
        }
    }

    public class UniversalEditorAddViewInfo
    {
        public string Action { get; set; }
        public string Controller { get; set; }
        public RouteValueDictionary Routes { get; set; }

        public bool InEditor { get; set; }
    }

    public class UniversalEditorPagedData
    {
        public string[] AddQueryParams { get; set; }
        public string AddQueryParamsJoin
        {
            get { return (AddQueryParams ?? new string[] { }).JoinToString("&"); }
            set { AddQueryParams = value.Split<string>("&").ToArray(); }
        }
        public string EditorName { get; set; }
        public SaveFunctionDelegate SaveRow { get; set; }

        public object PagedData { get; set; }
        public UniversalEditorSettings Settings { get; set; }
        public CurrentEditorType CurrentType { get; set; }
        public object EditedRow { get; set; }
        public bool IsAddingNew { get; set; }
        public string CallerController { get; set; }
        public string CallerAction { get; set; }
        public UniversalEditorAddViewInfo AddView { get; set; }


        public int Page
        {
            get
            {
                return PagedData != null ? (int)PagedData.GetPropertyValue("PageIndex") : HttpContext.Current.Request.QueryString["Page"].ToInt();
            }
        }

        public string ErrorList
        {
            get
            {
                if (EditedRow == null)
                    return "";

                var errors = new List<string>();
                foreach (var field in Settings.EditedFieldsList)
                {
                    errors.AddRange(field.Modificators.Select(modificator => modificator.CheckField(field, EditedRow.GetPropertyValue(field.FieldName))).Where(e => e.IsFilled()));
                }
                return errors.JoinToString("<br/>");
            }
        }

        public string RedirectURL { get; set; }

        public string[] FilterParams
        {
            get
            {
                if (Settings.Filters == null || !Settings.Filters.Any())
                    return new string[] { };
                return Settings.Filters.Select(x => x.QueryKey).ToArray();
            }
        }

        public bool HasFileUpload
        {
            get { return Settings != null && Settings.EditedFieldsList.Any(x => x.FieldType == UniversalEditorFieldType.DBImageUpload); }
        }

        public RouteValueDictionary JoinRoutes(object routes)
        {
            var dict = new RouteValueDictionary(routes);
            var fr = FilterParams.Where(x => HttpContext.Current.Request.QueryString[x].IsFilled());
            foreach (var r in fr)
            {
                dict.Add(r, HttpUtility.UrlDecode(HttpContext.Current.Request.QueryString[r]));
            }
            return dict;

        }

        public string GetImageWrapper(string fieldName, string columnName = "ID")
        {
            if (this.Settings == null || EditedRow == null)
            {
                return "";
            }

            return
                string.Format("/Master/ru/UniversalEditor/Image?tableName={0}&uidName={1}&uidValue={2}&fieldName={3}",
                              Settings.TableName, columnName, EditedRow.GetPropertyValue(columnName), fieldName);

        }

        public static string GetImageWrapper(string tableName, string columnName, string columnValue, string fieldName)
        {
            return
                string.Format("/Master/ru/UniversalEditor/Image?tableName={0}&uidName={1}&uidValue={2}&fieldName={3}",
                              tableName, columnName, columnValue, fieldName);

        }

        public bool IsNullImage(string fieldName)
        {
            if (EditedRow == null) return true;
            var data = (Binary)EditedRow.GetPropertyValue(fieldName);
            return (data == null || data.Length == 0);
        }

        public string GetDeleteWrapper(string fieldName, string columnName = "ID")
        {
            if (this.Settings == null || EditedRow == null)
            {
                return "";
            }

            return
                string.Format("/Master/ru/UniversalEditor/ClearImage?tableName={0}&uidName={1}&uidValue={2}&fieldName={3}",
                              Settings.TableName, columnName, EditedRow.GetPropertyValue(columnName), fieldName);

        }
    }

    public class UniversalEditorSettings
    {
        public List<UniversalListField> ShowedFieldsInList { get; set; }
        public bool HasDeleteColumn { get; set; }
        public string UIDColumnName { get; set; }
        public List<UniversalEditorField> EditedFieldsList { get; set; }
        public List<FilterConfiguration> Filters { get; set; }
        public bool AutoFilter { get; set; }
        public bool CanAddNew { get; set; }
        public string TableName { get; set; }
        public BeforeSaveFunctionDelegate OnUniversalSaving { get; set; }

        public string UniversalTableSaver(object obj, UniversalEditorSettings settings)
        {
            try
            {
                var db = new DB();
                if (obj == null) return "Объект не найден";
                var table = (ITable)db.GetType()
                           .GetProperty(settings.TableName)
                           .GetValue(db, null);

                if ((int)obj.GetPropertyValue(settings.UIDColumnName) == 0)
                {

                    if (settings.OnUniversalSaving != null)
                    {
                        obj = settings.OnUniversalSaving(obj);
                    }
                    table.InsertOnSubmit(obj);
                }
                else
                {
                    try
                    {
                        //obj.Detach();
                        //table.Attach(obj, true);
                        db.Refresh(RefreshMode.KeepChanges, obj);
                    }
                    catch (Exception e)
                    {
                        object entry =
                            Enumerable.Cast<object>(table)
                                      .FirstOrDefault(
                                          item =>
                                          (int) item.GetPropertyValue(settings.UIDColumnName) ==
                                          (int) obj.GetPropertyValue(settings.UIDColumnName));
                        if (entry != null)
                        {
                            obj.Detach();
                            entry.LoadPossibleProperties(obj, new string[] {settings.UIDColumnName});
                            db.Refresh(RefreshMode.KeepChanges);
                        }


                    }
                    //catch (Exception e)
                    {

                        /*
                                                obj.GetType().GetMethod("Initialize", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(this, null);

                                                var ndb = new DB();
                                                obj.ToString()|
                        */

                    }
                }
                db.SubmitChanges();
                return "";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }


        public static string DefaultTextChecker(object input)
        {
            if (input is bool)
                return (bool)input ? "Да" : "Нет";

            if (input is DateTime)
                return ((DateTime)input).ToString("d MMMM yyyy, HH:mm");

            if (input == null) return "<noname>";

            return input.ToString().IsNullOrEmpty() ? "<noname>" : input.ToString();
        }
        public static string DefaultTextCheckerNoReplace(object input)
        {
            if (input is bool)
                return (bool)input ? "Да" : "Нет";

            if (input is DateTime)
                return ((DateTime)input).ToString("d MMMM yyyy, HH:mm");

            if (input == null) return "";

            return input.ToString().IsNullOrEmpty() ? "" : input.ToString();
        }

    }

    public enum FilterType
    {
        Text,
        Integer,
        Date
    }

    public enum TextComparators
    {
        Contains,
        Equals
    }
    public enum NumericComparators
    {
        Less,
        LessOrEqual,
        Equal,
        GreaterOrEqual,
        Greater
    }
    public enum DateComparators
    {
        [Display(Name = "<")]
        Less,

        [Display(Name = "<=")]
        LessOrEqual,

        [Display(Name = "==")]
        Equal,

        [Display(Name = ">=")]
        GreaterOrEqual,

        [Display(Name = ">")]
        Greater,

        [Display(Name = "Range")]
        InRange
    }
    public class FilterConfiguration
    {
        public UniversalDataSource FilterSource { get; set; }
        public bool IsDropDown { get; set; }
        public string QueryKey { get; set; }
        public string HeaderText { get; set; }
        public FilterType Type { get; set; }
        public bool SkipInQuery { get; set; }
        public bool MainFilter { get; set; }
        public object ValueFromQuery
        {
            get
            {
                var q = HttpUtility.UrlDecode(HttpContext.Current.Request[QueryKey]);
                return string.IsNullOrEmpty(q) ? FilterSource.DefValue : q;
            }
        }



        protected Expression BuildExpressionDate(MemberExpression property, DateComparators comparator)
        {
            if (ValueFromQuery == null || ValueFromQuery.ToString().IsNullOrEmpty())
            {
                PropertyInfo p = typeof(DateTime?).GetProperty("HasValue");
                Expression nullValue = Expression.Constant(false, p.PropertyType);
                return Expression.Equal(property, nullValue);
            }

            DateTime? dValue = null;
            DateTime temp;
            if (
                DateTime.TryParseExact(ValueFromQuery.ToString(), "yyyy.MM.ddTHH:mm:ss.fff", CultureInfo.CurrentCulture,
                                        DateTimeStyles.None, out temp))
                dValue = temp;

            DateTime? secondValue = null;

            Expression searchExpression1 = null;
            Expression searchExpression2 = null;

            switch (comparator)
            {
                case DateComparators.Less:
                    searchExpression1 = Expression.LessThan(property, Expression.Constant(dValue));
                    break;
                case DateComparators.LessOrEqual:
                    searchExpression1 = Expression.LessThanOrEqual(property, Expression.Constant(dValue));
                    break;
                case DateComparators.Equal:
                    searchExpression1 = Expression.Equal(property, Expression.Constant(dValue));
                    break;
                case DateComparators.GreaterOrEqual:
                case DateComparators.InRange:
                    searchExpression1 = Expression.GreaterThanOrEqual(property, Expression.Constant(dValue));
                    break;
                case DateComparators.Greater:
                    searchExpression1 = Expression.GreaterThan(property, Expression.Constant(dValue));
                    break;
                default:
                    searchExpression1 = null;
                    break;
            }



            if (comparator == DateComparators.InRange && secondValue.HasValue)
            {
                searchExpression2 = Expression.LessThanOrEqual(property, Expression.Constant(secondValue.Value));
            }

            if (searchExpression1 == null && searchExpression2 == null)
            {
                return null;
            }
            else if (searchExpression1 != null && searchExpression2 != null)
            {
                var combinedExpression = Expression.AndAlso(searchExpression1, searchExpression2);
                return combinedExpression;
            }
            else if (searchExpression1 != null)
            {
                return searchExpression1;
            }
            else
            {
                return searchExpression2;
            }
        }
        protected Expression BuildExpressionNumeric(MemberExpression property, NumericComparators comparator)
        {

            if (ValueFromQuery == null || ValueFromQuery.ToString().IsNullOrEmpty())
            {
                return null;
            }

            var iValue = ValueFromQuery.ToTypedValue<int?>();
            if (!iValue.HasValue) return null;
            switch (comparator)
            {
                case NumericComparators.Less:
                    return Expression.LessThan(property, Expression.Constant(iValue.Value));
                case NumericComparators.LessOrEqual:
                    return Expression.LessThanOrEqual(property, Expression.Constant(iValue.Value));
                case NumericComparators.Equal:
                    return Expression.Equal(property, Expression.Constant(iValue.Value));
                case NumericComparators.GreaterOrEqual:
                    return Expression.GreaterThanOrEqual(property, Expression.Constant(iValue.Value));
                case NumericComparators.Greater:
                    return Expression.GreaterThan(property, Expression.Constant(iValue.Value));
                default:
                    throw new InvalidOperationException("Comparator not supported.");
            }
        }


        protected Expression BuildExpressionText(MemberExpression property, TextComparators comparator)
        {
            if (ValueFromQuery == null)
            {
                return null;
            }

            var searchExpression = Expression.Call(
                property,
                typeof(string).GetMethod(comparator.ToString(), new[] { typeof(string) }),
                Expression.Constant(ValueFromQuery));

            return searchExpression;
        }


        private Expression<Func<T, bool>> CreatePredicateWithNullCheck<T>(Expression searchExpression, ParameterExpression arg, MemberExpression targetProperty)
        {
            string[] parts = QueryKey.Split('.');

            Expression nullCheckExpression = null;
            if (parts.Length > 1)
            {
                MemberExpression property = Expression.Property(arg, parts[0]);
                nullCheckExpression = Expression.NotEqual(property, Expression.Constant(null));

                for (int i = 1; i < parts.Length - 1; i++)
                {
                    property = Expression.Property(property, parts[i]);
                    Expression innerNullCheckExpression = Expression.NotEqual(property, Expression.Constant(null));

                    nullCheckExpression = Expression.AndAlso(nullCheckExpression, innerNullCheckExpression);
                }
            }

            if (!targetProperty.Type.IsValueType || (targetProperty.Type.IsGenericType && targetProperty.Type.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                var innerNullCheckExpression = Expression.NotEqual(targetProperty, Expression.Constant(null));

                if (nullCheckExpression == null)
                {
                    nullCheckExpression = innerNullCheckExpression;
                }
                else
                {
                    nullCheckExpression = Expression.AndAlso(nullCheckExpression, innerNullCheckExpression);
                }
            }

            if (nullCheckExpression == null || ValueFromQuery == null)
            {
                return Expression.Lambda<Func<T, bool>>(searchExpression, arg);
            }
            else
            {
                var combinedExpression = Expression.AndAlso(nullCheckExpression, searchExpression);

                var predicate = Expression.Lambda<Func<T, bool>>(combinedExpression, arg);

                return predicate;
            }
        }



        protected Expression BuildExpression(MemberExpression property)
        {
            if (Type == FilterType.Integer)
                return BuildExpressionNumeric(property, NumericComparators.Equal);
            if (Type == FilterType.Text)
                return BuildExpressionText(property, TextComparators.Equals);
            if (Type == FilterType.Date)
                return BuildExpressionDate(property, DateComparators.Equal);
            return null;
        }

        public IQueryable<T> ApplyToQuery<T>(IQueryable<T> query)
        {
            //var arg = Expression.Parameter(typeof(T), "p");
            var arg = Expression.Parameter(typeof(T), "p");
            var property = GetPropertyAccess(arg);

            Expression searchExpression = null;

            if (property.Type.IsGenericType && property.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (ValueFromQuery == null)
                    searchExpression = BuildExpression(Expression.Property(property, "HasValue"));
                else searchExpression = BuildExpression(Expression.Property(property, "Value"));
            }
            else
            {
                searchExpression = BuildExpression(property);
            }

            if (searchExpression == null)
            {
                return query;
            }
            else
            {
                var predicate = CreatePredicateWithNullCheck<T>(searchExpression, arg, property);
                return query.Where(predicate);
            }
        }

        private MemberExpression GetPropertyAccess(ParameterExpression arg)
        {
            string[] parts = QueryKey.Split('.');

            MemberExpression property = Expression.Property(arg, parts[0]);

            for (int i = 1; i < parts.Length; i++)
            {
                property = Expression.Property(property, parts[i]);
            }

            return property;
        }


    }


    public class UniversalField
    {
        public string FieldName { get; set; }
        public string HeaderText { get; set; }
        public TextFunctionDelegate TextFunction { get; set; }
        public string Template { get; set; }
        public string CheckedText(object input)
        {
            return TextFunction == null ? UniversalEditorSettings.DefaultTextChecker(input) : TextFunction(input);
        }
    }


    public class UniversalListField : UniversalField
    {
        public bool IsLinkToEdit { get; set; }
        public bool IsOrderColumn { get; set; }
        public int? Width { get; set; }
    }

    public class UniversalEditorField : UniversalField
    {
        public UniversalEditorField()
        {
            FieldType = UniversalEditorFieldType.TextBox;
            DataType = typeof(string);
            Modificators = new List<IUniversalFieldModificator>();
        }
        public UniversalDataSource InnerListDataSource { get; set; }
        public bool ReadOnly { get; set; }
        public UniversalEditorFieldType FieldType { get; set; }
        public Type DataType { get; set; }
        public List<IUniversalFieldModificator> Modificators { get; set; }

        public bool Hidden { get; set; }
    }


    public class RequiredModificator : IUniversalFieldModificator
    {
        public string CheckField(UniversalEditorField field, object value)
        {
            var errText = "Поле \"" + field.HeaderText + "\" обязательно для заполнения";
            if (field.FieldType == UniversalEditorFieldType.DBImageUpload)
            {
                var rq = HttpContext.Current.Request[field.FieldName + "_Path"];

                if (rq.IsNullOrEmpty() && value == null)
                    return errText;
                return "";
            }
            var obj = value.ToTypedObject(field.DataType);
            if (obj == null || (obj is DateTime && (DateTime)obj == DateTime.MinValue))
                return errText;
            if (obj is string && ((string)obj).IsNullOrEmpty()) return errText;
            return "";
        }
    }

    public class RangeModificator : IUniversalFieldModificator
    {
        public decimal MinVal { get; set; }
        public decimal MaxVal { get; set; }

        public RangeModificator(decimal minVal, decimal maxVal)
        {
            MinVal = minVal;
            MaxVal = maxVal;
        }

        public string CheckField(UniversalEditorField field, object value)
        {
            string errText = "Поле \"" + field.HeaderText + "\" должно быть ";
            if (MinVal != decimal.MinValue)
                errText += "больше " + MinVal.ToUniString();
            if (MinVal != decimal.MinValue && MaxVal != decimal.MaxValue)
                errText += " и ";
            if (MaxVal != decimal.MaxValue)
                errText += "меньше " + MaxVal.ToUniString();
            var obj = value.ToTypedObject(field.DataType);
            if (obj == null)
            {
                if (field.DataType == typeof(decimal))
                {
                    obj = (value ?? "").ToString().Replace(",", ".").ToTypedObject(field.DataType) ??
                          (value ?? "").ToString().Replace(".", ",").ToTypedObject(field.DataType);
                }
            }
            if (obj == null)
                return errText;
            if (obj is decimal)
            {
                if ((decimal)obj <= MinVal || (decimal)obj >= MaxVal)
                    return errText;
            }
            if (obj is int)
            {
                if ((int)obj <= MinVal || (int)obj >= MaxVal)
                    return errText;
            }
            return "";
        }
    }

    public interface IUniversalFieldModificator
    {
        string CheckField(UniversalEditorField field, object value);
    }


    public enum UniversalEditorFieldType
    {
        DropDown,
        TextBox,
        CheckBox,
        TextArea,
        TextEditor,
        Calendar,
        Label,
        DBImageUpload
    }

    public enum CurrentEditorType
    {
        List,
        Edit,
        Delete
    }
}