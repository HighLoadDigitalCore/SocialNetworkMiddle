using System.Web;
using System.Web.Optimization;

namespace Smoking
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {

            bundles.Add(new ScriptBundle("~/Content/client/header").Include(
                "~/Content/client/js/jquery-1.6.2.min.js",
                "~/Content/client/js/jquery.hoverIntent.minified.js",
                "~/Content/client/js/jquery.dcverticalmegamenu.1.3.js",
                "~/Content/client/js/jquery.cookie.js",
                "~/Content/client/js/script.js"
                            ));

            
                
            bundles.Add(new ScriptBundle("~/Content/client/footer").Include(
                "~/Content/client/js/jquery.simplemodal.js",
                "~/Content/client/js/basic.js",
                "~/Scripts/jquery.validate.min.js",
                "~/Scripts/jquery.unobtrusive-ajax.min.js",
                "~/Scripts/jquery.json-2.3.min.js"

                            ));


            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-1.9.1.min.js",
                        "~/Scripts/jquery-ui-1.9.2.min.js",
                        "~/Scripts/FileUpload/jquery.*"));

            bundles.Add(new ScriptBundle("~/bundles/FileUpload").Include(
                        "~/Scripts/FileUpload/jquery.*"));

            bundles.Add(new ScriptBundle("~/bundles/unobtrusive").Include(
                "~/Scripts/jquery.validate.min.js", "~/Scripts/jquery.validate.unobtrusive.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/ajax").Include(
                "~/Scripts/jquery.unobtrusive-ajax.min.js"));


            bundles.Add(new ScriptBundle("~/bundles/tree").Include(
                "~/Scripts/jquery.json*",
                "~/Scripts/jquery.cookie*",
                "~/Scripts/jquery.jstree*"
                ));            
            bundles.Add(new ScriptBundle("~/bundles/master").Include(
                "~/Scripts/master*"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-timepicker-addon.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
                        "~/Content/themes/base/jquery.ui.core.css",
                        "~/Content/themes/base/jquery.ui.resizable.css",
                        "~/Content/themes/base/jquery.ui.selectable.css",
                        "~/Content/themes/base/jquery.ui.accordion.css",
                        "~/Content/themes/base/jquery.ui.autocomplete.css",
                        "~/Content/themes/base/jquery.ui.button.css",
                        "~/Content/themes/base/jquery.ui.dialog.css",
                        "~/Content/themes/base/jquery.ui.slider.css",
                        "~/Content/themes/base/jquery.ui.tabs.css",
                        "~/Content/themes/base/jquery.ui.datepicker.css",
                        "~/Content/themes/base/jquery.ui.progressbar.css",
                        "~/Content/themes/base/jquery.ui.theme.css"));
        }
    }
}