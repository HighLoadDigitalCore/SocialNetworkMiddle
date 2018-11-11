using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using Smoking.Extensions;
using Smoking.Models;

namespace Smoking.Models
{
    public partial class SiteSetting
    {

        public static bool Set(string key, object value)
        {
            try
            {
                var db = new DB();
                var setting = db.SiteSettings.FirstOrDefault(x => x.Setting == key);
                if (setting != null)
                {
                    setting.Value = value.ToString();
                }
                db.SubmitChanges();
                if (HttpContext.Current.Cache[key] != null)
                {
                    HttpContext.Current.Cache.Remove(key);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static T Get<T>(string key, bool skipCache = false)
        {
            if (!skipCache && HttpContext.Current.Cache[key] != null)
            {
                return (T)HttpContext.Current.Cache[key];
            }
            DB db = new DB();
            T obj = default(T);
            var setting = from x in db.SiteSettings
                          where x.Setting == key
                          select x;
            if (setting.Any())
            {
                try
                {
                    obj = (T)setting.First().oValue;
                }
                catch (Exception)
                {
                    obj = default(T);
                }

                if (key.IsFilled() && obj != null && !skipCache)
                    HttpContext.Current.Cache.Add(key, obj, null, DateTime.Now.AddMinutes(3), Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
            }
            return obj;
        }
        public object oValue
        {
            get
            {
                return Value.ToTypedObject(Type.GetType(this.ObjectType));
            }
        }
        public string TemplateName
        {
            get
            {
                return ObjectType.Replace("System.", "");
            }
        }

        public static void ExecuteFunc(string funcName)
        {
            Assembly assembly = Assembly.LoadFile(Assembly.GetExecutingAssembly().Location);
            var nameParts = funcName.Split<string>(new[] { "." }).ToList();
            Type type = assembly.GetType(nameParts.Take(nameParts.Count() - 1).JoinToString("."));
            if (type != null)
            {
                MethodInfo methodInfo = type.GetMethod(nameParts.Last());
                if (methodInfo != null)
                {

                    ParameterInfo[] parameters = methodInfo.GetParameters();
                    object classInstance = Activator.CreateInstance(type, null);
                    if (parameters.Length == 0)
                    {
                        methodInfo.Invoke(classInstance, null);
                    }
                }
            }
        }
        public static IEnumerable<SelectListItem> GetDataSource(string funcName, string selectedVal)
        {
            object result = null;
            Assembly assembly = Assembly.LoadFile(Assembly.GetExecutingAssembly().Location);
            var nameParts = funcName.Split<string>(new[] { "." }).ToList();
            Type type = assembly.GetType(nameParts.Take(nameParts.Count() - 1).JoinToString("."));
            if (type != null)
            {
                MethodInfo methodInfo = type.GetMethod(nameParts.Last());
                if (methodInfo != null)
                {

                    ParameterInfo[] parameters = methodInfo.GetParameters();
                    object classInstance = Activator.CreateInstance(type, null);
                    if (parameters.Length == 0)
                    {
                        result = methodInfo.Invoke(classInstance, null);
                    }
                    /*
                                        else
                                        {
                                            object[] parametersArray = new object[] { "Hello" };
                                            result = methodInfo.Invoke(classInstance, parametersArray);
                                        }
                    */
                }
            }

            if (result == null || !(result is List<KeyValuePair<string, string>>))
            {
                return new SelectList(new List<KeyValuePair<string, string>>());
            }
            var retList =
                (result as List<KeyValuePair<string, string>>).Select(
                    x => new SelectListItem() { Text = x.Value, Value = x.Key, Selected = x.Value == selectedVal })
                                                              .ToList();
            retList.Insert(0, new SelectListItem() { Text = "Не выбрано", Value = "0", Selected = selectedVal == "0" });
            return retList;
        }
    }

}