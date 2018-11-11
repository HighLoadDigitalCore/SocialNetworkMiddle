using System.Web;
using System.Web.Mvc;

namespace Smoking
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            var errorAttribute = new HandleErrorAttribute {View = "NotFound"};
            filters.Add(errorAttribute);
        }
    }
}